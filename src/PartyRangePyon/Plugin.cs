using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Command;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Group;

namespace PartyRangePyon;

public sealed class Plugin : IDalamudPlugin, IDisposable
{
	private const string CommandName = "/partyrange";

	public static IFontHandle GameFont;

	private bool FontDirty;

	private DateTime LastFontUpdateTime = DateTime.Now;

	private WindowSystem Windows;

	private static MainWindow MainWindow;

	private DateTime LastFUpdateTime = DateTime.Now;

	private DateTime LastUpdateTime = DateTime.Now;

	public bool DebugMode;

	private readonly PartyMemberOverlay?[] PartyMembers = new PartyMemberOverlay[8];

	public string Name => "PartyRangePyon";

	[PluginService]
	public static IDalamudPluginInterface PluginInterface { get; private set; }

	[PluginService]
	public static ICommandManager CommandManager { get; private set; }

	[PluginService]
	public static IClientState ClientState { get; private set; }

	[PluginService]
	public static IFramework Framework { get; private set; }

	[PluginService]
	public static IObjectTable ObjectTable { get; private set; }

	[PluginService]
	public static IDutyState DutyState { get; private set; }

	[PluginService]
	public static IGameGui GameGui { get; private set; }

	[PluginService]
	public static IPluginLog Log { get; private set; }

	[PluginService]
	public static ITextureProvider TextureProvider { get; private set; }

	[PluginService]
	public static ICondition Condition { get; private set; }

	[PluginService]
	public static IPartyList PartyList { get; private set; }

	public static Config Config { get; set; }

	private unsafe static Span<PartyMember> PartyMemberSpan => ((Group)(&((GroupManager)GroupManager.Instance()).MainGroup)).PartyMembers;

	public Plugin(IDalamudPluginInterface pluginInterface)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Expected O, but got Unknown
		CommandManager.AddHandler("/partyrange", new CommandInfo(new HandlerDelegate(OnCommand))
		{
			HelpMessage = "Open PartyRangePyon Interface."
		});
		PluginInterface.UiBuilder.OpenConfigUi += delegate
		{
			((Window)MainWindow).IsOpen = true;
		};
		Config = (PluginInterface.GetPluginConfig() as Config) ?? new Config();
		Config.Initialize(PluginInterface);
		UpdateFont();
		Windows = new WindowSystem(Name);
		MainWindow mainWindow = new MainWindow(this);
		((Window)mainWindow).IsOpen = false;
		MainWindow = mainWindow;
		Windows.AddWindow((Window)(object)MainWindow);
		foreach (int item in Enumerable.Range(0, 8))
		{
			PartyMembers[item] = new PartyMemberOverlay(item);
		}
		ClientState.Login += ClientState_Login;
		ClientState.Logout += ClientState_Logout;
		PluginInterface.UiBuilder.Draw += Windows.Draw;
		PluginInterface.UiBuilder.Draw += OnDraw;
		Framework.Update += new OnUpdateDelegate(Framework_Update);
	}

	public void UpdateFont(bool delayUpdate = false)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (delayUpdate)
		{
			FontDirty = true;
			LastFontUpdateTime = DateTime.Now;
		}
		else
		{
			GameFont = PluginInterface.UiBuilder.FontAtlas.NewGameFontHandle(new GameFontStyle((GameFontFamily)((Config.Font < 1 || Config.Font > 6) ? 1 : Config.Font), (float)Config.FontSize));
		}
	}

	private void ClientState_Login()
	{
	}

	private void ClientState_Logout()
	{
		PartyMemberOverlay[] partyMembers = PartyMembers;
		for (int i = 0; i < partyMembers.Length; i++)
		{
			partyMembers[i]?.Reset();
		}
	}

	private void Framework_Update(IFramework framework)
	{
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Expected O, but got Unknown
		if (!Config.Enabled || ClientState.LocalPlayer == null)
		{
			return;
		}
		if (FontDirty && LastFontUpdateTime.AddMilliseconds(250.0) < DateTime.Now)
		{
			FontDirty = false;
			LastFontUpdateTime = DateTime.Now;
			UpdateFont();
		}
		if (!(LastFUpdateTime.AddMilliseconds(Config.UpdateMs) < DateTime.Now) || IsOccupied())
		{
			return;
		}
		if (DebugMode)
		{
			if (((IGameObject)ClientState.LocalPlayer).TargetObject != null)
			{
				PartyMembers[0].Distance = MathF.Max(0f, Vector3.Distance(((IGameObject)ClientState.LocalPlayer).TargetObject.Position, ((IGameObject)ClientState.LocalPlayer).Position) - (((IGameObject)ClientState.LocalPlayer).HitboxRadius + ((IGameObject)ClientState.LocalPlayer).TargetObject.HitboxRadius));
			}
			else
			{
				PartyMembers[0].Distance = 7.5f;
			}
		}
		else
		{
			PartyMemberOverlay[] partyMembers = PartyMembers;
			foreach (PartyMemberOverlay partyMemberOverlay in partyMembers)
			{
				if (partyMemberOverlay != null && !partyMemberOverlay.IsVisible)
				{
					continue;
				}
				bool flag = partyMemberOverlay == null;
				if (!flag)
				{
					bool flag2;
					switch (partyMemberOverlay?.ObjectId)
					{
					case 3758096384u:
					case 0u:
						flag2 = true;
						break;
					default:
						flag2 = false;
						break;
					}
					flag = flag2;
				}
				if (!flag)
				{
					IPlayerCharacter val = (IPlayerCharacter)ObjectTable.SearchById((ulong)(partyMemberOverlay?.ObjectId ?? 0));
					if (val != null)
					{
						partyMemberOverlay.Distance = MathF.Max(0f, Vector3.Distance(((IGameObject)val).Position, ((IGameObject)ClientState.LocalPlayer).Position) - (((IGameObject)ClientState.LocalPlayer).HitboxRadius + ((IGameObject)val).HitboxRadius));
					}
				}
			}
		}
		LastFUpdateTime = DateTime.Now;
	}

	private void OnDraw()
	{
		if (!Config.Enabled || ClientState.LocalPlayer == null || !(LastUpdateTime.AddMilliseconds(Config.UpdateMs) < DateTime.Now) || IsOccupied())
		{
			return;
		}
		PartyMemberOverlay[] partyMembers = PartyMembers;
		foreach (PartyMemberOverlay partyMemberOverlay in partyMembers)
		{
			if (partyMemberOverlay == null || partyMemberOverlay.IsVisible)
			{
				bool flag;
				switch (partyMemberOverlay?.ObjectId)
				{
				case 3758096384u:
				case 0u:
					flag = true;
					break;
				default:
					flag = false;
					break;
				}
				if (!flag && (partyMemberOverlay?.ObjectId != ((IGameObject)ClientState.LocalPlayer).GameObjectId || DebugMode))
				{
					partyMemberOverlay?.DrawRange();
				}
			}
		}
		LastUpdateTime = DateTime.Now;
	}

	public static bool IsOccupied()
	{
		if (!Condition[(ConditionFlag)25] && !Condition[(ConditionFlag)30] && !Condition[(ConditionFlag)33] && !Condition[(ConditionFlag)38] && !Condition[(ConditionFlag)39] && !Condition[(ConditionFlag)35] && !Condition[(ConditionFlag)31] && !Condition[(ConditionFlag)32] && !Condition[(ConditionFlag)50] && !Condition[(ConditionFlag)58] && !Condition[(ConditionFlag)78] && !Condition[(ConditionFlag)45] && !Condition[(ConditionFlag)51] && !Condition[(ConditionFlag)11] && !Condition[(ConditionFlag)37] && !Condition[(ConditionFlag)5] && !Condition[(ConditionFlag)40] && !Condition[(ConditionFlag)2] && !Condition[(ConditionFlag)7] && !Condition[(ConditionFlag)6] && !Condition[(ConditionFlag)42] && !Condition[(ConditionFlag)8] && !Condition[(ConditionFlag)65] && !Condition[(ConditionFlag)9] && !Condition[(ConditionFlag)70] && !Condition[(ConditionFlag)3] && !Condition[(ConditionFlag)10] && !Condition[(ConditionFlag)64] && !Condition[(ConditionFlag)71] && !Condition[(ConditionFlag)15] && !Condition[(ConditionFlag)14] && !Condition[(ConditionFlag)12] && !Condition[(ConditionFlag)13] && !Condition[(ConditionFlag)16] && !Condition[(ConditionFlag)41] && !Condition[(ConditionFlag)43] && !Condition[(ConditionFlag)68] && !Condition[(ConditionFlag)67])
		{
			IPlayerCharacter localPlayer = ClientState.LocalPlayer;
			if (localPlayer == null)
			{
				return true;
			}
			return !((IGameObject)localPlayer).IsTargetable;
		}
		return true;
	}

	private void OnCommand(string command, string args)
	{
		((Window)MainWindow).IsOpen = true;
	}

	public void Dispose()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		Framework.Update -= new OnUpdateDelegate(Framework_Update);
		ClientState.Login -= ClientState_Login;
		ClientState.Logout -= ClientState_Logout;
		PluginInterface.UiBuilder.Draw -= Windows.Draw;
		PluginInterface.UiBuilder.Draw -= OnDraw;
		((IDisposable)GameFont).Dispose();
		CommandManager.RemoveHandler("/partyrange");
	}
}
