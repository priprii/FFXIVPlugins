using System;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace SoundPyon;

public class Plugin : IDalamudPlugin, IDisposable
{
	private const string CommandName = "/soundpyon";

	private const string AltCommandName = "/sound";

	private WindowSystem Windows;

	public static MainWindow MainWindow;

	public string Name => "SoundPyon";

	[PluginService]
	internal static IPluginLog Log { get; private set; }

	[PluginService]
	internal IDalamudPluginInterface PluginInterface { get; init; }

	[PluginService]
	internal ICommandManager CommandManager { get; init; }

	[PluginService]
	internal ISigScanner SigScanner { get; init; }

	[PluginService]
	internal IGameInteropProvider GameInteropProvider { get; init; }

	public static Config Config { get; set; }

	internal Filter Filter { get; }

	public Plugin()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected O, but got Unknown
		CommandManager.AddHandler("/soundpyon", new CommandInfo(new HandlerDelegate(OnCommand))
		{
			HelpMessage = "Open Plugin Interface."
		});
		CommandManager.AddHandler("/sound", new CommandInfo(new HandlerDelegate(OnCommand))
		{
			HelpMessage = "Open Plugin Interface."
		});
		PluginInterface.UiBuilder.OpenConfigUi += delegate
		{
			((Window)MainWindow).IsOpen = true;
		};
		Config = (PluginInterface.GetPluginConfig() as Config) ?? new Config();
		Config.Initialize(PluginInterface);
		Windows = new WindowSystem(Name);
		MainWindow mainWindow = new MainWindow(this);
		((Window)mainWindow).IsOpen = false;
		MainWindow = mainWindow;
		Windows.AddWindow((IWindow)(object)MainWindow);
		Filter = new Filter(this);
		if (Config.Enabled)
		{
			Filter.Enable();
		}
		PluginInterface.UiBuilder.DisableGposeUiHide = true;
		PluginInterface.UiBuilder.Draw += Windows.Draw;
	}

	private void OnCommand(string command, string args)
	{
		((Window)MainWindow).IsOpen = !((Window)MainWindow).IsOpen;
	}

	public void Dispose()
	{
		Filter.Dispose();
		PluginInterface.UiBuilder.Draw -= Windows.Draw;
		CommandManager.RemoveHandler("/soundpyon");
		CommandManager.RemoveHandler("/sound");
	}
}
