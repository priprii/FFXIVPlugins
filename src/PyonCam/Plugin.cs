using System;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using PyonCam.Config;
using PyonCam.Services;
using PyonCam.UI;
using PyonCam.UI.Windows;

namespace PyonCam;

public class Plugin : IDalamudPlugin, IDisposable
{
	public const string Name = "PyonCam";

	private const string CommandName = "/pyoncam";

	private const string AltCommandName = "/cam";

	private const string FreeCamCommandName = "/freecam";

	private readonly Configuration Config;

	private readonly ServiceContext Services;

	private readonly WindowContext Windows;

	public static Version Version { get; private set; }

	public Plugin(IDalamudPluginInterface pi)
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Expected O, but got Unknown
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Expected O, but got Unknown
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Expected O, but got Unknown
		Services = new ServiceContext(pi);
		Windows = new WindowContext();
		Version = Services.PluginInterface.Manifest.AssemblyVersion;
		Config = (Services.PluginInterface.GetPluginConfig() as Configuration) ?? new Configuration();
		Config.Initialize(Services.PluginInterface);
		Services.Initialize(Config);
		Windows.Initialize(Config, Services, Services.ClientState.IsLoggedIn);
		ICommandManager commandManager = Services.CommandManager;
		WindowContext windows = Windows;
		commandManager.AddHandler("/pyoncam", new CommandInfo(new HandlerDelegate(windows.OnCommand))
		{
			HelpMessage = "Open Main Interface\n/pyoncam freecam (/freecam) → Toggle FreeCam\n/pyoncam noclip → Toggle Camera Collision\n/pyoncam spectate → Toggle Spectating\n/pyoncam preset {Name} → Set active preset to specified\n/pyoncam preset default → Set active preset to default\n/cam → Alternative Command Alias"
		});
		ICommandManager commandManager2 = Services.CommandManager;
		WindowContext windows2 = Windows;
		commandManager2.AddHandler("/cam", new CommandInfo(new HandlerDelegate(windows2.OnCommand))
		{
			ShowInHelp = false
		});
		Services.CommandManager.AddHandler("/freecam", new CommandInfo(new HandlerDelegate(Windows.OnFreeCamCommand))
		{
			ShowInHelp = false
		});
		Services.PluginInterface.UiBuilder.OpenMainUi += delegate
		{
			((Window)Windows.Get<ConfigWindow>()).Toggle();
		};
		Services.PluginInterface.UiBuilder.OpenConfigUi += delegate
		{
			((Window)Windows.Get<ConfigWindow>()).Toggle();
		};
		Services.PluginInterface.UiBuilder.DisableGposeUiHide = true;
		Services.PluginInterface.UiBuilder.Draw += Windows.Draw;
		IFramework framework = Services.Framework;
		ServiceContext services = Services;
		framework.Update += new OnUpdateDelegate(services.Update);
	}

	public void Dispose()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		IFramework framework = Services.Framework;
		ServiceContext services = Services;
		framework.Update -= new OnUpdateDelegate(services.Update);
		Services.PluginInterface.UiBuilder.Draw -= Windows.Draw;
		Services.CommandManager.RemoveHandler("/pyoncam");
		Services.CommandManager.RemoveHandler("/cam");
		Services.CommandManager.RemoveHandler("/freecam");
		Services.Dispose();
		Windows.Dispose();
	}
}
