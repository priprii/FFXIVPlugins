using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Hypostasis.Dalamud;
using Hypostasis.Game;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Extensions;
using Lumina.Text.ReadOnly;
using Newtonsoft.Json;

namespace TriggerPyon;

public class Plugin : IDalamudPlugin, IDisposable
{
	private const string CommandName = "/triggerpyon";

	private const string AltCommandName = "/trigger";

	private int UpdateIndex = 12;

	public List<Emote> Emotes = new List<Emote>();

	public static List<SpecialEmote> SpecialEmotes = new List<SpecialEmote>();

	public static List<string> Worlds = new List<string>();

	public static HashSet<ResidentialTerritory> ResidentialTerritories = new HashSet<ResidentialTerritory>();

	public static HashSet<NonResidentialTerritory> NonResidentialTerritories = new HashSet<NonResidentialTerritory>();

	public static IList<JsonConverter> Converters = new List<JsonConverter>
	{
		new ActionBaseConverter(),
		new CounterBaseConverter(),
		new ReceiverBaseConverter(),
		new ReactionBaseConverter()
	};

	private WindowSystem Windows;

	public MainWindow MainWindow;

	public UpdatesWindow UpdatesWindow;

	public string Name => "TriggerPyon";

	[PluginService]
	internal static IPluginLog Log { get; private set; } = null;

	[PluginService]
	internal static IClientState ClientState { get; private set; } = null;

	[PluginService]
	internal static IDalamudPluginInterface PluginInterface { get; private set; } = null;

	[PluginService]
	internal ICommandManager CommandManager { get; init; }

	[PluginService]
	internal static IGameInteropProvider GameInteropProvider { get; private set; } = null;

	[PluginService]
	internal ICondition Condition { get; init; }

	[PluginService]
	internal static IFramework Framework { get; private set; } = null;

	[PluginService]
	internal static IObjectTable Objects { get; private set; } = null;

	[PluginService]
	internal static ITargetManager Targets { get; private set; } = null;

	[PluginService]
	internal static IDataManager DataManager { get; private set; } = null;

	[PluginService]
	internal static IGameGui GameGui { get; private set; } = null;

	[PluginService]
	internal static IChatGui ChatGui { get; private set; } = null;

	[PluginService]
	internal static IToastGui ToastGui { get; private set; } = null;

	[PluginService]
	internal ISigScanner ISigScanner { get; init; }

	public SigScannerWrapper SigScanner { get; private set; }

	public ExcelSheet<TerritoryType> TerritorySheet { get; init; }

	public ExcelSheet<Emote> EmoteSheet { get; init; }

	public Honorific? Honorific { get; private set; }

	public EmoteHook EmoteHook { get; private set; }

	public Chat Chat { get; init; }

	public DiscordManager DiscordManager { get; private set; }

	public TriggerManager TriggerManager { get; private set; }

	public static Config Config { get; private set; } = null;

	public Plugin()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Expected O, but got Unknown
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Expected O, but got Unknown
		CommandManager.AddHandler("/triggerpyon", new CommandInfo(new HandlerDelegate(OnCommand))
		{
			HelpMessage = "Open Plugin Interface."
		});
		CommandManager.AddHandler("/trigger", new CommandInfo(new HandlerDelegate(OnCommand))
		{
			HelpMessage = "Open Plugin Interface."
		});
		PluginInterface.UiBuilder.OpenConfigUi += delegate
		{
			((Window)MainWindow).IsOpen = true;
		};
		SigScanner = new SigScannerWrapper(ISigScanner);
		SigScanner.InjectSignatures();
		Common.Initialize();
		Config = LoadConfig();
		Windows = new WindowSystem(Name);
		MainWindow mainWindow = new MainWindow(this);
		((Window)mainWindow).IsOpen = false;
		MainWindow = mainWindow;
		UpdatesWindow updatesWindow = new UpdatesWindow(this);
		((Window)updatesWindow).IsOpen = false;
		UpdatesWindow = updatesWindow;
		Windows.AddWindow((IWindow)(object)MainWindow);
		Windows.AddWindow((IWindow)(object)UpdatesWindow);
		TerritorySheet = DataManager.GetExcelSheet<TerritoryType>((ClientLanguage?)null, (string)null);
		EmoteSheet = DataManager.GetExcelSheet<Emote>((ClientLanguage?)null, (string)null);
		InitializeEmotes();
		InitializeWorlds();
		InitializeTerritories();
		Mare.Initialize();
		Honorific = new Honorific();
		EmoteHook = new EmoteHook(this);
		Chat = new Chat(this);
		ChatGui.ChatMessage += new OnHandleableChatMessageDelegate(Chat.OnChatMessage);
		DiscordManager = new DiscordManager(this);
		TriggerManager = new TriggerManager(this);
		PluginInterface.UiBuilder.DisableGposeUiHide = true;
		PluginInterface.UiBuilder.Draw += Windows.Draw;
		Framework.Update += new OnUpdateDelegate(Framework_Update);
		DiscordManager.ConnectIfAnyTriggerEnabled();
		if (Config.UpdateIndex != UpdateIndex)
		{
			Config.UpdateIndex = UpdateIndex;
			Config.Save();
			if (Config.ShowUpdates)
			{
				((Window)UpdatesWindow).IsOpen = true;
			}
		}
	}

	private void Framework_Update(IFramework framework)
	{
		PlayerManager.UpdatePlayerList();
		TriggerManager.Update();
		Honorific?.Update();
	}

	private Config LoadConfig()
	{
		FileInfo configFile = PluginInterface.ConfigFile;
		if (configFile != null && configFile.Exists)
		{
			return JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFile.FullName), new JsonSerializerSettings
			{
				Converters = Converters
			}) ?? new Config();
		}
		return new Config();
	}

	private void InitializeEmotes()
	{
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		SpecialEmotes = new List<SpecialEmote>
		{
			new SpecialEmote(0, "[P] Idle 0", triggersEmoteHook: false),
			new SpecialEmote(91, "[P] Idle 1", triggersEmoteHook: false),
			new SpecialEmote(92, "[P] Idle 2", triggersEmoteHook: false),
			new SpecialEmote(107, "[P] Idle 3", triggersEmoteHook: false),
			new SpecialEmote(108, "[P] Idle 4", triggersEmoteHook: false),
			new SpecialEmote(218, "[P] Idle 5", triggersEmoteHook: false),
			new SpecialEmote(219, "[P] Idle 6", triggersEmoteHook: false),
			new SpecialEmote(52, "[P] Sit Ground 0", triggersEmoteHook: true),
			new SpecialEmote(97, "[P] Sit Ground 1", triggersEmoteHook: false),
			new SpecialEmote(98, "[P] Sit Ground 2", triggersEmoteHook: false),
			new SpecialEmote(117, "[P] Sit Ground 3", triggersEmoteHook: false),
			new SpecialEmote(50, "[P] Sit Chair 0", triggersEmoteHook: true),
			new SpecialEmote(95, "[P] Sit Chair 1 (Anywhere)", triggersEmoteHook: true),
			new SpecialEmote(96, "[P] Sit Chair 2", triggersEmoteHook: false),
			new SpecialEmote(254, "[P] Sit Chair 3", triggersEmoteHook: false),
			new SpecialEmote(255, "[P] Sit Chair 4", triggersEmoteHook: false),
			new SpecialEmote(88, "[P] Sleep 0", triggersEmoteHook: true),
			new SpecialEmote(99, "[P] Sleep 1", triggersEmoteHook: false),
			new SpecialEmote(100, "[P] Sleep 2", triggersEmoteHook: false),
			new SpecialEmote(51, "[P] Stand (Chair)", triggersEmoteHook: true),
			new SpecialEmote(53, "[P] Stand (Ground)", triggersEmoteHook: true),
			new SpecialEmote(89, "[P] Stand (Sleep)", triggersEmoteHook: true)
		};
		Enumerator<Emote> enumerator = EmoteSheet.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Emote exEmote = enumerator.Current;
				string text = ((object)((Emote)(ref exEmote)).Name/*cast due to constrained. prefix*/).ToString();
				SpecialEmote specialEmote = SpecialEmotes.FirstOrDefault((SpecialEmote x) => x.ID == ((Emote)(ref exEmote)).RowId);
				List<Emote> emotes = Emotes;
				ushort id = (ushort)((Emote)(ref exEmote)).RowId;
				string name = ((((Emote)(ref exEmote)).RowId == 146) ? "Dote (Targeted)" : ((((Emote)(ref exEmote)).RowId == 147) ? "Dote (Untargeted)" : ((specialEmote != null && ((Emote)(ref exEmote)).RowId == specialEmote.ID) ? specialEmote.Name : (string.IsNullOrWhiteSpace(text) ? $"Unknown-{((Emote)(ref exEmote)).RowId}" : text))));
				TextCommand? valueNullable = ((Emote)(ref exEmote)).TextCommand.ValueNullable;
				object command;
				if (!valueNullable.HasValue)
				{
					command = null;
				}
				else
				{
					TextCommand valueOrDefault = valueNullable.GetValueOrDefault();
					command = ((object)((TextCommand)(ref valueOrDefault)).Command/*cast due to constrained. prefix*/).ToString();
				}
				emotes.Add(new Emote(id, name, (string?)command, specialEmote != null, specialEmote?.TriggersEmoteHook ?? true));
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		Emotes.Sort((Emote a, Emote b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
	}

	private unsafe void InitializeWorlds()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<World> enumerator = DataManager.GetExcelSheet<World>((ClientLanguage?)null, (string)null).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				World current = enumerator.Current;
				WorldDCGroupType? valueNullable = ((World)(ref current)).DataCenter.ValueNullable;
				uint? num;
				if (!valueNullable.HasValue)
				{
					num = null;
				}
				else
				{
					WorldDCGroupType valueOrDefault = valueNullable.GetValueOrDefault();
					num = ((WorldDCGroupType)(ref valueOrDefault)).Region.RowId;
				}
				uint? num2 = num;
				if (num2 < 1 || num2 > 4)
				{
					continue;
				}
				ReadOnlySeString name = ((World)(ref current)).Name;
				if (!((ReadOnlySeString)(ref name)).IsEmpty)
				{
					name = ((World)(ref current)).Name;
					if (!((object)(*(ReadOnlySeString*)(&name))/*cast due to constrained. prefix*/).ToString().Contains('-'))
					{
						List<string> worlds = Worlds;
						name = ((World)(ref current)).Name;
						worlds.Add(((object)(*(ReadOnlySeString*)(&name))/*cast due to constrained. prefix*/).ToString());
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private void InitializeTerritories()
	{
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
		ResidentialTerritories = new HashSet<ResidentialTerritory>
		{
			new ResidentialTerritory(136u, "Mist", ResidentialType.Ward),
			new ResidentialTerritory(282u, "Private Cottage - Mist", ResidentialType.House),
			new ResidentialTerritory(283u, "Private House - Mist", ResidentialType.House),
			new ResidentialTerritory(284u, "Private Mansion - Mist", ResidentialType.House),
			new ResidentialTerritory(384u, "Private Chambers - Mist", ResidentialType.Chambers),
			new ResidentialTerritory(423u, "Company Workshop - Mist", ResidentialType.Workshop),
			new ResidentialTerritory(573u, "Topmast Apartment Lobby", ResidentialType.ApartmentLobby),
			new ResidentialTerritory(608u, "Topmast Apartment", ResidentialType.Apartment),
			new ResidentialTerritory(340u, "The Lavender Beds", ResidentialType.Ward),
			new ResidentialTerritory(342u, "Private Cottage - The Lavender Beds", ResidentialType.House),
			new ResidentialTerritory(343u, "Private House - The Lavender Beds", ResidentialType.House),
			new ResidentialTerritory(344u, "Private Mansion - The Lavender Beds", ResidentialType.House),
			new ResidentialTerritory(385u, "Private Chambers - The Lavender Beds", ResidentialType.Chambers),
			new ResidentialTerritory(425u, "Company Workshop - The Lavender Beds", ResidentialType.Workshop),
			new ResidentialTerritory(574u, "Lily Hills Apartment Lobby", ResidentialType.ApartmentLobby),
			new ResidentialTerritory(609u, "Lily Hills Apartment", ResidentialType.Apartment),
			new ResidentialTerritory(341u, "The Goblet", ResidentialType.Ward),
			new ResidentialTerritory(345u, "Private Cottage - The Goblet", ResidentialType.House),
			new ResidentialTerritory(346u, "Private House -  The Goblet", ResidentialType.House),
			new ResidentialTerritory(347u, "Private Mansion -  The Goblet", ResidentialType.House),
			new ResidentialTerritory(386u, "Private Chambers - The Goblet", ResidentialType.Chambers),
			new ResidentialTerritory(424u, "Company Workshop - The Goblet", ResidentialType.Workshop),
			new ResidentialTerritory(575u, "Sultana's Breath Apartment Lobby", ResidentialType.ApartmentLobby),
			new ResidentialTerritory(610u, "Sultana's Breath Apartment", ResidentialType.Apartment),
			new ResidentialTerritory(641u, "Shirogane", ResidentialType.Ward),
			new ResidentialTerritory(649u, "Private Cottage - Shirogane", ResidentialType.House),
			new ResidentialTerritory(650u, "Private House - Shirogane", ResidentialType.House),
			new ResidentialTerritory(651u, "Private Mansion - Shirogane", ResidentialType.House),
			new ResidentialTerritory(652u, "Private Chambers - Shirogane", ResidentialType.Chambers),
			new ResidentialTerritory(653u, "Company Workshop - Shirogane", ResidentialType.Workshop),
			new ResidentialTerritory(654u, "Kobai Goten Apartment Lobby", ResidentialType.ApartmentLobby),
			new ResidentialTerritory(655u, "Kobai Goten Apartment", ResidentialType.Apartment),
			new ResidentialTerritory(979u, "Empyreum", ResidentialType.Ward),
			new ResidentialTerritory(980u, "Private Cottage - Empyreum", ResidentialType.House),
			new ResidentialTerritory(981u, "Private House - Empyreum", ResidentialType.House),
			new ResidentialTerritory(982u, "Private Mansion - Empyreum", ResidentialType.House),
			new ResidentialTerritory(983u, "Private Chambers - Empyreum", ResidentialType.Chambers),
			new ResidentialTerritory(984u, "Company Workshop - Empyreum", ResidentialType.Workshop),
			new ResidentialTerritory(985u, "Ingleside Apartment Lobby", ResidentialType.ApartmentLobby),
			new ResidentialTerritory(999u, "Ingleside Apartment", ResidentialType.Apartment)
		};
		TerritoryType val = default(TerritoryType);
		PlaceName valueOrDefault;
		foreach (ResidentialTerritory rt in ResidentialTerritories)
		{
			if (LinqExtensions.TryGetFirst<TerritoryType>((IEnumerable<TerritoryType>)TerritorySheet, (Predicate<TerritoryType>)((TerritoryType x) => ((TerritoryType)(ref x)).RowId == rt.Id), ref val))
			{
				PlaceName? valueNullable = ((TerritoryType)(ref val)).PlaceName.ValueNullable;
				object obj;
				if (!valueNullable.HasValue)
				{
					obj = null;
				}
				else
				{
					valueOrDefault = valueNullable.GetValueOrDefault();
					obj = SeStringExtensions.ToDalamudString(((PlaceName)(ref valueOrDefault)).Name).TextValue;
				}
				string text = (string)obj;
				rt.Name = text ?? rt.Name;
			}
		}
		Enumerator<TerritoryType> enumerator2 = TerritorySheet.GetEnumerator();
		try
		{
			while (enumerator2.MoveNext())
			{
				TerritoryType ter = enumerator2.Current;
				PlaceName? valueNullable = ((TerritoryType)(ref ter)).PlaceName.ValueNullable;
				object obj2;
				if (!valueNullable.HasValue)
				{
					obj2 = null;
				}
				else
				{
					valueOrDefault = valueNullable.GetValueOrDefault();
					ReadOnlySeString name = ((PlaceName)(ref valueOrDefault)).Name;
					obj2 = ((ReadOnlySeString)(ref name)).ExtractText();
				}
				string text2 = (string)obj2;
				if (!string.IsNullOrWhiteSpace(text2) && ResidentialTerritories.FirstOrDefault((ResidentialTerritory x) => x.Id == ((TerritoryType)(ref ter)).RowId) == null)
				{
					NonResidentialTerritories.Add(new NonResidentialTerritory(((TerritoryType)(ref ter)).RowId, text2));
				}
			}
		}
		finally
		{
			((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public bool TryGetCurrentTerritory(out TerritoryType res)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		res = default(TerritoryType);
		return LinqExtensions.TryGetFirst<TerritoryType>((IEnumerable<TerritoryType>)TerritorySheet, (Predicate<TerritoryType>)((TerritoryType x) => ((TerritoryType)(ref x)).RowId == ClientState.TerritoryType), ref res);
	}

	private void OnCommand(string command, string args)
	{
		((Window)MainWindow).IsOpen = !((Window)MainWindow).IsOpen;
	}

	public bool HasInvalidConditionForTitle()
	{
		if (!Condition[(ConditionFlag)70] && !Condition[(ConditionFlag)45] && !Condition[(ConditionFlag)51] && !Condition[(ConditionFlag)86] && !Condition[(ConditionFlag)53] && !Condition[(ConditionFlag)35] && !Condition[(ConditionFlag)31] && !Condition[(ConditionFlag)32] && !Condition[(ConditionFlag)92] && !Condition[(ConditionFlag)93] && !Condition[(ConditionFlag)58])
		{
			return Condition[(ConditionFlag)78];
		}
		return true;
	}

	public void Dispose()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		DiscordManager.Disconnect();
		DiscordManager.Dispose();
		ChatGui.ChatMessage -= new OnHandleableChatMessageDelegate(Chat.OnChatMessage);
		Framework.Update -= new OnUpdateDelegate(Framework_Update);
		PluginInterface.UiBuilder.Draw -= Windows.Draw;
		Windows.RemoveAllWindows();
		CommandManager.RemoveHandler("/triggerpyon");
		CommandManager.RemoveHandler("/trigger");
		TriggerManager.Dispose();
		EmoteHook.Dispose();
		Honorific?.Dispose();
		Mare.Dispose();
		Common.Dispose();
		SigScanner.Dispose();
	}
}
