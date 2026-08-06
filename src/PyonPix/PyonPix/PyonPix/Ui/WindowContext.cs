using System;
using System.Collections.Concurrent;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using PyonPix.Config;
using PyonPix.Services;
using PyonPix.Services.Core;
using PyonPix.Services.Game;
using PyonPix.Shared.Structs.Territory;
using PyonPix.Ui.Components;
using PyonPix.Ui.Windows;

namespace PyonPix.Ui;

public sealed class WindowContext : IWindowContext
{
	private readonly ConcurrentDictionary<Type, object> Windows = new ConcurrentDictionary<Type, object>();

	private WindowSystem? WindowSystem;

	private Configuration Config;

	private IServiceContext Services;

	private StateService? StateService;

	private const string CommandName = "/pyonpix";

	private const string AltCommandName = "/pix";

	public static WindowContext Instance { get; private set; }

	public TWindow Register<TWindow>(TWindow window) where TWindow : class
	{
		Windows[typeof(TWindow)] = window;
		return window;
	}

	public TWindow Get<TWindow>() where TWindow : class
	{
		if (TryGet<TWindow>(out TWindow window) && window != null)
		{
			return window;
		}
		throw new InvalidOperationException("Window Failure: " + typeof(TWindow).Name);
	}

	public bool TryGet<TWindow>(out TWindow? window) where TWindow : class
	{
		if (Windows.TryGetValue(typeof(TWindow), out object value) && value is TWindow val)
		{
			window = val;
			return true;
		}
		window = null;
		return false;
	}

	public void Initialize(Configuration config, IServiceContext services)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Expected O, but got Unknown
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Expected O, but got Unknown
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Expected O, but got Unknown
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Expected O, but got Unknown
		WindowSystem = new WindowSystem("PyonPix");
		Config = config;
		Services = services;
		WindowSystem.AddWindow((IWindow)(object)Register(new MainWindow(config, services, this)));
		WindowSystem.AddWindow((IWindow)(object)Register(new BrowserWindow(config, services, this)));
		WindowSystem.AddWindow((IWindow)(object)Register(new ExtensionsWindow(config, services, this)));
		WindowSystem.AddWindow((IWindow)(object)Register(new DataWindow(config, services, this)));
		WindowSystem.AddWindow((IWindow)(object)Register(new SyncSearchWindow(config, services, this)));
		WindowSystem.AddWindow((IWindow)(object)Register(new ConfigWindow(config, services, this)));
		WindowSystem.AddWindow((IWindow)(object)Register(new PixConfigWindow(config, services, this)));
		WindowSystem.AddWindow((IWindow)(object)Register(new PixMembersWindow(config, services, this)));
		WindowSystem.AddWindow((IWindow)(object)Register(new UserWindow(config, services, this)));
		WindowSystem.AddWindow((IWindow)(object)Register(new SetupWindow(config, services, this)));
		WindowSystem.AddWindow((IWindow)(object)Register(new UpdatesWindow(config, services, this)));
		Services.CommandManager.AddHandler("/pyonpix", new CommandInfo(new HandlerDelegate(OnCommand))
		{
			HelpMessage = "Open Main Interface\n/pyonpix setup → Open Initial Setup\n/pyonpix changelog → Open Changelog\n/pyonpix browser → Open Browser\n/pyonpix extensions → Open Extension Manager\n/pyonpix data → Open Data Manager\n/pyonpix sync → Open Sync Search\n/pyonpix config → Open Main Config\n/pyonpix user → Open User Config\n/pyonpix {PIXID} → Toggle Pix\n/pix → Alternative Command Alias"
		});
		Services.CommandManager.AddHandler("/pix", new CommandInfo(new HandlerDelegate(OnCommand))
		{
			ShowInHelp = false
		});
		Services.PluginInterface.UiBuilder.OpenMainUi += delegate
		{
			((Window)Get<MainWindow>()).Toggle();
		};
		Services.PluginInterface.UiBuilder.OpenConfigUi += delegate
		{
			((Window)Get<ConfigWindow>()).Toggle();
		};
		Services.PluginInterface.UiBuilder.DisableGposeUiHide = true;
		Services.PluginInterface.UiBuilder.Draw += Draw;
		if (Services.TryGet<StateService>(out StateService))
		{
			StateService? stateService = StateService;
			if (stateService != null)
			{
				stateService.InitialLoad += StateService_InitialLoad;
			}
		}
	}

	private void StateService_InitialLoad(TerritoryData? territory)
	{
		((Window)Get<MainWindow>()).IsOpen = Config.UI.Main.IsOpen;
		((Window)Get<BrowserWindow>()).IsOpen = Config.UI.Browser.IsOpen;
		((Window)Get<ExtensionsWindow>()).IsOpen = Config.UI.Extensions.IsOpen;
		((Window)Get<DataWindow>()).IsOpen = Config.UI.Data.IsOpen;
		((Window)Get<SyncSearchWindow>()).IsOpen = Config.UI.SyncSearch.IsOpen;
		((Window)Get<ConfigWindow>()).IsOpen = Config.UI.Config.IsOpen;
		((Window)Get<PixConfigWindow>()).IsOpen = false;
		((Window)Get<PixMembersWindow>()).IsOpen = false;
		((Window)Get<UserWindow>()).IsOpen = Config.UI.User.IsOpen;
		bool flag = false;
		bool initialSetup = Config.UI.Setup.InitialSetup;
		((Window)Get<SetupWindow>()).IsOpen = initialSetup || Config.UI.Setup.IsOpen;
		if (initialSetup)
		{
			Config.UI.Setup.InitialSetup = false;
			flag = true;
		}
		bool flag2 = false;
		if (Config.UI.Updates.LastVersion != Plugin.Version.ToString())
		{
			Config.UI.Updates.LastVersion = Plugin.Version.ToString();
			if (Config.UI.Updates.ShowUpdates)
			{
				flag2 = true;
			}
			flag = true;
		}
		((Window)Get<UpdatesWindow>()).IsOpen = flag2 || Config.UI.Updates.IsOpen;
		if (flag)
		{
			Config.Save();
		}
		StateService? stateService = StateService;
		if (stateService != null)
		{
			stateService.InitialLoad -= StateService_InitialLoad;
		}
	}

	public void Draw()
	{
		WindowSystem? windowSystem = WindowSystem;
		if (windowSystem != null)
		{
			windowSystem.Draw();
		}
		Tooltip.Draw();
	}

	public void OnCommand(string command, string args)
	{
		args = args.Trim().ToLower();
		switch (args)
		{
		case "browser":
		{
			BrowserWindow browserWindow = Get<BrowserWindow>();
			((Window)browserWindow).Toggle();
			if (!((Window)browserWindow).IsOpen)
			{
				browserWindow.OnCloseUserInteraction();
			}
			break;
		}
		case "extensions":
			((Window)Get<ExtensionsWindow>()).Toggle();
			break;
		case "data":
			((Window)Get<DataWindow>()).Toggle();
			break;
		case "sync":
			((Window)Get<SyncSearchWindow>()).Toggle();
			break;
		case "config":
			((Window)Get<ConfigWindow>()).Toggle();
			break;
		case "user":
			((Window)Get<UserWindow>()).Toggle();
			break;
		case "setup":
			((Window)Get<SetupWindow>()).Toggle();
			break;
		case "changelog":
			((Window)Get<UpdatesWindow>()).Toggle();
			break;
		default:
		{
			PixService service;
			if (string.IsNullOrWhiteSpace(args))
			{
				((Window)Get<MainWindow>()).Toggle();
			}
			else if (Services.TryGet<PixService>(out service))
			{
				service.Toggle(args);
			}
			break;
		}
		}
	}

	public void Dispose()
	{
		if (TryGet<BrowserWindow>(out BrowserWindow window))
		{
			window.Dispose();
		}
		Services.CommandManager.RemoveHandler("/pyonpix");
		Services.CommandManager.RemoveHandler("/pix");
		Services.PluginInterface.UiBuilder.Draw -= Draw;
		WindowSystem? windowSystem = WindowSystem;
		if (windowSystem != null)
		{
			windowSystem.RemoveAllWindows();
		}
		WindowSystem = null;
		Windows.Clear();
	}
}
