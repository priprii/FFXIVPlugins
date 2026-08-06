using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Common.Math;
using PyonPix.Config;
using PyonPix.Config.Global.Properties;
using PyonPix.Services.Core;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Structs.Pix.Properties;
using PyonPix.Shared.Utility;
using PyonPix.Structs.Light;
using PyonPix.Ui;

namespace PyonPix.Services.Game;

public class LightService(Configuration config, IServiceContext services, IWindowContext windows) : BaseService(config, services, windows)
{
	private unsafe delegate SceneLight* SceneLightCtorDelegate(SceneLight* self);

	private unsafe delegate bool SceneLightInitializeDelegate(SceneLight* self);

	private unsafe delegate nint SceneLightSetupDelegate(SceneLight* self);

	private unsafe delegate void CleanupRenderDelegate(SceneLight* light);

	private unsafe delegate void DestructorDelegate(SceneLight* light, bool a2);

	[Signature("E8 ?? ?? ?? ?? 48 89 84 FB ?? ?? ?? ?? 48 85 C0 0F 84 ?? ?? ?? ?? 48 8B C8")]
	private SceneLightCtorDelegate _sceneLightCtor;

	[Signature("E8 ?? ?? ?? ?? 48 8B 94 FB ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ??")]
	private SceneLightInitializeDelegate _sceneLightInit;

	[Signature("F6 41 38 01")]
	private SceneLightSetupDelegate _sceneLightSpawn;

	private readonly Dictionary<string, Light> Lights = new Dictionary<string, Light>();

	private const int MaxHistorySamples = 64;

	private PixService PixService => Services.Get<PixService>();

	public override Task Initialize()
	{
		Services.GameInteropProvider.InitializeFromAttributes((object)this);
		PixService.PixSpawned += OnPixSpawned;
		PixService.PixUpdated += OnPixUpdated;
		PixService.PixDespawned += OnPixDespawned;
		PixService.AllPixDespawned += OnAllPixDespawned;
		return Task.CompletedTask;
	}

	private void OnPixSpawned(IPix pix, bool isUserAction)
	{
		Spawn(pix);
	}

	private void OnPixUpdated(PixUpdate u)
	{
		if (u.Pix == null || !PixService.IsSpawned(u.Pix))
		{
			return;
		}
		bool flag = Lights.ContainsKey(u.Pix.Id);
		bool enabled = u.Pix.Light.Enabled;
		PixUpdateType type = u.Type;
		if (type == PixUpdateType.All || type == PixUpdateType.RendererTransform || (uint)(type - 7) <= 1u)
		{
			if (enabled && !flag)
			{
				Spawn(u.Pix);
			}
			else if (!enabled && flag)
			{
				Despawn(u.Pix);
			}
			else if (flag)
			{
				Update(u.Pix);
			}
		}
	}

	private void OnPixDespawned(IPix pix, bool isUserAction)
	{
		Despawn(pix);
	}

	private void OnAllPixDespawned()
	{
	}

	private unsafe void Spawn(IPix p)
	{
		if (p.Light.Enabled && (!Lights.TryGetValue(p.Id, out var value) || value.Address == IntPtr.Zero))
		{
			SceneLight* ptr = ((IMemorySpace)IMemorySpace.GetDefaultSpace()).Malloc<SceneLight>(8uL);
			_sceneLightCtor(ptr);
			_sceneLightInit(ptr);
			_sceneLightSpawn(ptr);
			((long*)ptr)[7] |= 2L;
			Lights[p.Id] = new Light
			{
				Address = (nint)ptr,
				ScreenAverage = null,
				History = new Vector3[64],
				HistoryTicks = new long[64],
				HistoryCount = 0,
				HistoryIndex = 0,
				LastTimestamp = 0L
			};
			Update(p);
		}
	}

	public void UpdateById(string pixId, Vector3? screenAvg = null)
	{
		if (PixService.SpawnedPixs.TryGetValue(pixId, out IPix value))
		{
			Update(value, screenAvg);
		}
	}

	public unsafe void Update(IPix? p, Vector3? screenAvg = null)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if (p != null && p.Light.Enabled && Lights.TryGetValue(p.Id, out var value) && value.Address != IntPtr.Zero)
		{
			SceneLight* address = (SceneLight*)value.Address;
			LightPixProperties light = p.Light;
			if (screenAvg.HasValue)
			{
				Lights[p.Id] = ComputeTemporalAccumulation(value, screenAvg.Value);
			}
			Vector3 position = p.Renderer.Position;
			Quaternion rotation = p.Renderer.Rotation;
			Vector3 vector = Vector3.Transform(light.Position, rotation) + position;
			Quaternion quaternion = Quaternion.Normalize(Quaternion.Multiply(rotation, light.Rotation));
			Unsafe.Write(&((Transform)(&address->Transform)).Position, Vector3.op_Implicit(vector));
			Unsafe.Write(&((Transform)(&address->Transform)).Rotation, Quaternion.op_Implicit(quaternion));
			RenderLight* renderLight = address->RenderLight;
			if (renderLight != null)
			{
				renderLight->Flags = light.Flags;
				renderLight->LightType = light.LightType;
				renderLight->Transform = &address->Transform;
				renderLight->Color = CalculateColour(light, value.ScreenAverage);
				renderLight->Range = light.Range;
				renderLight->LightAngle = light.LightAngle;
				renderLight->FalloffType = light.FalloffType;
				renderLight->FalloffAngle = light.FalloffAngle;
				renderLight->Falloff = light.FalloffPower;
				renderLight->CharaShadowRange = light.ShadowRange;
				renderLight->ShadowNear = light.ShadowNear;
				renderLight->ShadowFar = light.ShadowFar;
			}
		}
	}

	private Light ComputeTemporalAccumulation(Light l, Vector3 sample)
	{
		LightGlobalProperties light = Config.Global.Light;
		float num = Math.Clamp(light.InfluenceSmoothing, 0f, 1f);
		float b = MathF.Max(0.01f, light.InfluenceSmoothingDuration);
		float num2 = MathUtil.Lerp(0f, b, num);
		long timestamp = Stopwatch.GetTimestamp();
		long num3 = (long)(num2 * (float)Stopwatch.Frequency);
		int num4 = l.HistoryIndex % 64;
		l.History[num4] = sample;
		l.HistoryTicks[num4] = timestamp;
		l.HistoryIndex = (num4 + 1) % 64;
		if (l.HistoryCount < 64)
		{
			l.HistoryCount++;
		}
		Vector3 vector;
		if (num <= 0f || num2 <= 0f)
		{
			vector = sample;
		}
		else
		{
			Vector3 zero = Vector3.Zero;
			int num5 = 0;
			long num6 = timestamp - num3;
			for (int i = 0; i < l.HistoryCount; i++)
			{
				int num7 = (num4 - 1 - i + 64) % 64;
				long num8 = l.HistoryTicks[num7];
				if (num8 != 0L)
				{
					if (num8 < num6)
					{
						break;
					}
					zero += l.History[num7];
					num5++;
				}
			}
			vector = ((num5 == 0) ? sample : (zero / num5));
		}
		float x = ((l.LastTimestamp == 0L) ? 0f : MathUtil.TicksToSeconds(timestamp - l.LastTimestamp));
		x = MathF.Min(x, 0.2f);
		float b2 = MathF.Max(0.1f, light.InfluenceSmoothingDuration);
		float num9 = MathUtil.Lerp(0.01f, b2, num);
		Vector3 value;
		if (x <= 0f || num <= 0f)
		{
			value = vector;
		}
		else
		{
			float x2 = 1f - MathF.Exp((0f - x) / num9);
			x2 = MathF.Max(x2, 0.0001f);
			value = Vector3.Lerp(l.ScreenAverage ?? vector, vector, x2);
		}
		l.ScreenAverage = value;
		l.LastTimestamp = timestamp;
		return l;
	}

	private ColorHDR CalculateColour(LightPixProperties props, Vector3? screenAverageLinear = null)
	{
		float num = props.Intensity;
		Vector3 value3;
		if (props.ScreenColourInfluence > 0f && screenAverageLinear.HasValue)
		{
			float amount = Math.Clamp(props.ScreenColourInfluence, 0f, 1f);
			Vector3 value = new Vector3(props.Colour.X, props.Colour.Y, props.Colour.Z);
			Vector3 value2 = screenAverageLinear.Value;
			value2 = new Vector3(MathF.Pow(value2.X, props.InfluenceGammaCurve), MathF.Pow(value2.Y, props.InfluenceGammaCurve), MathF.Pow(value2.Z, props.InfluenceGammaCurve));
			value2 *= props.InfluenceColourIntensity;
			value2 = Vector3.Min(value2, Vector3.One * 4f);
			value3 = Vector3.Lerp(value, value2, amount);
			num *= props.InfluenceBrightnessIntensity;
		}
		else
		{
			value3 = new Vector3(props.Colour.X, props.Colour.Y, props.Colour.Z);
		}
		return new ColorHDR(new Vector4(value3, props.Colour.W), num);
	}

	private unsafe void Despawn(IPix p)
	{
		if (Lights.TryGetValue(p.Id, out var l) && l.Address != IntPtr.Zero)
		{
			Lights.Remove(p.Id);
			Services.Framework.RunOnFrameworkThread((Action)delegate
			{
				InvokeDtor((SceneLight*)l.Address);
			});
		}
	}

	private unsafe void DespawnAll()
	{
		if (Services.Framework.IsFrameworkUnloading)
		{
			return;
		}
		Services.Framework.RunOnFrameworkThread((Action)delegate
		{
			foreach (KeyValuePair<string, Light> light in Lights)
			{
				if (light.Value.Address != IntPtr.Zero)
				{
					InvokeDtor((SceneLight*)light.Value.Address);
				}
			}
			Lights.Clear();
		});
	}

	private unsafe void InvokeDtor(SceneLight* light)
	{
		GetVirtualFunc<CleanupRenderDelegate>(light, 1)(light);
		GetVirtualFunc<DestructorDelegate>(light, 0)(light, a2: false);
	}

	private unsafe static T GetVirtualFunc<T>(SceneLight* light, int index)
	{
		return Marshal.GetDelegateForFunctionPointer<T>(light->_vf[index]);
	}

	public override Task Dispose()
	{
		PixService.PixSpawned -= OnPixSpawned;
		PixService.PixUpdated -= OnPixUpdated;
		PixService.PixDespawned -= OnPixDespawned;
		PixService.AllPixDespawned -= OnAllPixDespawned;
		DespawnAll();
		return Task.CompletedTask;
	}
}
