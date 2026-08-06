using System;
using Dalamud.Game.Gui.Toast;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace Hypostasis.Dalamud;

public class DalamudApi
{
	private static readonly string printName = Hypostasis.PluginName;

	private static readonly string printHeader = "[" + printName + "] ";

	public static IDalamudPluginInterface PluginInterface { get; private set; }

	[PluginService]
	public static IAddonEventManager AddonEventManager { get; private set; }

	[PluginService]
	public static IAddonLifecycle AddonLifecycle { get; private set; }

	[PluginService]
	public static IAetheryteList AetheryteList { get; private set; }

	[PluginService]
	public static IBuddyList BuddyList { get; private set; }

	[PluginService]
	public static IChatGui ChatGui { get; private set; }

	[PluginService]
	public static IClientState ClientState { get; private set; }

	[PluginService]
	public static ICommandManager CommandManager { get; private set; }

	[PluginService]
	public static ICondition Condition { get; private set; }

	[PluginService]
	public static IDataManager DataManager { get; private set; }

	[PluginService]
	public static IDtrBar DtrBar { get; private set; }

	[PluginService]
	public static IDutyState DutyState { get; private set; }

	[PluginService]
	public static IFateTable FateTable { get; private set; }

	[PluginService]
	public static IFlyTextGui FlyTextGui { get; private set; }

	[PluginService]
	public static IFramework Framework { get; private set; }

	[PluginService]
	public static IGameConfig GameConfig { get; private set; }

	[PluginService]
	public static IGameGui GameGui { get; private set; }

	[PluginService]
	public static IGameInteropProvider GameInteropProvider { get; private set; }

	[PluginService]
	public static IGameLifecycle GameLifecycle { get; private set; }

	[PluginService]
	public static IGamepadState GamepadState { get; private set; }

	[PluginService]
	public static IJobGauges JobGauges { get; private set; }

	[PluginService]
	public static IKeyState KeyState { get; private set; }

	[PluginService]
	public static INotificationManager NotificationManager { get; private set; }

	[PluginService]
	public static IObjectTable ObjectTable { get; private set; }

	[PluginService]
	public static IPartyFinderGui PartyFinderGui { get; private set; }

	[PluginService]
	public static IPartyList PartyList { get; private set; }

	[PluginService]
	public static IPluginLog PluginLog { get; private set; }

	[PluginService]
	private static ISigScanner sigScanner
	{
		set
		{
			SigScanner = new SigScannerWrapper(value);
		}
	}

	public static SigScannerWrapper SigScanner { get; private set; }

	[PluginService]
	public static ITargetManager TargetManager { get; private set; }

	[PluginService]
	public static ITextureProvider TextureProvider { get; private set; }

	[PluginService]
	public static ITitleScreenMenu TitleScreenMenu { get; private set; }

	[PluginService]
	public static IToastGui ToastGui { get; private set; }

	public DalamudApi(IDalamudPluginInterface pluginInterface)
	{
		PluginInterface = pluginInterface;
		if (!pluginInterface.Inject((object)this, Array.Empty<object>()))
		{
			throw new ApplicationException("Failed loading DalamudApi!");
		}
	}

	public static void PrintEcho(string message)
	{
		ChatGui.Print(printHeader + message, (string)null, (ushort?)null);
	}

	public static void PrintError(string message)
	{
		ChatGui.PrintError(printHeader + message, (string)null, (ushort?)null);
	}

	public static void ShowNotification(string message, NotificationType type = (NotificationType)0, uint msDelay = 3000u)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		NotificationManager.AddNotification(new Notification
		{
			Type = type,
			Title = printName,
			Content = message,
			InitialDuration = TimeSpan.FromMilliseconds(msDelay)
		});
	}

	public static void ShowToast(string message, ToastOptions options = null)
	{
		ToastGui.ShowNormal(printHeader + message, options);
	}

	public static void ShowQuestToast(string message, QuestToastOptions options = null)
	{
		ToastGui.ShowQuest(printHeader + message, options);
	}

	public static void ShowErrorToast(string message)
	{
		ToastGui.ShowError(printHeader + message);
	}

	public static void LogVerbose(string message, Exception exception = null)
	{
		PluginLog.Verbose(exception, message, Array.Empty<object>());
	}

	public static void LogDebug(string message, Exception exception = null)
	{
		PluginLog.Debug(exception, message, Array.Empty<object>());
	}

	public static void LogInfo(string message, Exception exception = null)
	{
		PluginLog.Information(exception, message, Array.Empty<object>());
	}

	public static void LogWarning(string message, Exception exception = null)
	{
		PluginLog.Warning(exception, message, Array.Empty<object>());
	}

	public static void LogError(string message, Exception exception = null)
	{
		PluginLog.Error(exception, message, Array.Empty<object>());
	}

	public static void LogFatal(string message, Exception exception = null)
	{
		PluginLog.Fatal(exception, message, Array.Empty<object>());
	}

	public static void Initialize(IDalamudPluginInterface pluginInterface)
	{
		new DalamudApi(pluginInterface);
	}

	public static void Dispose()
	{
		SigScanner?.Dispose();
	}
}
