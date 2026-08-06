using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using PyonPix.Config;
using PyonPix.Config.Global.Properties;
using PyonPix.Services.Core;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Structs.Pix.Properties;
using PyonPix.Shared.Structs.Renderer;
using PyonPix.Structs.Browser;
using PyonPix.Structs.Renderer;
using PyonPix.Ui;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace PyonPix.Services.Game;

public unsafe class RendererService(PyonPix.Config.Configuration config, IServiceContext services, IWindowContext windows) : BaseService(config, services, windows)
{
	private unsafe delegate void OMSetRenderTargetsDelegate(void* context, uint numViews, void** rtvArray, void* depthStencilView);

	private unsafe delegate int PresentDelegate(void* swapChain, uint syncInterval, uint flags);

	private Hook<OMSetRenderTargetsDelegate>? HookOMSetRenderTargets;

	private Hook<PresentDelegate>? HookPresent;

	private unsafe readonly Device* Device = Device.Instance();

	private DepthStencilView? DepthDsv;

	public ShaderResourceView? DepthSrv;

	public ShaderResourceView? RenderSrv;

	private RenderTargetView? RenderRtv;

	private ulong PresentIndex;

	private ulong LastPresentIndex;

	private bool SceneRendered;

	private BlendState BlendS;

	private VertexShader VS;

	private PixelShader PS;

	private SamplerState Sampler;

	private SharpDX.Direct3D11.Buffer ShaderParams;

	private Texture2D AvgTexture;

	private RenderTargetView AvgRTV;

	private Texture2D AvgStaging;

	private VertexShader AvgVS;

	private PixelShader AvgPS;

	public uint DsvAddr = 112u;

	public uint RtvAddr = 104u;

	private uint CompositionIndex;

	private PixService? PixService => Services.Get<PixService>();

	private DXService? DXService => Services.Get<DXService>();

	private StateService? StateService => Services.Get<StateService>();

	private BrowserService? BrowserService => Services.Get<BrowserService>();

	private LightService? LightService => Services.Get<LightService>();

	private PixInputService? PixInputService => Services.Get<PixInputService>();

	public Dictionary<string, Renderer> Renderers { get; private set; } = new Dictionary<string, Renderer>();

	public unsafe override async Task Initialize()
	{
		if (DXService != null && DXService.D3D11Device != null)
		{
			VS = await DXService.LoadShader<VertexShader>("vsmain");
			PS = await DXService.LoadShader<PixelShader>("psmain");
			Sampler = new SamplerState(DXService.D3D11Device, new SamplerStateDescription
			{
				Filter = Filter.MinMagMipLinear,
				AddressU = TextureAddressMode.Clamp,
				AddressV = TextureAddressMode.Clamp,
				AddressW = TextureAddressMode.Clamp
			});
			ShaderParams = new SharpDX.Direct3D11.Buffer(DXService.D3D11Device, Utilities.SizeOf<ShaderParams>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
			Texture2DDescription description = new Texture2DDescription
			{
				Width = 16,
				Height = 16,
				MipLevels = 1,
				ArraySize = 1,
				Format = Format.R8G8B8A8_UNorm,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
				BindFlags = (BindFlags.ShaderResource | BindFlags.RenderTarget)
			};
			AvgTexture = new Texture2D(DXService.D3D11Device, description);
			AvgRTV = new RenderTargetView(DXService.D3D11Device, AvgTexture);
			description.Usage = ResourceUsage.Staging;
			description.BindFlags = BindFlags.None;
			description.CpuAccessFlags = CpuAccessFlags.Read;
			AvgStaging = new Texture2D(DXService.D3D11Device, description);
			AvgVS = await DXService.LoadShader<VertexShader>("vsavg");
			AvgPS = await DXService.LoadShader<PixelShader>("psavg");
			nint num = *(nint*)((nint)(*(IntPtr*)DXService.D3D11Context.NativePointer) + (nint)33 * (nint)sizeof(nint));
			HookOMSetRenderTargets = Services.GameInteropProvider.HookFromAddress<OMSetRenderTargetsDelegate>((IntPtr)num, (OMSetRenderTargetsDelegate)OMSetRenderTargetsDetour, (HookBackend)0);
			HookOMSetRenderTargets.Enable();
			nint num2 = *(nint*)((nint)(*(IntPtr*)DXService.SwapChainPtr) + (nint)8 * (nint)sizeof(nint));
			HookPresent = Services.GameInteropProvider.HookFromAddress<PresentDelegate>((IntPtr)num2, (PresentDelegate)PresentDetour, (HookBackend)0);
			HookPresent.Enable();
			PixService? pixService = PixService;
			if (pixService != null)
			{
				pixService.PixSpawned += OnPixSpawned;
			}
			PixService? pixService2 = PixService;
			if (pixService2 != null)
			{
				pixService2.PixUpdated += OnPixUpdated;
			}
			PixService? pixService3 = PixService;
			if (pixService3 != null)
			{
				pixService3.PixDespawned += OnPixDespawned;
			}
			PixService? pixService4 = PixService;
			if (pixService4 != null)
			{
				pixService4.AllPixDespawned += OnAllPixDespawned;
			}
		}
	}

	private void OnPixSpawned(IPix p, bool isUserAction)
	{
		if (p != null && (!Renderers.TryGetValue(p.Id, out Renderer value) || value == null))
		{
			value = new Renderer(p.Id);
			RebuildTransform(p, value);
			RebuildProperties(p, value);
			RebuildGlobalProperties(Config.Global.Renderer);
			Renderers[p.Id] = value;
			ClearViews();
		}
	}

	private void OnPixUpdated(PixUpdate u)
	{
		if (u.Pix != null && PixService != null && PixService.IsSpawned(u.Pix) && Renderers.TryGetValue(u.Pix.Id, out Renderer value) && value != null)
		{
			PixUpdateType type = u.Type;
			if (type == PixUpdateType.All || (uint)(type - 5) <= 1u)
			{
				RebuildTransform(u.Pix, value);
				RebuildProperties(u.Pix, value);
			}
		}
	}

	private void OnPixDespawned(IPix p, bool isUserAction)
	{
		if (p != null && Renderers.TryGetValue(p.Id, out Renderer value))
		{
			value?.Dispose();
			Renderers.Remove(p.Id);
			ClearViews();
		}
	}

	private void OnAllPixDespawned()
	{
		ClearViews();
	}

	private void RebuildTransform(IPix p, Renderer r)
	{
		RendererPixProperties renderer = p.Renderer;
		r.ScreenTransform = Matrix4x4.CreateScale(renderer.Scale) * Matrix4x4.CreateFromQuaternion(renderer.Rotation) * Matrix4x4.CreateTranslation(renderer.Position);
	}

	private void RebuildProperties(IPix p, Renderer r)
	{
		if (DXService != null)
		{
			RendererPixProperties renderer = p.Renderer;
			r.ScreenTint = renderer.ScreenTint;
			r.EdgeColour = renderer.EdgeColour;
			r.BackColour = renderer.BackColour;
			r.BorderColour = renderer.BorderColour;
			r.BorderWidthH = renderer.BorderWidthH;
			r.BorderWidthV = renderer.BorderWidthV;
			r.BorderMode = renderer.BorderMode;
			r.BorderFeather = renderer.BorderFeather;
			r.EdgeFeather = renderer.EdgeFeather;
			r.RasterizerState?.Dispose();
			r.RasterizerState = new RasterizerState(DXService.D3D11Device, new RasterizerStateDescription
			{
				FillMode = FillMode.Solid,
				CullMode = ((renderer.CullMode == PyonPix.Shared.Structs.Renderer.CullMode.Front) ? SharpDX.Direct3D11.CullMode.Back : ((renderer.CullMode != PyonPix.Shared.Structs.Renderer.CullMode.Back) ? SharpDX.Direct3D11.CullMode.None : SharpDX.Direct3D11.CullMode.Front))
			});
			r.DepthState?.Dispose();
			r.DepthState = new DepthStencilState(DXService.D3D11Device, new DepthStencilStateDescription
			{
				IsDepthEnabled = renderer.Depth,
				DepthWriteMask = (renderer.Depth ? DepthWriteMask.All : DepthWriteMask.Zero),
				DepthComparison = ((renderer.DepthComparison == DepthComparison.LessEqual) ? Comparison.GreaterEqual : Comparison.LessEqual),
				IsStencilEnabled = false
			});
			r.DepthOffset = renderer.DepthOffset;
		}
	}

	public void RebuildGlobalProperties(RendererGlobalProperties r)
	{
		if (DXService != null)
		{
			BlendS?.Dispose();
			BlendStateDescription description = new BlendStateDescription
			{
				AlphaToCoverageEnable = r.AlphaToCoverageEnable,
				IndependentBlendEnable = r.IndependentBlendEnable
			};
			description.RenderTarget[0] = new RenderTargetBlendDescription
			{
				IsBlendEnabled = r.IsBlendEnabled,
				SourceBlend = r.SourceBlend,
				DestinationBlend = r.DestinationBlend,
				BlendOperation = r.BlendOperation,
				SourceAlphaBlend = r.SourceAlphaBlend,
				DestinationAlphaBlend = r.DestinationAlphaBlend,
				AlphaBlendOperation = r.AlphaBlendOperation,
				RenderTargetWriteMask = r.RenderTargetWriteMask
			};
			BlendS = new BlendState(DXService.D3D11Device, description);
		}
	}

	public void ClearViews()
	{
		PresentIndex = 0uL;
		LastPresentIndex = 0uL;
		SceneRendered = false;
		DepthDsv = null;
		DepthSrv = null;
		RenderSrv = null;
		RenderRtv = null;
	}

	private unsafe DepthStencilView? GetOrCreateDSV(Texture* tex, ref DepthStencilView? dsv)
	{
		if (tex == null || ((Texture)tex).D3D11Texture2D == null)
		{
			return null;
		}
		dsv?.Dispose();
		dsv = null;
		nint d3D11Texture2D = (nint)((Texture)tex).D3D11Texture2D;
		try
		{
			Marshal.AddRef(d3D11Texture2D);
			dsv = new DepthStencilView(d3D11Texture2D);
			return dsv;
		}
		catch (Exception ex)
		{
			Services.Log.Warning("Failed to create DSV for RenderTargetManager texture: " + ex.Message, Array.Empty<object>());
			return null;
		}
	}

	private unsafe DepthStencilView? GetOrCreateDSV2(Texture* tex, ref DepthStencilView? dsv)
	{
		if (tex == null || ((Texture)tex).D3D11Texture2D == null)
		{
			return null;
		}
		dsv?.Dispose();
		dsv = null;
		nint d3D11Texture2D = (nint)((Texture)tex).D3D11Texture2D;
		try
		{
			Marshal.AddRef(d3D11Texture2D);
			using Texture2D texture2D = new Texture2D(d3D11Texture2D);
			Texture2DDescription description = texture2D.Description;
			SharpDX.Direct3D11.Device device = texture2D.Device;
			DepthStencilViewDescription description2 = new DepthStencilViewDescription
			{
				Format = GetDSVFormat(description.Format),
				Dimension = DepthStencilViewDimension.Texture2D,
				Texture2D = new DepthStencilViewDescription.Texture2DResource
				{
					MipSlice = 0
				}
			};
			dsv = new DepthStencilView(device, texture2D, description2);
			return dsv;
		}
		catch (Exception ex)
		{
			Services.Log.Warning("Failed to create DSV for RenderTargetManager texture: " + ex.Message, Array.Empty<object>());
			return null;
		}
	}

	private unsafe ShaderResourceView? GetOrCreateSRV(Texture* tex, ref ShaderResourceView? srv)
	{
		if (tex == null || ((Texture)tex).D3D11Texture2D == null)
		{
			return null;
		}
		srv?.Dispose();
		srv = null;
		nint d3D11Texture2D = (nint)((Texture)tex).D3D11Texture2D;
		try
		{
			Marshal.AddRef(d3D11Texture2D);
			using Texture2D texture2D = new Texture2D(d3D11Texture2D);
			Texture2DDescription description = texture2D.Description;
			SharpDX.Direct3D11.Device device = texture2D.Device;
			Format sRVFormat = GetSRVFormat(description.Format);
			if (sRVFormat != description.Format)
			{
				ShaderResourceViewDescription description2 = new ShaderResourceViewDescription
				{
					Format = sRVFormat,
					Dimension = ShaderResourceViewDimension.Texture2D,
					Texture2D = new ShaderResourceViewDescription.Texture2DResource
					{
						MostDetailedMip = 0,
						MipLevels = ((description.MipLevels == 0) ? 1 : description.MipLevels)
					}
				};
				srv = new ShaderResourceView(device, texture2D, description2);
			}
			else
			{
				srv = new ShaderResourceView(device, texture2D);
			}
			return srv;
		}
		catch (Exception ex)
		{
			Services.Log.Warning("Failed to create SRV for RenderTargetManager texture: " + ex.Message, Array.Empty<object>());
			return null;
		}
	}

	private unsafe RenderTargetView? GetOrCreateRTV(Texture* tex, ref RenderTargetView? rtv)
	{
		if (tex == null || ((Texture)tex).D3D11Texture2D == null)
		{
			return null;
		}
		rtv?.Dispose();
		rtv = null;
		nint d3D11Texture2D = (nint)((Texture)tex).D3D11Texture2D;
		try
		{
			Marshal.AddRef(d3D11Texture2D);
			using Texture2D texture2D = new Texture2D(d3D11Texture2D);
			Texture2DDescription description = texture2D.Description;
			SharpDX.Direct3D11.Device device = texture2D.Device;
			RenderTargetViewDescription description2 = new RenderTargetViewDescription
			{
				Format = GetRTVFormat(description.Format),
				Dimension = RenderTargetViewDimension.Texture2D,
				Texture2D = new RenderTargetViewDescription.Texture2DResource
				{
					MipSlice = 0
				}
			};
			rtv = new RenderTargetView(device, texture2D, description2);
			return rtv;
		}
		catch (Exception ex)
		{
			Services.Log.Warning("Failed to create RTV for RenderTargetManager texture: " + ex.Message, Array.Empty<object>());
			return null;
		}
	}

	private static Format GetDSVFormat(Format format)
	{
		return format switch
		{
			Format.R24G8_Typeless => Format.D24_UNorm_S8_UInt, 
			Format.R32_Typeless => Format.D32_Float, 
			Format.R32G8X24_Typeless => Format.D32_Float_S8X24_UInt, 
			_ => format, 
		};
	}

	private static Format GetSRVFormat(Format format)
	{
		return format switch
		{
			Format.R16_Typeless => Format.R16_UNorm, 
			Format.R24G8_Typeless => Format.R24_UNorm_X8_Typeless, 
			Format.R32_Typeless => Format.R32_Float, 
			Format.R32G8X24_Typeless => Format.R32_Float_X8X24_Typeless, 
			Format.R8G8B8A8_Typeless => Format.R8G8B8A8_UNorm, 
			Format.B8G8R8A8_Typeless => Format.B8G8R8A8_UNorm, 
			Format.R16G16B16A16_Typeless => Format.R16G16B16A16_Float, 
			Format.R32G32B32A32_Typeless => Format.R32G32B32A32_Float, 
			Format.R10G10B10A2_Typeless => Format.R10G10B10A2_UNorm, 
			_ => format, 
		};
	}

	private static Format GetRTVFormat(Format format)
	{
		return format switch
		{
			Format.R16G16B16A16_Typeless => Format.R16G16B16A16_Float, 
			Format.R8G8B8A8_Typeless => Format.R8G8B8A8_UNorm, 
			Format.B8G8R8A8_Typeless => Format.B8G8R8A8_UNorm, 
			Format.R10G10B10A2_Typeless => Format.R10G10B10A2_UNorm, 
			_ => format, 
		};
	}

	private bool IsDSVForTexture(nint dsvPtr, nint texturePtr)
	{
		if (dsvPtr == IntPtr.Zero || texturePtr == IntPtr.Zero)
		{
			return false;
		}
		try
		{
			return IsMatchingResource(dsvPtr, texturePtr);
		}
		catch
		{
			return false;
		}
	}

	private bool IsRTVForTexture(nint rtvPtr, nint texturePtr)
	{
		if (rtvPtr == IntPtr.Zero || texturePtr == IntPtr.Zero)
		{
			return false;
		}
		try
		{
			return IsMatchingResource(rtvPtr, texturePtr);
		}
		catch
		{
			return false;
		}
	}

	private unsafe static bool IsMatchingResource(nint viewPtr, nint texturePtr)
	{
		if (viewPtr == IntPtr.Zero)
		{
			return false;
		}
		nint* ptr = *(nint**)viewPtr;
		if (ptr == null)
		{
			return false;
		}
		delegate* unmanaged<void*, void**, void> delegate_002A = (delegate* unmanaged<void*, void**, void>)ptr[7];
		void* ptr2 = null;
		delegate_002A((void*)viewPtr, &ptr2);
		if (ptr2 == null)
		{
			return false;
		}
		nint num = (nint)ptr2;
		((delegate* unmanaged<void*, uint>)(*(IntPtr*)((nint)(*(IntPtr*)num) + (nint)2 * (nint)sizeof(nint))))((void*)num);
		return texturePtr == num;
	}

	private unsafe void OMSetRenderTargetsDetour(void* context, uint numViews, void** rtvArray, void* depthStencilView)
	{
		HookOMSetRenderTargets.Original(context, numViews, rtvArray, depthStencilView);
		if (StateService == null || !StateService.LocalPlayerExists)
		{
			ClearViews();
		}
		else
		{
			if (depthStencilView == (void*)IntPtr.Zero || numViews == 0 || rtvArray == null)
			{
				return;
			}
			RenderTargetManager* ptr = RenderTargetManager.Instance();
			if (ptr == null)
			{
				return;
			}
			Texture* tex = *(Texture**)((byte*)ptr + DsvAddr);
			Texture* tex2 = *(Texture**)((byte*)ptr + RtvAddr);
			GetOrCreateDSV(tex, ref DepthDsv);
			GetOrCreateSRV(tex2, ref RenderSrv);
			if (DepthDsv == null || RenderSrv == null)
			{
				return;
			}
			nint num = (IsDSVForTexture((nint)depthStencilView, DepthDsv.NativePointer) ? ((nint)depthStencilView) : IntPtr.Zero);
			nint num2 = IntPtr.Zero;
			for (int i = 0; i < numViews; i++)
			{
				nint num3 = (nint)rtvArray[i];
				if (num3 != IntPtr.Zero && IsRTVForTexture(num3, RenderSrv.Resource.NativePointer))
				{
					num2 = num3;
					break;
				}
			}
			SceneRendered = num != IntPtr.Zero && num2 != IntPtr.Zero;
			if (!SceneRendered)
			{
				return;
			}
			if (LastPresentIndex != PresentIndex && CompositionIndex == Config.Global.Renderer.CompositionIndex)
			{
				LastPresentIndex = PresentIndex;
				try
				{
					Marshal.AddRef(num);
					using DepthStencilView dsv = new DepthStencilView(num);
					Marshal.AddRef(num2);
					using RenderTargetView rtv = new RenderTargetView(num2);
					Draw(dsv, rtv);
					SceneRendered = false;
				}
				catch (Exception value)
				{
					Services.Log.Error($"Draw Failed: {value}", Array.Empty<object>());
				}
			}
			if (SceneRendered)
			{
				CompositionIndex++;
			}
		}
	}

	private unsafe int PresentDetour(void* swapChain, uint syncInterval, uint flags)
	{
		CompositionIndex = 0u;
		PresentIndex++;
		return HookPresent.Original(swapChain, syncInterval, flags);
	}

	private void Draw(DepthStencilView dsv, RenderTargetView rtv)
	{
		try
		{
			if (BrowserService == null || DXService == null)
			{
				return;
			}
			if (BrowserService.State == BrowserState.Stopping)
			{
				BrowserService.InvokeShutdown();
			}
			else
			{
				if (BrowserService.State != BrowserState.Running || DXService.D3D11Device == null || DXService.D3D11Context == null || DXService.DXGISwapChain == null || DXService.D3D11Device.DeviceRemovedReason != Result.Ok || DepthDsv == null || RenderSrv == null || Renderers.Count == 0 || BrowserService.Tabs.Count == 0)
				{
					return;
				}
				DeviceContext d3D11Context = DXService.D3D11Context;
				RawViewportF[] viewports = d3D11Context.Rasterizer.GetViewports<RawViewportF>();
				if (viewports == null || viewports.Length == 0)
				{
					return;
				}
				DepthStencilView depthStencilViewRef;
				RenderTargetView[] renderTargets = d3D11Context.OutputMerger.GetRenderTargets(1, out depthStencilViewRef);
				if (renderTargets == null || renderTargets.Length == 0)
				{
					return;
				}
				RasterizerState state = d3D11Context.Rasterizer.State;
				BlendState blendState = d3D11Context.OutputMerger.BlendState;
				DepthStencilState depthStencilState = d3D11Context.OutputMerger.DepthStencilState;
				VertexShader vertexShader = d3D11Context.VertexShader.Get();
				PixelShader pixelShader = d3D11Context.PixelShader.Get();
				InputLayout inputLayout = d3D11Context.InputAssembler.InputLayout;
				PrimitiveTopology primitiveTopology = d3D11Context.InputAssembler.PrimitiveTopology;
				try
				{
					Matrix4x4 camView = Matrix4x4.Transpose(CameraService.GetViewMatrix());
					Matrix4x4 camProj = Matrix4x4.Transpose(CameraService.GetProjectionMatrix());
					foreach (Renderer value3 in Renderers.Values)
					{
						if (BrowserService.Tabs.TryGetValue(value3.PixId, out Tab value) && value.SRV != null && DrawRenderer(d3D11Context, dsv, rtv, viewports[0], value3, value.SRV, camView, camProj))
						{
							PixInputService?.HandleRendererMouseInput(value3, value);
							System.Numerics.Vector3? screenAvg = ComputeLight(d3D11Context, value3, value.SRV);
							LightService?.UpdateById(value3.PixId, screenAvg);
						}
					}
					return;
				}
				finally
				{
					if (viewports != null && viewports.Length != 0)
					{
						d3D11Context.Rasterizer.SetViewports(viewports, viewports.Length);
					}
					d3D11Context.OutputMerger.SetRenderTargets(depthStencilViewRef, renderTargets);
					if (renderTargets != null && renderTargets.Length != 0)
					{
						renderTargets[0]?.Dispose();
					}
					depthStencilViewRef?.Dispose();
					d3D11Context.Rasterizer.State = state;
					state?.Dispose();
					d3D11Context.OutputMerger.SetBlendState(blendState);
					blendState?.Dispose();
					d3D11Context.OutputMerger.SetDepthStencilState(depthStencilState);
					depthStencilState?.Dispose();
					d3D11Context.VertexShader.Set(vertexShader);
					vertexShader?.Dispose();
					d3D11Context.PixelShader.Set(pixelShader);
					pixelShader?.Dispose();
					d3D11Context.InputAssembler.InputLayout = inputLayout;
					inputLayout?.Dispose();
					d3D11Context.InputAssembler.PrimitiveTopology = primitiveTopology;
				}
			}
		}
		catch (InvalidOperationException)
		{
		}
		catch (Exception value2)
		{
			Services.Log.Error($"Renderer Failed: {value2}", Array.Empty<object>());
		}
	}

	private unsafe bool DrawRenderer(DeviceContext ctx, DepthStencilView dsv, RenderTargetView rtv, RawViewportF viewport, Renderer r, ShaderResourceView srv, Matrix4x4 camView, Matrix4x4 camProj)
	{
		try
		{
			if (Device == null)
			{
				return false;
			}
			if (DepthDsv == null || RenderSrv == null || srv == null)
			{
				return false;
			}
			if (!r.ScreenTransform.HasValue)
			{
				return false;
			}
			ctx.OutputMerger.SetRenderTargets(dsv, rtv);
			ctx.Rasterizer.SetViewport(viewport);
			ctx.InputAssembler.InputLayout = null;
			ctx.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
			ctx.Rasterizer.State = r.RasterizerState;
			ctx.OutputMerger.SetBlendState(BlendS);
			ctx.OutputMerger.SetDepthStencilState(r.DepthState);
			ShaderParams data = new ShaderParams
			{
				CameraView = camView,
				CameraProjection = camProj,
				ScreenTransform = Matrix4x4.Transpose(r.ScreenTransform.Value),
				ScreenTint = r.ScreenTint,
				EdgeColour = r.EdgeColour,
				BackColour = r.BackColour,
				BorderColour = r.BorderColour,
				BorderWidthH = r.BorderWidthH,
				BorderWidthV = r.BorderWidthV,
				BorderMode = (int)r.BorderMode,
				BorderFeather = r.BorderFeather,
				EdgeFeather = r.EdgeFeather,
				DepthOffset = r.DepthOffset
			};
			ctx.UpdateSubresource(ref data, ShaderParams);
			ctx.VertexShader.Set(VS);
			ctx.VertexShader.SetConstantBuffer(0, ShaderParams);
			ctx.PixelShader.Set(PS);
			ctx.PixelShader.SetShaderResource(0, srv);
			ctx.PixelShader.SetSampler(0, Sampler);
			ctx.PixelShader.SetConstantBuffer(0, ShaderParams);
			ctx.Draw(36, 0);
			ctx.PixelShader.SetShaderResource(0, null);
		}
		catch (Exception value)
		{
			Services.Log.Error($"DrawRenderer Failed: {value}", Array.Empty<object>());
			return false;
		}
		return true;
	}

	private unsafe System.Numerics.Vector3? ComputeLight(DeviceContext ctx, Renderer r, ShaderResourceView srv)
	{
		if (srv == null || AvgRTV == null || AvgStaging == null)
		{
			return null;
		}
		RawViewportF[] viewports = ctx.Rasterizer.GetViewports<RawViewportF>();
		DepthStencilView depthStencilViewRef;
		RenderTargetView[] renderTargets = ctx.OutputMerger.GetRenderTargets(1, out depthStencilViewRef);
		RasterizerState state = ctx.Rasterizer.State;
		DepthStencilState depthStencilState = ctx.OutputMerger.DepthStencilState;
		VertexShader vertexShader = ctx.VertexShader.Get();
		PixelShader pixelShader = ctx.PixelShader.Get();
		try
		{
			ctx.OutputMerger.SetRenderTargets(null, AvgRTV);
			ctx.Rasterizer.SetViewport(new Viewport(0, 0, 16, 16));
			ctx.Rasterizer.State = null;
			ctx.OutputMerger.SetDepthStencilState(null);
			ctx.VertexShader.Set(AvgVS);
			ctx.PixelShader.Set(AvgPS);
			ctx.PixelShader.SetShaderResource(0, srv);
			ctx.PixelShader.SetSampler(0, Sampler);
			ctx.Draw(3, 0);
			ctx.PixelShader.SetShaderResource(0, null);
			ctx.CopyResource(AvgTexture, AvgStaging);
			DataBox dataBox = ctx.MapSubresource(AvgStaging, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
			_ = System.Numerics.Vector3.Zero;
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			byte* dataPointer = (byte*)dataBox.DataPointer;
			for (int i = 0; i < 16; i++)
			{
				byte* ptr = dataPointer + i * dataBox.RowPitch;
				for (int j = 0; j < 16; j++)
				{
					num += (float)(int)ptr[j * 4] / 255f;
					num2 += (float)(int)ptr[j * 4 + 1] / 255f;
					num3 += (float)(int)ptr[j * 4 + 2] / 255f;
				}
			}
			System.Numerics.Vector3 value = new System.Numerics.Vector3(num / 256f, num2 / 256f, num3 / 256f);
			ctx.UnmapSubresource(AvgStaging, 0);
			return value;
		}
		finally
		{
			if (viewports != null && viewports.Length != 0)
			{
				ctx.Rasterizer.SetViewports(viewports, viewports.Length);
			}
			ctx.OutputMerger.SetRenderTargets(depthStencilViewRef, renderTargets);
			if (renderTargets != null && renderTargets.Length != 0)
			{
				renderTargets[0]?.Dispose();
			}
			depthStencilViewRef?.Dispose();
			ctx.Rasterizer.State = state;
			state?.Dispose();
			ctx.OutputMerger.SetDepthStencilState(depthStencilState);
			depthStencilState?.Dispose();
			ctx.VertexShader.Set(vertexShader);
			vertexShader?.Dispose();
			ctx.PixelShader.Set(pixelShader);
			pixelShader?.Dispose();
		}
	}

	private void DespawnAll()
	{
		foreach (Renderer value in Renderers.Values)
		{
			value?.Dispose();
		}
		Renderers.Clear();
	}

	public override Task Dispose()
	{
		PixService? pixService = PixService;
		if (pixService != null)
		{
			pixService.PixSpawned -= OnPixSpawned;
		}
		PixService? pixService2 = PixService;
		if (pixService2 != null)
		{
			pixService2.PixUpdated -= OnPixUpdated;
		}
		PixService? pixService3 = PixService;
		if (pixService3 != null)
		{
			pixService3.PixDespawned -= OnPixDespawned;
		}
		PixService? pixService4 = PixService;
		if (pixService4 != null)
		{
			pixService4.AllPixDespawned -= OnAllPixDespawned;
		}
		HookOMSetRenderTargets?.Disable();
		HookOMSetRenderTargets?.Dispose();
		HookPresent?.Disable();
		HookPresent?.Dispose();
		DespawnAll();
		VS?.Dispose();
		PS?.Dispose();
		Sampler?.Dispose();
		ShaderParams?.Dispose();
		BlendS?.Dispose();
		return Task.CompletedTask;
	}
}
