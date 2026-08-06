using System;
using System.Runtime.InteropServices;
using PyonPix.Ipc;
using PyonPix.Mediator.Interop;
using PyonPix.Shared.Interop;
using PyonPix.Shared.Ipc;
using PyonPix.Shared.Structs.Renderer;

namespace PyonPix.Mediator;

internal static class Program
{
	private static MemoryMappedIpc Ipc;

	private static BrowserInterop.OnLogCallback OnLog;

	private static BrowserInterop.OnHostReadyCallback OnHostReady;

	private static BrowserInterop.OnHostFailedCallback OnHostFailed;

	private static BrowserInterop.OnTabReadyCallback OnTabReady;

	private static BrowserInterop.OnTabFailedCallback OnTabFailed;

	private static BrowserInterop.OnTabDestroyedCallback OnTabDestroyed;

	private static BrowserInterop.OnFrameReadyCallback OnFrameReady;

	private static BrowserInterop.OnCursorChangedCallback OnCursorChanged;

	private static BrowserInterop.OnNavigationStartingCallback OnNavigationStarting;

	private static BrowserInterop.OnNavigationCompletedCallback OnNavigationCompleted;

	private static BrowserInterop.OnNavigationCanceledCallback OnNavigationCanceled;

	private static BrowserInterop.OnHistoryChangedCallback OnHistoryChanged;

	private static BrowserInterop.OnTitleChangedCallback OnTitleChanged;

	private static BrowserInterop.OnFavIconChangedCallback OnFavIconChanged;

	private static BrowserInterop.OnWebMessageReceivedCallback OnWebMessageReceived;

	private static BrowserInterop.OnExtensionOperationCallback OnExtensionOperation;

	[STAThread]
	private static void Main(string[] args)
	{
		Ipc = new MemoryMappedIpc("PyonPix", isPlugin: false);
		try
		{
			Ipc.OnCommand += delegate(Command cmd)
			{
				switch (cmd.Type)
				{
				case CommandType.MediatorInitializeRequest:
					Ipc.SendCommand(CommandType.MediatorInitializeSuccess);
					break;
				case CommandType.BrowserHeartbeat:
					BrowserInterop.Heartbeat();
					break;
				case CommandType.BrowserShutdown:
					BrowserInterop.Shutdown();
					break;
				case CommandType.BrowserLostFocus:
					BrowserInterop.LostFocus();
					break;
				}
			};
			InitializeBrowser();
			Ipc.SendCommand(CommandType.MediatorInitializeSuccess);
			Ipc.OnInitializeBrowser += delegate(InitializeBrowser e)
			{
				bool flag = BrowserInterop.Initialize(e.PluginPath, e.GamePid, new LUID
				{
					LowPart = e.LuidLowPart,
					HighPart = e.LuidHighPart
				});
				Ipc.SendCommand(flag ? CommandType.BrowserInitializeSuccess : CommandType.BrowserInitializeFailed);
			};
			Ipc.OnCreateTab += delegate(CreateTab e)
			{
				string[] array = new string[e.ExtensionsLength];
				for (int i = 0; i < e.ExtensionsLength; i++)
				{
					array[i] = e.Extensions(i);
				}
				BrowserInterop.CreateTab(e.PixId, e.GpuAcceleration, e.X, e.Y, e.W, e.H, e.SyncCookies, array, array.Length);
			};
			Ipc.OnDestroyTab += delegate(DestroyTab e)
			{
				BrowserInterop.DestroyTab(e.PixId);
			};
			Ipc.OnUpdateMediaState += delegate(UpdateMediaState e)
			{
				BrowserInterop.UpdateMediaState(e.PixId, (uint)e.Action, e.IsPlaying, e.SeekTime, e.Duration, e.Timestamp);
			};
			Ipc.OnToggleTheatreMode += delegate(ToggleTheatreMode e)
			{
				BrowserInterop.ToggleTheatreMode(e.PixId);
			};
			Ipc.OnNavigate += delegate(Navigate e)
			{
				BrowserInterop.Navigate(e.PixId, e.Uri);
			};
			Ipc.OnReload += delegate(Reload e)
			{
				BrowserInterop.Reload(e.PixId);
			};
			Ipc.OnStopNavigation += delegate(StopNavigation e)
			{
				BrowserInterop.StopNavigation(e.PixId);
			};
			Ipc.OnResize += delegate(Resize e)
			{
				BrowserInterop.Resize(e.PixId, e.X, e.Y, e.W, e.H);
			};
			Ipc.OnReposition += delegate(Reposition e)
			{
				BrowserInterop.Reposition(e.PixId, e.X, e.Y);
			};
			Ipc.OnSetFocusedTab += delegate(SetFocusedTab e)
			{
				BrowserInterop.SetFocusedTab(e.PixId, e.ByUserInput);
			};
			Ipc.OnSendMouseEvent += delegate(SendMouseEvent e)
			{
				BrowserInterop.SendMouseEvent(e.PixId, e.Msg, (nint)e.WParam, (nint)e.LParam);
			};
			Ipc.OnUpdateSpatialAudio += delegate(UpdateSpatialAudio e)
			{
				BrowserInterop.UpdateSpatialAudio(e.PixId, e.Left, e.Right);
			};
			Ipc.OnOpenDevTools += delegate(OpenDevTools e)
			{
				BrowserInterop.OpenDevTools(e.PixId);
			};
			Ipc.OnInstallExtension += delegate(InstallExtension e)
			{
				BrowserInterop.InstallExtension(e.ExtensionId, e.ExtensionName);
			};
			Ipc.OnUninstallExtension += delegate(UninstallExtension e)
			{
				BrowserInterop.UninstallExtension(e.ExtensionId, e.ExtensionName);
			};
			Ipc.OnEnableExtension += delegate(EnableExtension e)
			{
				BrowserInterop.EnableExtension(e.ExtensionId, e.ExtensionName);
			};
			Ipc.OnDisableExtension += delegate(DisableExtension e)
			{
				BrowserInterop.DisableExtension(e.ExtensionId, e.ExtensionName);
			};
			Ipc.Start();
			Win32Interop.MessageLoop();
			Ipc.Dispose();
		}
		catch (Exception value)
		{
			Ipc.SendLog(LogType.Error, $"[Mediator] Critical Error: {value}");
		}
	}

	private static void InitializeBrowser()
	{
		OnLog = delegate(LogType logType, string msg)
		{
			Ipc.SendLog(logType, msg);
		};
		OnHostReady = delegate
		{
			Ipc.SendHostInitializeState(StateType.Success);
		};
		OnHostFailed = delegate(string message)
		{
			Ipc.SendHostInitializeState(StateType.Failed, message);
		};
		OnTabReady = delegate(string pixId)
		{
			Ipc.SendTabInitializeState(StateType.Success, pixId);
		};
		OnTabFailed = delegate(string pixId, string message)
		{
			Ipc.SendTabInitializeState(StateType.Failed, pixId, message);
		};
		OnTabDestroyed = delegate(string pixId)
		{
			Ipc.SendTabInitializeState(StateType.TabDestroyed, pixId);
		};
		OnFrameReady = delegate(string pixId, nint sharedTex, uint w, uint h)
		{
			Ipc.SendUpdateFrame(pixId, sharedTex, w, h);
		};
		OnCursorChanged = delegate(uint cursorId)
		{
			Ipc.SendCursorChanged(cursorId);
		};
		OnNavigationStarting = delegate(string pixId, string uri, bool userInitiated)
		{
			Ipc.SendNavigationStarting(pixId, uri, userInitiated);
		};
		OnHistoryChanged = delegate(string pixId, string uri)
		{
			Ipc.SendHistoryChanged(pixId, uri);
		};
		OnTitleChanged = delegate(string pixId, string title)
		{
			Ipc.SendTitleChanged(pixId, title);
		};
		OnNavigationCompleted = delegate(string pixId, uint statusCode)
		{
			Ipc.SendNavigationCompleted(pixId, statusCode);
		};
		OnNavigationCanceled = delegate(string pixId)
		{
			Ipc.SendNavigationCanceled(pixId);
		};
		OnFavIconChanged = delegate(string pixId, nint data, int length)
		{
			if (length > 0 && data != IntPtr.Zero)
			{
				byte[] array = new byte[length];
				Marshal.Copy(data, array, 0, length);
				Ipc.SendFavIconChanged(pixId, array);
			}
		};
		OnWebMessageReceived = delegate(string pixId, string json)
		{
			Ipc.SendWebMessageReceived(pixId, json);
		};
		OnExtensionOperation = delegate(ExtensionOp extensionOp, string extensionId)
		{
			Ipc.SendExtensionOperation(extensionOp, extensionId);
		};
		BrowserInterop.RegisterCallbacks(OnLog, OnHostReady, OnHostFailed, OnTabReady, OnTabFailed, OnTabDestroyed, OnFrameReady, OnCursorChanged, OnNavigationStarting, OnNavigationCompleted, OnNavigationCanceled, OnHistoryChanged, OnTitleChanged, OnFavIconChanged, OnWebMessageReceived, OnExtensionOperation);
	}
}
