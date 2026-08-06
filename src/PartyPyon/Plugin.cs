using System;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace PartyPyon;

public sealed class Plugin : IDalamudPlugin, IDisposable
{
	private const string Name = "PartyPyon";

	private const string CommandName = "/partypyon";

	private WindowSystem Windows;

	public MainWindow MainWindow;

	[PluginService]
	public static IDalamudPluginInterface PluginInterface { get; private set; }

	[PluginService]
	public static ICommandManager CommandManager { get; private set; }

	[PluginService]
	public static IPlayerState PlayerState { get; private set; }

	[PluginService]
	public static IObjectTable ObjectTable { get; private set; }

	[PluginService]
	public static IFramework Framework { get; private set; }

	[PluginService]
	public static ICondition Condition { get; private set; }

	[PluginService]
	public static IGameGui GameGui { get; private set; }

	[PluginService]
	public static IPartyFinderGui PartyFinderGui { get; private set; }

	[PluginService]
	public static ISigScanner SigScanner { get; private set; }

	[PluginService]
	public static IPluginLog PluginLog { get; private set; }

	public Config Config { get; init; }

	public PFManager PFManager { get; init; }

	public Plugin()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Expected O, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		CommandManager.AddHandler("/partypyon", new CommandInfo(new HandlerDelegate(OnCommand))
		{
			HelpMessage = "Open plugin window."
		});
		Config = (PluginInterface.GetPluginConfig() as Config) ?? new Config();
		if (Config.Version < 1)
		{
			Config = new Config();
			Config.Initialize(PluginInterface);
			Config.Save();
		}
		else
		{
			Config.Initialize(PluginInterface);
		}
		PFManager = new PFManager(this);
		PFManager.Initialize();
		Windows = new WindowSystem("PartyPyon");
		MainWindow mainWindow = new MainWindow(this, PFManager);
		((Window)mainWindow).IsOpen = false;
		MainWindow = mainWindow;
		Windows.AddWindow((IWindow)(object)MainWindow);
		PluginInterface.UiBuilder.Draw += Windows.Draw;
		PluginInterface.UiBuilder.OpenConfigUi += delegate
		{
			((Window)MainWindow).IsOpen = true;
		};
		PartyFinderGui.ReceiveListing += new PartyFinderListingEventDelegate(PFManager.OnListing);
		Framework.Update += new OnUpdateDelegate(PFManager.Framework_Update);
	}

	public void Dispose()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		PluginInterface.UiBuilder.Draw -= Windows.Draw;
		Framework.Update -= new OnUpdateDelegate(PFManager.Framework_Update);
		PartyFinderGui.ReceiveListing -= new PartyFinderListingEventDelegate(PFManager.OnListing);
		PFManager.Dispose();
		CommandManager.RemoveHandler("/partypyon");
	}

	private void OnCommand(string command, string args)
	{
		((Window)MainWindow).IsOpen = true;
	}
}
