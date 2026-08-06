using System;
using System.Collections.Concurrent;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using PyonPix.Config;
using PyonPix.Services;
using PyonPix.Services.Core;
using PyonPix.Ui.Components;
using PyonPix.Ui.Windows;

namespace PyonPix.Ui;

public sealed class WindowContext : IWindowContext
{
	private readonly ConcurrentDictionary<Type, object> _windows = new ConcurrentDictionary<Type, object>();

	private WindowSystem? _windowSystem;

	private IServiceContext _services;

	private const string CommandName = "/pyonpix";

	private const string AltCommandName = "/pix";

	public static WindowContext Instance { get; private set; }

	public TWindow Register<TWindow>(TWindow window) where TWindow : class
	{
		_windows[typeof(TWindow)] = window;
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
		if (_windows.TryGetValue(typeof(TWindow), out object value) && value is TWindow val)
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
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Expected O, but got Unknown
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Expected O, but got Unknown
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Expected O, but got Unknown
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Expected O, but got Unknown
		_windowSystem = new WindowSystem("PyonPix");
		_services = services;
		bool isLoggedIn = _services.ClientState.IsLoggedIn;
		WindowSystem? windowSystem = _windowSystem;
		MainWindow mainWindow = new MainWindow(config, services, this);
		((Window)mainWindow).IsOpen = isLoggedIn && config.UI.Main.IsOpen;
		windowSystem.AddWindow((IWindow)(object)Register(mainWindow));
		WindowSystem? windowSystem2 = _windowSystem;
		BrowserWindow browserWindow = new BrowserWindow(config, services, this);
		((Window)browserWindow).IsOpen = isLoggedIn && config.UI.Browser.IsOpen;
		windowSystem2.AddWindow((IWindow)(object)Register(browserWindow));
		WindowSystem? windowSystem3 = _windowSystem;
		ExtensionsWindow extensionsWindow = new ExtensionsWindow(config, services, this);
		((Window)extensionsWindow).IsOpen = isLoggedIn && config.UI.Extensions.IsOpen;
		windowSystem3.AddWindow((IWindow)(object)Register(extensionsWindow));
		WindowSystem? windowSystem4 = _windowSystem;
		DataWindow dataWindow = new DataWindow(config, services, this);
		((Window)dataWindow).IsOpen = isLoggedIn && config.UI.Data.IsOpen;
		windowSystem4.AddWindow((IWindow)(object)Register(dataWindow));
		WindowSystem? windowSystem5 = _windowSystem;
		SyncSearchWindow syncSearchWindow = new SyncSearchWindow(config, services, this);
		((Window)syncSearchWindow).IsOpen = isLoggedIn && config.UI.SyncSearch.IsOpen;
		windowSystem5.AddWindow((IWindow)(object)Register(syncSearchWindow));
		WindowSystem? windowSystem6 = _windowSystem;
		ConfigWindow configWindow = new ConfigWindow(config, services, this);
		((Window)configWindow).IsOpen = isLoggedIn && config.UI.Config.IsOpen;
		windowSystem6.AddWindow((IWindow)(object)Register(configWindow));
		WindowSystem? windowSystem7 = _windowSystem;
		PixConfigWindow pixConfigWindow = new PixConfigWindow(config, services, this);
		((Window)pixConfigWindow).IsOpen = false;
		windowSystem7.AddWindow((IWindow)(object)Register(pixConfigWindow));
		WindowSystem? windowSystem8 = _windowSystem;
		PixMembersWindow pixMembersWindow = new PixMembersWindow(config, services, this);
		((Window)pixMembersWindow).IsOpen = false;
		windowSystem8.AddWindow((IWindow)(object)Register(pixMembersWindow));
		WindowSystem? windowSystem9 = _windowSystem;
		UserWindow userWindow = new UserWindow(config, services, this);
		((Window)userWindow).IsOpen = isLoggedIn && config.UI.User.IsOpen;
		windowSystem9.AddWindow((IWindow)(object)Register(userWindow));
		bool isOpen = false;
		if (config.UI.Updates.LastVersion != Plugin.Version.ToString())
		{
			config.UI.Updates.LastVersion = Plugin.Version.ToString();
			config.Save();
			if (config.UI.Updates.ShowUpdates)
			{
				isOpen = true;
			}
		}
		WindowSystem? windowSystem10 = _windowSystem;
		UpdatesWindow updatesWindow = new UpdatesWindow(config, services, this);
		((Window)updatesWindow).IsOpen = isOpen;
		windowSystem10.AddWindow((IWindow)(object)Register(updatesWindow));
		_services.CommandManager.AddHandler("/pyonpix", new CommandInfo(new HandlerDelegate(OnCommand))
		{
			HelpMessage = "Open Main Interface\n/pyonpix browser → Open Browser\n/pyonpix extensions → Open Extension Manager\n/pyonpix data → Open Data Manager\n/pyonpix sync → Open Sync Search\n/pyonpix config → Open Main Config\n/pyonpix user → Open User Config\n/pyonpix {PIXID} → Toggle Pix\n/pix → Alternative Command Alias"
		});
		_services.CommandManager.AddHandler("/pix", new CommandInfo(new HandlerDelegate(OnCommand))
		{
			ShowInHelp = false
		});
		_services.PluginInterface.UiBuilder.OpenMainUi += delegate
		{
			((Window)Get<MainWindow>()).Toggle();
		};
		_services.PluginInterface.UiBuilder.OpenConfigUi += delegate
		{
			((Window)Get<ConfigWindow>()).Toggle();
		};
		_services.PluginInterface.UiBuilder.DisableGposeUiHide = true;
		_services.PluginInterface.UiBuilder.Draw += Draw;
	}

	public void Draw()
	{
		WindowSystem? windowSystem = _windowSystem;
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
		default:
			if (string.IsNullOrWhiteSpace(args))
			{
				((Window)Get<MainWindow>()).Toggle();
			}
			else
			{
				_services.Get<PixService>().Toggle(args);
			}
			break;
		}
	}

	public void Dispose()
	{
		if (TryGet<BrowserWindow>(out BrowserWindow window))
		{
			window.Dispose();
		}
		_services.CommandManager.RemoveHandler("/pyonpix");
		_services.CommandManager.RemoveHandler("/pix");
		_services.PluginInterface.UiBuilder.Draw -= Draw;
		WindowSystem? windowSystem = _windowSystem;
		if (windowSystem != null)
		{
			windowSystem.RemoveAllWindows();
		}
		_windowSystem = null;
		_windows.Clear();
	}
}
