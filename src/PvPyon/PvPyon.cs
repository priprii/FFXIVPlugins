using System;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using PvPyon.Api;

namespace PvPyon;

public sealed class PvPyon : IDalamudPlugin, IDisposable
{
	private const string CommandName = "/pvpyon";

	private WindowSystem Windows;

	private static MainWindow MainWindow;

	private PluginData m_PluginData;

	private NameplateTagTargetModifier m_NameplateTagTargetModifier;

	public string Name => "PvPyon";

	public PvPyon(DalamudPluginInterface pluginInterface)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		PluginServices.Initialize(pluginInterface);
		global::PvPyon.Api.PluginServices.Initialize(pluginInterface);
		PluginServices.CommandManager.AddHandler("/pvpyon", new CommandInfo(new HandlerDelegate(OnCommand))
		{
			HelpMessage = "Open plugin window."
		});
		PluginServices.DalamudPluginInterface.UiBuilder.OpenConfigUi += delegate
		{
			((Window)MainWindow).IsOpen = true;
		};
		Windows = new WindowSystem(Name);
		MainWindow mainWindow = new MainWindow(this);
		((Window)mainWindow).IsOpen = false;
		MainWindow = mainWindow;
		global::PvPyon.MainWindow.Config = (PluginServices.DalamudPluginInterface.GetPluginConfig() as Config) ?? new Config();
		global::PvPyon.MainWindow.Config.Initialize(PluginServices.DalamudPluginInterface);
		Windows.AddWindow((Window)(object)MainWindow);
		MainWindow.Initialize();
		PluginServices.DalamudPluginInterface.UiBuilder.Draw += Windows.Draw;
		m_PluginData = new PluginData(global::PvPyon.MainWindow.Config);
		m_NameplateTagTargetModifier = new NameplateTagTargetModifier(global::PvPyon.MainWindow.Config, m_PluginData);
	}

	public void Dispose()
	{
		m_NameplateTagTargetModifier.Dispose();
		PluginServices.DalamudPluginInterface.UiBuilder.Draw -= Windows.Draw;
		MainWindow.Dispose();
		PluginServices.CommandManager.RemoveHandler("/pvpyon");
	}

	private void OnCommand(string command, string args)
	{
		((Window)MainWindow).IsOpen = true;
	}
}
