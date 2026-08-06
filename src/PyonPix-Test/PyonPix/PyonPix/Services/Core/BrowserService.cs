using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures.TextureWraps;
using PyonPix.Config;
using PyonPix.Config.Global.Properties;
using PyonPix.Config.Pix;
using PyonPix.Events;
using PyonPix.Ipc;
using PyonPix.Services.Game;
using PyonPix.Shared.Ipc;
using PyonPix.Shared.Structs.Browser;
using PyonPix.Shared.Structs.Browser.WebMessages;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Structs.Pix.Properties;
using PyonPix.Structs.Audio;
using PyonPix.Structs.Browser;
using PyonPix.Structs.Renderer;
using PyonPix.Ui;
using PyonPix.Utility;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace PyonPix.Services.Core;

public class BrowserService(PyonPix.Config.Configuration config, IServiceContext services, IWindowContext windows) : BaseService(config, services, windows)
{
	private Process? MediatorProcess;

	private MemoryMappedIpc Ipc;

	private long HeartbeatTick;

	private const uint HeartbeatTickRate = 1000u;

	private long SpatialAudioTick;

	private const uint SpatialAudioTickRate = 100u;

	public readonly Dictionary<string, Tab> Tabs = new Dictionary<string, Tab>();

	public string PresentationUri = string.Empty;

	private PixService PixService => Services.Get<PixService>();

	private DXService DXService => Services.Get<DXService>();

	private StateService StateService => Services.Get<StateService>();

	private ExtensionsService ExtensionsService => Services.Get<ExtensionsService>();

	private DataService DataService => Services.Get<DataService>();

	private SyncService SyncService => Services.Get<SyncService>();

	public BrowserState State { get; private set; }

	public System.Numerics.Vector2 PresentationPosition { get; private set; }

	public System.Numerics.Vector2 PresentationSize { get; private set; }

	public bool IsResizing { get; set; }

	public bool IsRescaling { get; set; }

	public bool IsHidden { get; set; } = true;

	public uint CursorId { get; private set; }

	public Tab? FocusedTab { get; private set; }

	public bool CanNavigate
	{
		get
		{
			if (State == BrowserState.Running)
			{
				return FocusedTab?.CanNavigate ?? false;
			}
			return false;
		}
	}

	public bool CanGoBack
	{
		get
		{
			if (State == BrowserState.Running)
			{
				return FocusedTab?.CanGoBack ?? false;
			}
			return false;
		}
	}

	public bool CanGoForward
	{
		get
		{
			if (State == BrowserState.Running)
			{
				return FocusedTab?.CanGoForward ?? false;
			}
			return false;
		}
	}

	public bool CanReload
	{
		get
		{
			if (State == BrowserState.Running)
			{
				return FocusedTab?.CanReload ?? false;
			}
			return false;
		}
	}

	public bool CanCancel
	{
		get
		{
			if (State == BrowserState.Running)
			{
				return FocusedTab?.CanCancel ?? false;
			}
			return false;
		}
	}

	public event Action<StatusUpdate>? OnStatusUpdate;

	public override Task Initialize()
	{
		Ipc = new MemoryMappedIpc("PyonPix", isPlugin: true);
		PixService.PixSpawned += OnPixSpawned;
		PixService.PixUpdated += OnPixUpdated;
		PixService.PixDespawned += OnPixDespawned;
		PixService.AllPixDespawned += OnAllPixDespawned;
		ExtensionsService.InstallExtensionRequest += delegate(string extensionId, string extensionName)
		{
			InstallExtension(extensionId, extensionName);
		};
		ExtensionsService.UninstallExtensionRequest += delegate(string extensionId, string extensionName)
		{
			UninstallExtension(extensionId, extensionName);
		};
		ExtensionsService.EnableExtensionRequest += delegate(string extensionId, string extensionName)
		{
			EnableExtension(extensionId, extensionName);
		};
		ExtensionsService.DisableExtensionRequest += delegate(string extensionId, string extensionName)
		{
			DisableExtension(extensionId, extensionName);
		};
		return Task.CompletedTask;
	}

	private void OnPixSpawned(IPix? p, bool isUserAction)
	{
		if (p != null && !Tabs.TryGetValue(p.Id, out Tab _))
		{
			DataService.CancelPendingRemoval(p.Id);
			EnsureTabForPix(p);
			if (FocusedTab == null)
			{
				SetFocus(p.Id, byUserInput: false);
			}
			SpawnBehaviour territorySpawnBehaviour = Config.Global.Browser.TerritorySpawnBehaviour;
			if (isUserAction || territorySpawnBehaviour.HasFlag(SpawnBehaviour.Navigate))
			{
				NavigateForPix(p);
			}
			territorySpawnBehaviour.HasFlag(SpawnBehaviour.Unmute);
		}
	}

	private void OnPixUpdated(PixUpdate u)
	{
		if (u.Pix == null || !PixService.IsSpawned(u.Pix))
		{
			return;
		}
		bool flag;
		switch (u.Type)
		{
		case PixUpdateType.All:
		case PixUpdateType.Uri:
		case PixUpdateType.BrowserProperties:
		case PixUpdateType.AudioProperties:
		case PixUpdateType.MediaState:
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (!flag || !Tabs.TryGetValue(u.Pix.Id, out Tab _))
		{
			return;
		}
		PixUpdateType type = u.Type;
		if ((uint)type <= 1u)
		{
			if (u.PerformLocalUpdate)
			{
				DataService.CancelPendingRemoval(u.Pix.Id);
				NavigateForPix(u.Pix);
			}
		}
		else if (State == BrowserState.Running)
		{
			if (u.Origin == PixUpdateOrigin.Remote && u.Type == PixUpdateType.MediaState && u.Pix is SyncedPix { Media: not null } syncedPix)
			{
				Services.Log.Verbose($"[BrowserService] OnPixUpdated MediaState applying to {u.Pix.Id}: {syncedPix.Media.IsPlaying},({syncedPix.Media.Action}),{syncedPix.Media.SeekTime}", Array.Empty<object>());
				Ipc.SendUpdateMediaState(u.Pix.Id, syncedPix.Media.Action, syncedPix.Media.IsPlaying, syncedPix.Media.SeekTime, syncedPix.Media.Duration, syncedPix.Media.Timestamp);
			}
			type = u.Type;
			if ((type == PixUpdateType.All || type == PixUpdateType.AudioProperties) ? true : false)
			{
				Ipc.SendUpdateSpatialAudio(u.Pix.Id, 1f, 1f);
			}
		}
	}

	private void OnPixDespawned(IPix? p, bool isUserAction)
	{
		if (p != null && Tabs.TryGetValue(p.Id, out Tab value))
		{
			DespawnBehaviour territoryDespawnBehaviour = Config.Global.Browser.TerritoryDespawnBehaviour;
			if (isUserAction || territoryDespawnBehaviour.HasFlag(DespawnBehaviour.Shutdown))
			{
				DestroyTab(value);
			}
			else
			{
				territoryDespawnBehaviour.HasFlag(DespawnBehaviour.Mute);
			}
		}
	}

	private void OnAllPixDespawned()
	{
	}

	private void InitializeMediator()
	{
		bool flag = true;
		if (!TryGetMediatorProcess(out MediatorProcess))
		{
			MediatorProcess.StartInfo = new ProcessStartInfo
			{
				FileName = Path.Combine(Services.PluginInterface.AssemblyLocation.DirectoryName, "PyonPix.Mediator.exe"),
				Arguments = string.Empty,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			MediatorProcess.Start();
			flag = false;
		}
		if (flag)
		{
			Ipc.SendCommand(CommandType.MediatorInitializeRequest);
		}
	}

	private bool TryGetMediatorProcess(out Process p)
	{
		p = new Process();
		Process[] processesByName = Process.GetProcessesByName("PyonPix.Mediator");
		if (processesByName.Length != 0)
		{
			p = processesByName[0];
			return true;
		}
		return false;
	}

	public void InitializeBrowser()
	{
		if (DXService.D3D11Device == null || State != BrowserState.Stopped)
		{
			return;
		}
		State = BrowserState.Initializing;
		Ipc.OnLog += delegate(Log e)
		{
			switch (e.Type)
			{
			case LogType.Verbose:
				Services.Log.Verbose("[Browser] " + e.Message, Array.Empty<object>());
				break;
			case LogType.Info:
				Services.Log.Info("[Browser] " + e.Message, Array.Empty<object>());
				break;
			case LogType.Warn:
				Services.Log.Warning("[Browser] " + e.Message, Array.Empty<object>());
				break;
			case LogType.Error:
				this.OnStatusUpdate?.Invoke(new StatusUpdate(e.Message, StatusType.Error));
				Services.Log.Error("[Browser] " + e.Message, Array.Empty<object>());
				break;
			}
		};
		Ipc.OnCommand += delegate(Command e)
		{
			switch (e.Type)
			{
			case CommandType.MediatorInitializeSuccess:
				this.OnStatusUpdate?.Invoke(new StatusUpdate("Initializing Browser"));
				Services.Log.Verbose("[Mediator] Initializing Browser", Array.Empty<object>());
				Ipc.SendInitializeBrowser(Config.GetConfigPath(), (uint)Environment.ProcessId, DXService.Luid);
				break;
			case CommandType.BrowserInitializeSuccess:
				this.OnStatusUpdate?.Invoke(new StatusUpdate("Browser Initialized"));
				Services.Log.Info("[Browser] Initialized", Array.Empty<object>());
				State = BrowserState.Running;
				break;
			case CommandType.BrowserInitializeFailed:
				this.OnStatusUpdate?.Invoke(new StatusUpdate("Browser Initialization Failed", StatusType.Error));
				Services.Log.Error("[Browser] Initialization Failed", Array.Empty<object>());
				State = BrowserState.Stopped;
				break;
			case CommandType.BrowserInitializeRequest:
				break;
			}
		};
		Ipc.OnHostInitializeState += delegate(HostInitializeState e)
		{
			switch (e.Type)
			{
			case StateType.Success:
				this.OnStatusUpdate?.Invoke(new StatusUpdate("BrowserHost Initialized"));
				Services.Log.Info("[BrowserHost] Initialized", Array.Empty<object>());
				State = BrowserState.Running;
				DataService.RefreshCacheAsync();
				{
					foreach (KeyValuePair<string, Tab> tab in Tabs)
					{
						Tab value = tab.Value;
						if (value.State == TabState.Uninitialized || value.State == TabState.WaitingForHost)
						{
							CreateNativeTab(value);
						}
					}
					break;
				}
			case StateType.Failed:
				this.OnStatusUpdate?.Invoke(new StatusUpdate("BrowserHost Failed", StatusType.Error));
				Services.Log.Error("[BrowserHost] " + e.Message, Array.Empty<object>());
				State = BrowserState.Stopped;
				InvokeShutdown();
				break;
			}
		};
		Ipc.OnTabInitializeState += delegate(TabInitializeState e)
		{
			if (e.Type == StateType.Success)
			{
				if (!Tabs.TryGetValue(e.PixId, out Tab value))
				{
					Services.Log.Warning("[Browser:" + e.PixId + "] Unknown Tab Initialized", Array.Empty<object>());
				}
				else
				{
					Services.Log.Info("[Browser:" + e.PixId + "] Initialized", Array.Empty<object>());
					value.State = TabState.Ready;
					if (!string.IsNullOrEmpty(value.PendingUri) && value.NavState == NavigationState.Pending)
					{
						value.NavState = NavigationState.Starting;
						Ipc.SendNavigate(e.PixId, BrowserUtil.NormalizeUri(value.PendingUri));
					}
					else
					{
						value.NavState = NavigationState.Ready;
					}
					DataService.RefreshCacheAsync();
					if (Config.Global.Browser.CheckUpdateExtensions)
					{
						ExtensionsService.CheckUpdateAllAsync(Config.Global.Browser.AutoUpdateExtensions);
					}
				}
			}
			else if (e.Type == StateType.Failed)
			{
				this.OnStatusUpdate?.Invoke(new StatusUpdate("Browser:" + e.PixId + " Failed", StatusType.Error));
				Services.Log.Error("[Browser:" + e.PixId + "] " + e.Message, Array.Empty<object>());
				if (Tabs.TryGetValue(e.PixId, out Tab value2))
				{
					DestroyTab(value2);
				}
			}
			else if (e.Type == StateType.TabDestroyed)
			{
				PixVariant variant = PixService.GetVariant(e.PixId);
				Services.Log.Verbose("[Browser:" + e.PixId + "] Destroyed", Array.Empty<object>());
				if (variant == null || !variant.PersistentCache)
				{
					DataService.RemoveUDF(e.PixId);
				}
			}
		};
		Ipc.OnUpdateFrame += delegate(UpdateFrame e)
		{
			UpdateFrame(e.PixId, (nint)e.SharedTexture, e.W, e.H);
		};
		Ipc.OnCursorChanged += delegate(CursorChanged e)
		{
			CursorId = e.CursorId;
		};
		Ipc.OnNavigationStarting += delegate(NavigationStarting e)
		{
			if (e.UserInitiated && Tabs.TryGetValue(e.PixId, out Tab value))
			{
				this.OnStatusUpdate?.Invoke(new StatusUpdate("Navigating to " + e.Uri));
				value.NavState = NavigationState.Started;
				value.PendingUri = BrowserUtil.NormalizeUriForSync(e.Uri);
			}
		};
		Ipc.OnHistoryChanged += delegate(HistoryChanged e)
		{
			string uri = BrowserUtil.NormalizeUriForSync(e.Uri);
			if (Tabs.TryGetValue(e.PixId, out Tab value))
			{
				_ = value.PendingUri;
				if (value.PendingUri != null && value.PendingUri.StartsWith("data:text/html;"))
				{
					value.NavState = NavigationState.Ready;
					value.PendingUri = null;
				}
				else
				{
					if (uri == "about:blank")
					{
						if (value.PendingUri == null || !value.PendingUri.StartsWith("pix://"))
						{
							return;
						}
						uri = value.PendingUri;
					}
					else
					{
						this.OnStatusUpdate?.Invoke(new StatusUpdate("Navigating to " + e.Uri, StatusType.Info, 2500));
					}
					value.PendingUri = null;
					value.PresentationUri = uri;
					if (FocusedTab == null || FocusedTab.PixId == value.PixId)
					{
						PresentationUri = value.PresentationUri;
					}
					if (!(value.CurrentNavigationItem?.Uri == uri))
					{
						if (PixService.SpawnedPixs.TryGetValue(e.PixId, out IPix value2))
						{
							value2.Browser.Uri = uri;
							PixService.UpdateUri(value2, PixUpdateOrigin.Local, performLocalUpdate: false);
						}
						if (value.CurrentNavigationItem != null && Uri.TryCreate(uri, UriKind.Absolute, out Uri result) && !string.IsNullOrWhiteSpace(result.Fragment) && value.CurrentNavigationItem.Uri.StartsWith(uri.Replace(result.Fragment, string.Empty)))
						{
							value.CurrentNavigationItem.Uri = uri;
						}
						else
						{
							if (value.CurrentNavigationIndex < value.History.Count - 1)
							{
								value.History.RemoveRange(value.CurrentNavigationIndex + 1, value.History.Count - value.CurrentNavigationIndex - 1);
							}
							int num = value.History.FindIndex((NavigationItem x) => x.Uri == uri);
							if (num != -1)
							{
								value.History.RemoveAt(num);
							}
							value.History.Add(new NavigationItem(uri));
							if (value.History.Count > 10)
							{
								value.History.RemoveAt(0);
							}
							value.CurrentNavigationIndex = value.History.Count - 1;
							value.NavState = NavigationState.Ready;
						}
					}
				}
			}
		};
		Ipc.OnTitleChanged += delegate(TitleChanged e)
		{
			if (Tabs.TryGetValue(e.PixId, out Tab value) && value.CurrentNavigationItem != null)
			{
				value.CurrentNavigationItem.Title = e.Title;
			}
		};
		Ipc.OnNavigationCompleted += delegate(NavigationCompleted e)
		{
			if (Tabs.TryGetValue(e.PixId, out Tab value))
			{
				this.OnStatusUpdate?.Invoke(new StatusUpdate(string.Empty, StatusType.None));
				if (value.NavState != NavigationState.Ready)
				{
					value.NavState = NavigationState.Ready;
					value.PresentationUri = value.CurrentNavigationItem?.Uri ?? string.Empty;
				}
			}
		};
		Ipc.OnNavigationCanceled += delegate(NavigationCanceled e)
		{
			if (Tabs.TryGetValue(e.PixId, out Tab value))
			{
				this.OnStatusUpdate?.Invoke(new StatusUpdate(string.Empty, StatusType.None));
				value.NavState = NavigationState.Ready;
			}
		};
		Ipc.OnFavIconChanged += delegate(FavIconChanged e)
		{
			if (Tabs.TryGetValue(e.PixId, out Tab t))
			{
				byte[] bytes = e.GetDataArray();
				if (bytes != null && bytes.Length != 0)
				{
					Services.Framework.RunOnTick((Func<Task>)async delegate
					{
						IDalamudTextureWrap favIcon = await Services.TextureProvider.CreateFromImageAsync((ReadOnlyMemory<byte>)bytes, (string)null, default(CancellationToken));
						IDalamudTextureWrap? favIcon2 = t.FavIcon;
						t.FavIcon = favIcon;
						((IDisposable)favIcon2)?.Dispose();
					}, default(TimeSpan), 0, default(CancellationToken));
				}
			}
		};
		Ipc.OnWebMessageReceived += delegate(WebMessageReceived e)
		{
			if (Tabs.TryGetValue(e.PixId, out Tab _) && PixService.SpawnedPixs.TryGetValue(e.PixId, out IPix value2) && value2 is SyncedPix syncedPix)
			{
				WebMessage webMessage = JsonSerializer.Deserialize<WebMessage>(e.Json);
				if (webMessage != null)
				{
					switch (webMessage.Type)
					{
					case WebMessageType.MediaState:
					{
						MediaState mediaState = webMessage.Payload.Deserialize<MediaState>();
						if (mediaState != null)
						{
							if (!syncedPix.CanSyncEdit)
							{
								Services.Log.Verbose("[BrowserService] WebMessage MediaState: Client Resync", Array.Empty<object>());
								SyncService.SyncMediaState(e.PixId, mediaState);
							}
							else
							{
								syncedPix.Media = mediaState;
								Services.Log.Verbose("[BrowserService] WebMessage MediaState: " + e.Json, Array.Empty<object>());
								PixService.UpdateMediaState(syncedPix);
							}
						}
						break;
					}
					case WebMessageType.MediaReady:
					{
						Services.Log.Verbose("[BrowserService] WebMessage MediaReady: " + e.Json, Array.Empty<object>());
						MediaState mediaState2 = webMessage.Payload.Deserialize<MediaState>();
						if (mediaState2 != null)
						{
							SyncService.SyncMediaState(e.PixId, mediaState2);
						}
						break;
					}
					case WebMessageType.MediaResync:
					{
						Services.Log.Verbose("[BrowserService] WebMessage MediaResync: " + e.Json, Array.Empty<object>());
						MediaState mediaState3 = webMessage.Payload.Deserialize<MediaState>();
						if (mediaState3 != null)
						{
							SyncService.SyncMediaState(e.PixId, mediaState3);
						}
						break;
					}
					case WebMessageType.Navigate:
					{
						Services.Log.Verbose("[BrowserService] WebMessage Navigate: " + e.Json, Array.Empty<object>());
						PyonPix.Shared.Structs.Browser.WebMessages.Navigate navigate = webMessage.Payload.Deserialize<PyonPix.Shared.Structs.Browser.WebMessages.Navigate>();
						if (navigate != null && syncedPix.CanSyncEdit)
						{
							Navigate(navigate.Uri);
						}
						break;
					}
					}
				}
			}
		};
		Ipc.OnExtensionOperation += delegate(ExtensionOperation e)
		{
			ExtensionsService.IsOperating = false;
			switch (e.ExtensionOp)
			{
			case ExtensionOp.Install:
				this.OnStatusUpdate?.Invoke(new StatusUpdate("Extension Installed: " + e.ExtensionId));
				break;
			case ExtensionOp.Remove:
				this.OnStatusUpdate?.Invoke(new StatusUpdate("Extension Removed: " + e.ExtensionId));
				break;
			case ExtensionOp.Enable:
				this.OnStatusUpdate?.Invoke(new StatusUpdate("Extension Enabled: " + e.ExtensionId));
				break;
			case ExtensionOp.Disable:
				this.OnStatusUpdate?.Invoke(new StatusUpdate("Extension Disabled: " + e.ExtensionId));
				break;
			}
		};
		Ipc.Start();
		InitializeMediator();
	}

	private void CreateNativeTab(Tab t)
	{
		if (t.State != TabState.Creating && t.State != TabState.Ready)
		{
			t.State = TabState.Creating;
			int x = (int)t.RenderPos.X;
			int y = (int)t.RenderPos.Y;
			uint w = (uint)Math.Max(1f, t.RenderSize.X);
			uint h = (uint)Math.Max(1f, t.RenderSize.Y);
			string[] extensionsToInstall = ExtensionsService.GetExtensionsToInstall();
			Ipc.SendCreateTab(t.PixId, t.GpuAcceleration, x, y, w, h, t.SyncCookies, extensionsToInstall);
		}
	}

	private Tab EnsureTabForPix(IPix p)
	{
		if (Tabs.TryGetValue(p.Id, out Tab value))
		{
			return value;
		}
		(System.Numerics.Vector2, System.Numerics.Vector2) renderBounds = GetRenderBounds(p);
		System.Numerics.Vector2 item = renderBounds.Item1;
		System.Numerics.Vector2 item2 = renderBounds.Item2;
		PixVariant variant = PixService.GetVariant(p);
		Tab tab = new Tab
		{
			PixId = p.Id,
			GpuAcceleration = p.Browser.GpuAcceleration,
			SyncCookies = (variant?.SyncCookies ?? false),
			State = TabState.Uninitialized,
			NavState = NavigationState.Ready,
			RenderPos = item,
			RenderSize = item2
		};
		Tabs[p.Id] = tab;
		return tab;
	}

	public void Update()
	{
		if (State == BrowserState.Running)
		{
			long tickCount = Environment.TickCount64;
			if (tickCount - HeartbeatTick >= 1000)
			{
				HeartbeatTick = tickCount;
				Heartbeat();
			}
		}
	}

	public bool Draw(System.Numerics.Vector2 imguiPos, System.Numerics.Vector2 imguiSize)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		UpdateLayout(imguiPos, imguiSize);
		if (!TrySetPresentationBounds(imguiPos, imguiSize))
		{
			return false;
		}
		if (State != BrowserState.Running || FocusedTab?.SRV == null)
		{
			return false;
		}
		ImGui.Image(new ImTextureID((IntPtr)FocusedTab.SRV.NativePointer), imguiSize);
		return true;
	}

	public bool TrySetPresentationBounds(System.Numerics.Vector2 pos, System.Numerics.Vector2 size)
	{
		if (size.X < 1f || size.Y < 1f)
		{
			return false;
		}
		PresentationPosition = pos;
		PresentationSize = size;
		return true;
	}

	public void DetermineResizeState(bool sizeChanged, bool mouseDragging)
	{
		if (!IsResizing && sizeChanged && mouseDragging)
		{
			IsResizing = true;
		}
		if (IsResizing && !mouseDragging)
		{
			IsResizing = false;
		}
	}

	public void UpdateLayout(System.Numerics.Vector2 imguiPos, System.Numerics.Vector2 imguiSize)
	{
		if (State != BrowserState.Running || FocusedTab?.SRV == null || IsResizing || IsRescaling)
		{
			return;
		}
		foreach (IPix value2 in PixService.SpawnedPixs.Values)
		{
			if (!Tabs.TryGetValue(value2.Id, out Tab value) || value.State != TabState.Ready)
			{
				continue;
			}
			var (vector, vector2) = GetRenderBounds(value2, imguiPos, imguiSize);
			if (!(vector2.X < 1f) && !(vector2.Y < 1f))
			{
				if (vector2 != value.RenderSize)
				{
					value.RenderSize = vector2;
					value.RenderPos = vector;
					Ipc.SendResize(value2.Id, (int)value.RenderPos.X, (int)value.RenderPos.Y, (uint)value.RenderSize.X, (uint)value.RenderSize.Y);
				}
				else if (vector != value.RenderPos)
				{
					value.RenderPos = vector;
					Ipc.SendReposition(value2.Id, (int)value.RenderPos.X, (int)value.RenderPos.Y);
				}
			}
		}
	}

	public bool TryGetRenderBounds(IPix pix, out System.Numerics.Vector2 pos, out System.Numerics.Vector2 size)
	{
		pos = default(System.Numerics.Vector2);
		size = default(System.Numerics.Vector2);
		if (!Tabs.TryGetValue(pix.Id, out Tab value))
		{
			return false;
		}
		if (value.State != TabState.Ready)
		{
			return false;
		}
		(pos, size) = GetRenderBounds(pix);
		if (size.X > 0f)
		{
			return size.Y > 0f;
		}
		return false;
	}

	private (System.Numerics.Vector2, System.Numerics.Vector2) GetRenderBounds(IPix p, System.Numerics.Vector2 defPos = default(System.Numerics.Vector2), System.Numerics.Vector2 defSize = default(System.Numerics.Vector2))
	{
		System.Numerics.Vector2 gameResolution = UiUtil.GameResolution;
		if (defSize == default(System.Numerics.Vector2))
		{
			System.Numerics.Vector2 presentationSize = PresentationSize;
			if (presentationSize.X <= 1f || presentationSize.Y <= 1f)
			{
				defPos = System.Numerics.Vector2.Zero;
				defSize = gameResolution;
			}
			else
			{
				defPos = PresentationPosition;
				defSize = presentationSize;
			}
		}
		System.Numerics.Vector2 item = System.Numerics.Vector2.Zero;
		System.Numerics.Vector2 item2 = System.Numerics.Vector2.Zero;
		switch (p.Browser.ScaleMode)
		{
		case BrowserScaleMode.GameWindow:
			item = System.Numerics.Vector2.Zero;
			item2 = gameResolution;
			break;
		case BrowserScaleMode.GameWindowWhenHidden:
			item = (IsHidden ? System.Numerics.Vector2.Zero : defPos);
			item2 = (IsHidden ? gameResolution : defSize);
			break;
		case BrowserScaleMode.CustomScale:
			item = System.Numerics.Vector2.Zero;
			item2 = p.Browser.CustomScale;
			break;
		case BrowserScaleMode.CustomScaleWhenHidden:
			item = (IsHidden ? System.Numerics.Vector2.Zero : defPos);
			item2 = (IsHidden ? p.Browser.CustomScale : defSize);
			break;
		case BrowserScaleMode.BrowserWindow:
			item = defPos;
			item2 = defSize;
			break;
		}
		return (item, item2);
	}

	private void UpdateFrame(string tabId, nint sharedHandle, uint width, uint height)
	{
		if (DXService.D3D11Device == null || sharedHandle == IntPtr.Zero || DXService.D3D11Device.DeviceRemovedReason != Result.Ok || !Tabs.TryGetValue(tabId, out Tab value) || value.SharedHandle == sharedHandle)
		{
			return;
		}
		value.SRV?.Dispose();
		value.SRV = null;
		try
		{
			using Texture2D texture2D = DXService.D3D11Device.OpenSharedResource<Texture2D>(sharedHandle);
			if (texture2D != null)
			{
				ShaderResourceViewDescription description = new ShaderResourceViewDescription
				{
					Format = Format.B8G8R8A8_UNorm,
					Dimension = ShaderResourceViewDimension.Texture2D,
					Texture2D = new ShaderResourceViewDescription.Texture2DResource
					{
						MostDetailedMip = 0,
						MipLevels = 1
					}
				};
				ShaderResourceView shaderResourceView = new ShaderResourceView(DXService.D3D11Device, texture2D, description);
				value.SharedHandle = sharedHandle;
				value.SRV = shaderResourceView;
				if (tabId == FocusedTab?.PixId)
				{
					SwapFocusedSRV(shaderResourceView, sharedHandle);
				}
			}
		}
		catch (Exception ex)
		{
			Services.Log.Error(ex, "UpdateFrame Failed", Array.Empty<object>());
		}
	}

	public void UpdateSpatialAudio(Dictionary<string, Renderer>.ValueCollection renderers)
	{
		if (State != BrowserState.Running || PixService.SpawnedPixs.Count == 0)
		{
			return;
		}
		long tickCount = Environment.TickCount64;
		if (tickCount - SpatialAudioTick < 100)
		{
			return;
		}
		SpatialAudioTick = tickCount;
		AudioGlobalProperties audio = Config.Global.Audio;
		bool flag = audio.ListenerType == AudioListenerType.Camera || Services.Objects.LocalPlayer == null;
		System.Numerics.Vector3 vector;
		System.Numerics.Vector3 vector2;
		if (flag)
		{
			Matrix4x4.Invert(CameraService.GetViewMatrix(), out var result);
			vector = result.Translation;
			vector2 = System.Numerics.Vector3.Normalize(new System.Numerics.Vector3(result.M11, result.M12, result.M13));
		}
		else
		{
			vector = StateService.LocalPlayerPosition;
			vector2 = System.Numerics.Vector3.Normalize(System.Numerics.Vector3.Transform(System.Numerics.Vector3.UnitX, StateService.LocalPlayerRotation));
		}
		foreach (Renderer renderer in renderers)
		{
			if (renderer.ScreenTransform.HasValue && PixService.SpawnedPixs.TryGetValue(renderer.PixId, out IPix value))
			{
				AudioPixProperties audio2 = value.Audio;
				if (audio2.SpatialEnabled)
				{
					System.Numerics.Vector3 value2 = renderer.ScreenTransform.Value.Translation - vector;
					float num = value2.Length();
					float num2 = MathF.Max(0.01f, audio2.FalloffMaxDistance);
					float num3 = MathF.Min(num / num2, 1f);
					float num4 = 1f - num3 * num3 * (3f - 2f * num3);
					float num5 = audio2.Volume * num4 * audio.MasterVolume;
					value2.Y = 0f;
					vector2.Y = 0f;
					value2 = System.Numerics.Vector3.Normalize(value2);
					vector2 = System.Numerics.Vector3.Normalize(vector2);
					float y = (flag ? System.Numerics.Vector3.Dot(value2, vector2) : (0f - System.Numerics.Vector3.Dot(value2, vector2)));
					y = MathF.Max(-1f, MathF.Min(1f, y));
					float left = num5 * MathF.Sqrt(0.5f * (1f - y));
					float right = num5 * MathF.Sqrt(0.5f * (1f + y));
					Ipc.SendUpdateSpatialAudio(renderer.PixId, left, right);
				}
			}
		}
	}

	public void NavigateForPix(IPix pix)
	{
		if (pix == null)
		{
			return;
		}
		BrowserGlobalProperties browser = Config.Global.Browser;
		Tab tab = EnsureTabForPix(pix);
		tab.PendingUri = (string.IsNullOrWhiteSpace(pix.Browser.Uri) ? tab.GetHomeUri(browser.HomeUriType, browser.HomeUri) : pix.Browser.Uri);
		tab.NavState = NavigationState.Pending;
		if (State != BrowserState.Running)
		{
			tab.State = TabState.WaitingForHost;
			if (State == BrowserState.Stopped)
			{
				InitializeBrowser();
			}
		}
		else if (tab.State == TabState.Uninitialized || tab.State == TabState.WaitingForHost)
		{
			CreateNativeTab(tab);
		}
		else if (tab.State == TabState.Ready)
		{
			tab.NavState = NavigationState.Starting;
			Ipc.SendNavigate(tab.PixId, BrowserUtil.NormalizeUri(tab.PendingUri));
		}
	}

	private void SwapFocusedSRV(ShaderResourceView? newSrv, nint sharedHandle)
	{
		Tab? focusedTab = FocusedTab;
		if (focusedTab == null || focusedTab.SharedHandle != sharedHandle)
		{
			ShaderResourceView? obj = FocusedTab?.SRV;
			Tab? focusedTab2 = FocusedTab;
			if (focusedTab2 != null)
			{
				focusedTab2.SRV = newSrv;
			}
			Tab? focusedTab3 = FocusedTab;
			if (focusedTab3 != null)
			{
				focusedTab3.SharedHandle = sharedHandle;
			}
			obj?.Dispose();
		}
	}

	private void ReleaseFocusedSRV()
	{
		FocusedTab?.SRV?.Dispose();
		Tab? focusedTab = FocusedTab;
		if (focusedTab != null)
		{
			focusedTab.SRV = null;
		}
		Tab? focusedTab2 = FocusedTab;
		if (focusedTab2 != null)
		{
			focusedTab2.SharedHandle = IntPtr.Zero;
		}
	}

	public bool FocusTab(string pixId, bool byUserInput = true)
	{
		if (State != BrowserState.Running)
		{
			return false;
		}
		if (!Tabs.TryGetValue(pixId, out Tab _))
		{
			return false;
		}
		if (FocusedTab?.PixId != pixId)
		{
			SetFocus(pixId, byUserInput);
		}
		return true;
	}

	private void SetFocus(string pixId, bool byUserInput = true)
	{
		PixService.SpawnedPixs.TryGetValue(pixId, out IPix value);
		if (!Tabs.TryGetValue(pixId, out Tab value2))
		{
			if (value == null)
			{
				return;
			}
			value2 = EnsureTabForPix(value);
		}
		PresentationUri = value2.PresentationUri ?? value?.Browser.Uri ?? string.Empty;
		FocusedTab = value2;
		Ipc.SendSetFocusedTab(pixId, byUserInput);
		if (value2.SRV != null)
		{
			SwapFocusedSRV(value2.SRV, value2.SharedHandle);
		}
		else
		{
			ReleaseFocusedSRV();
		}
	}

	private void ClearFocus()
	{
		ReleaseFocusedSRV();
		FocusedTab = null;
		PresentationUri = string.Empty;
	}

	public void LostFocus()
	{
		if (State == BrowserState.Running)
		{
			Ipc.SendCommand(CommandType.BrowserLostFocus);
		}
	}

	public void Navigate(string uri)
	{
		if (CanNavigate && PixService.SpawnedPixs.TryGetValue(FocusedTab.PixId, out IPix value))
		{
			value.Browser.Uri = uri;
			FocusedTab.NavState = NavigationState.Starting;
			PixService.UpdateUri(value);
		}
	}

	public void NavHome()
	{
		if (CanNavigate)
		{
			BrowserGlobalProperties browser = Config.Global.Browser;
			Navigate(FocusedTab.GetHomeUri(browser.HomeUriType, browser.HomeUri));
		}
	}

	public void NavBack()
	{
		if (CanGoBack && PixService.SpawnedPixs.TryGetValue(FocusedTab.PixId, out IPix value))
		{
			FocusedTab.CurrentNavigationIndex--;
			value.Browser.Uri = FocusedTab.CurrentNavigationItem.Uri;
			FocusedTab.NavState = NavigationState.Starting;
			PixService.UpdateUri(value);
		}
	}

	public void NavHistory(int index)
	{
		if ((CanGoBack || CanGoForward) && PixService.SpawnedPixs.TryGetValue(FocusedTab.PixId, out IPix value))
		{
			FocusedTab.CurrentNavigationIndex = index;
			value.Browser.Uri = FocusedTab.CurrentNavigationItem.Uri;
			FocusedTab.NavState = NavigationState.Starting;
			PixService.UpdateUri(value);
		}
	}

	public void NavForward()
	{
		if (CanGoForward && PixService.SpawnedPixs.TryGetValue(FocusedTab.PixId, out IPix value))
		{
			FocusedTab.CurrentNavigationIndex++;
			value.Browser.Uri = FocusedTab.CurrentNavigationItem.Uri;
			FocusedTab.NavState = NavigationState.Starting;
			PixService.UpdateUri(value);
		}
	}

	public void NavReload()
	{
		if (CanReload && PixService.SpawnedPixs.TryGetValue(FocusedTab.PixId, out IPix _))
		{
			FocusedTab.NavState = NavigationState.Starting;
			Ipc.SendReload(FocusedTab.PixId);
		}
	}

	public void NavCancel()
	{
		if (CanCancel)
		{
			FocusedTab.NavState = NavigationState.Ready;
			Ipc.SendStopNavigation(FocusedTab.PixId);
		}
	}

	public void SendMouseEvent(IPix pix, uint msg, nint wParam, nint lParam)
	{
		SendMouseEvent(pix.Id, msg, wParam, lParam);
	}

	public void SendMouseEvent(string pixId, uint msg, nint wParam, nint lParam)
	{
		if (State == BrowserState.Running && Tabs.TryGetValue(pixId, out Tab value) && !(FocusedTab?.PixId != pixId) && value.State == TabState.Ready)
		{
			Ipc.SendSendMouseEvent(pixId, msg, wParam, lParam);
		}
	}

	public void ToggleTheatreMode()
	{
		if (CanNavigate && PixService.SpawnedPixs.TryGetValue(FocusedTab.PixId, out IPix _) && State == BrowserState.Running && FocusedTab.State == TabState.Ready)
		{
			Ipc.SendToggleTheatreMode(FocusedTab.PixId);
		}
	}

	public void ToggleTheatreMode(string pixId)
	{
		if (CanNavigate && PixService.SpawnedPixs.TryGetValue(pixId, out IPix _) && State == BrowserState.Running && Tabs.TryGetValue(pixId, out Tab value2) && value2.State == TabState.Ready)
		{
			Ipc.SendToggleTheatreMode(pixId);
		}
	}

	public void OpenDevTools()
	{
		if (State == BrowserState.Running && FocusedTab != null && FocusedTab.State == TabState.Ready)
		{
			Ipc.SendOpenDevTools(FocusedTab.PixId);
		}
	}

	public void InstallExtension(string extensionId, string extensionName)
	{
		if (State != BrowserState.Running || FocusedTab == null)
		{
			ExtensionsService.IsOperating = false;
		}
		else
		{
			Ipc.SendInstallExtension(extensionId, extensionName);
		}
	}

	public void UninstallExtension(string extensionId, string extensionName)
	{
		if (State != BrowserState.Running || FocusedTab == null)
		{
			ExtensionsService.IsOperating = false;
		}
		else
		{
			Ipc.SendUninstallExtension(extensionId, extensionName);
		}
	}

	public void EnableExtension(string extensionId, string extensionName)
	{
		if (State != BrowserState.Running || FocusedTab == null)
		{
			ExtensionsService.IsOperating = false;
		}
		else
		{
			Ipc.SendEnableExtension(extensionId, extensionName);
		}
	}

	public void DisableExtension(string extensionId, string extensionName)
	{
		if (State != BrowserState.Running || FocusedTab == null)
		{
			ExtensionsService.IsOperating = false;
		}
		else
		{
			Ipc.SendDisableExtension(extensionId, extensionName);
		}
	}

	private void Heartbeat()
	{
		if (State == BrowserState.Running)
		{
			Ipc.SendCommand(CommandType.BrowserHeartbeat);
		}
	}

	public void Shutdown()
	{
		if (State != BrowserState.Stopped && State != BrowserState.Stopping)
		{
			State = BrowserState.Stopping;
		}
	}

	public void InvokeShutdown()
	{
		State = BrowserState.Stopped;
		DestroyAllTabs();
		Ipc.SendCommand(CommandType.BrowserShutdown);
	}

	private void DestroyTab(Tab? t)
	{
		if (State == BrowserState.Running && t != null)
		{
			string pixId = t.PixId;
			if (FocusedTab?.PixId == pixId)
			{
				ClearFocus();
			}
			Ipc.SendDestroyTab(t.PixId);
			t.Dispose();
			Tabs.Remove(pixId);
		}
	}

	private void DestroyAllTabs()
	{
		foreach (Tab item in Tabs.Values.ToList())
		{
			DestroyTab(item);
		}
		Tabs.Clear();
	}

	public override Task Dispose()
	{
		PixService.PixSpawned -= OnPixSpawned;
		PixService.PixUpdated -= OnPixUpdated;
		PixService.PixDespawned -= OnPixDespawned;
		PixService.AllPixDespawned -= OnAllPixDespawned;
		InvokeShutdown();
		Ipc?.Dispose();
		MediatorProcess?.Kill();
		MediatorProcess?.Dispose();
		return Task.CompletedTask;
	}
}
