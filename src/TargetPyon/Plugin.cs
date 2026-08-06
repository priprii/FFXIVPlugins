using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace TargetPyon;

public sealed class Plugin : IDalamudPlugin, IDisposable
{
	private const string CommandName = "/tar";

	private const string AltCommandName = "/targetpyon";

	private const string VisCommandName = "/tarvisible";

	private const string OverlayCommandName = "/taroverlay";

	public AudioManager AudioManager;

	private WindowSystem Windows;

	public static MainWindow MainWindow;

	public static OverlayWindow OverlayWindow;

	private readonly ExcelSheet<ContentFinderCondition>? ContentFinderConditionsSheet;

	public static BlacklistManager BlacklistManager;

	public string Name => "Target";

	[PluginService]
	public static IDalamudPluginInterface PluginInterface { get; private set; }

	[PluginService]
	public static ICommandManager CommandManager { get; private set; }

	[PluginService]
	public static IClientState ClientState { get; private set; }

	[PluginService]
	public static IFramework Framework { get; private set; }

	[PluginService]
	public static IObjectTable Objects { get; private set; }

	[PluginService]
	public static IPartyList PartyList { get; private set; }

	[PluginService]
	public static ITargetManager Targets { get; private set; }

	[PluginService]
	public static IChatGui ChatGui { get; private set; }

	[PluginService]
	public static IGameGui GameGui { get; private set; }

	[PluginService]
	public static ISigScanner SigScanner { get; private set; }

	[PluginService]
	public static IDataManager DataManager { get; private set; }

	[PluginService]
	public static ICondition Condition { get; private set; }

	[PluginService]
	public static IGameInteropProvider GameInteropProvider { get; private set; }

	[PluginService]
	public static IPluginLog PluginLog { get; private set; }

	public static Config Config { get; set; }

	public Plugin()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Expected O, but got Unknown
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Expected O, but got Unknown
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Expected O, but got Unknown
		CommandManager.AddHandler("/tar", new CommandInfo(new HandlerDelegate(OnCommand))
		{
			HelpMessage = "Open Plugin Interface."
		});
		CommandManager.AddHandler("/targetpyon", new CommandInfo(new HandlerDelegate(OnCommand))
		{
			HelpMessage = "Open Plugin Interface."
		});
		CommandManager.AddHandler("/tarvisible", new CommandInfo(new HandlerDelegate(OnVisCommand))
		{
			HelpMessage = "Toggle visibility of players who are not in party/friend list."
		});
		CommandManager.AddHandler("/taroverlay", new CommandInfo(new HandlerDelegate(OnOverlayCommand))
		{
			HelpMessage = "Toggle visibility of the overlay."
		});
		PluginInterface.UiBuilder.OpenMainUi += delegate
		{
			((Window)OverlayWindow).IsOpen = true;
		};
		PluginInterface.UiBuilder.OpenConfigUi += delegate
		{
			((Window)MainWindow).IsOpen = true;
		};
		Config = (PluginInterface.GetPluginConfig() as Config) ?? new Config();
		Config.Initialize(PluginInterface);
		IPC.Initialize();
		AudioManager = new AudioManager();
		BlacklistManager = new BlacklistManager();
		Windows = new WindowSystem(Name);
		MainWindow mainWindow = new MainWindow(this);
		((Window)mainWindow).IsOpen = false;
		MainWindow = mainWindow;
		OverlayWindow overlayWindow = new OverlayWindow(this);
		((Window)overlayWindow).IsOpen = false;
		OverlayWindow = overlayWindow;
		Windows.AddWindow((IWindow)(object)MainWindow);
		Windows.AddWindow((IWindow)(object)OverlayWindow);
		UpdateFont();
		ContentFinderConditionsSheet = DataManager.GameData.GetExcelSheet<ContentFinderCondition>((Language?)null, (string)null) ?? null;
		PluginInterface.UiBuilder.DisableGposeUiHide = true;
		PluginInterface.UiBuilder.Draw += Windows.Draw;
		Framework.Update += new OnUpdateDelegate(Framework_Update);
		ClientState.TerritoryChanged += OnTerritoryChanged;
		ClientState.Login += ClientState_Login;
		ClientState.Logout += new LogoutDelegate(ClientState_Logout);
	}

	public void UpdateFont(bool delayUpdate = false)
	{
		OverlayWindow.UpdateFont(delayUpdate);
	}

	private void ClientState_Login()
	{
		OverlayWindow.TargetList.Clear();
	}

	private void ClientState_Logout(int type, int code)
	{
		OverlayWindow.TargetList.Clear();
	}

	private void OnTerritoryChanged(uint obj)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (ContentFinderConditionsSheet == null)
		{
			OverlayWindow.ContentType = TargetPyon.OverlayWindow.ContentTypes.NoDuty;
			return;
		}
		ContentFinderCondition? val = ((IEnumerable<ContentFinderCondition>)ContentFinderConditionsSheet).FirstOrDefault((ContentFinderCondition c) => ((ContentFinderCondition)(ref c)).TerritoryType.RowId == ClientState.TerritoryType);
		if (val.HasValue)
		{
			ContentFinderCondition value = val.Value;
			if (((ContentFinderCondition)(ref value)).RowId != 0)
			{
				OverlayWindow overlayWindow = OverlayWindow;
				value = val.Value;
				overlayWindow.ContentType = ((!((ContentFinderCondition)(ref value)).PvP) ? TargetPyon.OverlayWindow.ContentTypes.PvEDuty : TargetPyon.OverlayWindow.ContentTypes.PvPDuty);
				return;
			}
		}
		OverlayWindow.ContentType = TargetPyon.OverlayWindow.ContentTypes.NoDuty;
	}

	private void Framework_Update(IFramework framework)
	{
		if (Config.Enabled && Objects.LocalPlayer != null)
		{
			((Window)OverlayWindow).IsOpen = !IsOccupied();
			EntityManager.UpdatePlayerList();
			if (((Window)MainWindow).IsOpen)
			{
				EntityManager.UpdateObjectList();
			}
		}
		else if (((Window)OverlayWindow).IsOpen)
		{
			((Window)OverlayWindow).IsOpen = false;
		}
	}

	public void PlaySound(string name)
	{
		if (Config.UseCustomAudioAlert && AudioManager.AudioFileExists)
		{
			AudioManager.Play();
		}
		else if (Config.SoundID > 0 && Config.SoundID <= 16)
		{
			UIGlobals.PlayChatSoundEffect((uint)Config.SoundID);
		}
		if (name != "" && Config.ChatAlert && OverlayWindow.ContentType != TargetPyon.OverlayWindow.ContentTypes.PvPDuty)
		{
			ChatGui.Print("Targeted by " + name, (string)null, (ushort?)null);
		}
	}

	public bool IsOccupied()
	{
		if (!Condition[(ConditionFlag)25] && !Condition[(ConditionFlag)30] && !Condition[(ConditionFlag)33] && !Condition[(ConditionFlag)38] && !Condition[(ConditionFlag)39] && !Condition[(ConditionFlag)35] && !Condition[(ConditionFlag)31] && !Condition[(ConditionFlag)32] && !Condition[(ConditionFlag)58] && !Condition[(ConditionFlag)78] && !Condition[(ConditionFlag)45] && !Condition[(ConditionFlag)51] && !Condition[(ConditionFlag)70] && !Condition[(ConditionFlag)14] && !Condition[(ConditionFlag)12])
		{
			return Condition[(ConditionFlag)86];
		}
		return true;
	}

	public void Dispose()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		IPC.Dispose();
		AudioManager.Dispose();
		BlacklistManager.Dispose();
		ClientState.Login -= ClientState_Login;
		ClientState.Logout -= new LogoutDelegate(ClientState_Logout);
		ClientState.TerritoryChanged -= OnTerritoryChanged;
		Framework.Update -= new OnUpdateDelegate(Framework_Update);
		PluginInterface.UiBuilder.Draw -= Windows.Draw;
		CommandManager.RemoveHandler("/tar");
		CommandManager.RemoveHandler("/targetpyon");
		CommandManager.RemoveHandler("/tarvisible");
		CommandManager.RemoveHandler("/taroverlay");
	}

	private void OnCommand(string command, string args)
	{
		((Window)MainWindow).IsOpen = true;
	}

	private void OnVisCommand(string command, string args)
	{
		ToggleAllVisibility();
	}

	private void OnOverlayCommand(string command, string args)
	{
		Config.OverlayVisible = !Config.OverlayVisible;
		Config.Save();
	}

	public static void ToggleAllVisibility()
	{
		if (Objects.LocalPlayer == null)
		{
			return;
		}
		Config.PlayerVisibilityFilter = !Config.PlayerVisibilityFilter;
		Config.Save();
		EntityManager.UpdatePlayerVisibility();
		if (Config.PlayerVisibilityFilter)
		{
			return;
		}
		foreach (PlayerEntityInfo nearbyPlayer in EntityManager.NearbyPlayers)
		{
			if (nearbyPlayer.GameObject.GameObjectId != ((IGameObject)Objects.LocalPlayer).GameObjectId && !nearbyPlayer.IsVisible)
			{
				nearbyPlayer.Show();
			}
		}
	}

	public static void ToggleAllObjectVisibility()
	{
		if (Objects.LocalPlayer == null)
		{
			return;
		}
		Config.ObjectVisibilityFilter = !Config.ObjectVisibilityFilter;
		Config.Save();
		EntityManager.UpdateObjectVisibility();
		if (Config.ObjectVisibilityFilter)
		{
			return;
		}
		foreach (IObjectEntityInfo nearbyObject in EntityManager.NearbyObjects)
		{
			if (!nearbyObject.IsVisible)
			{
				nearbyObject.Show();
			}
		}
	}
}
