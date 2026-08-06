using System;
using System.Collections.Concurrent;
using System.Linq;
using Dalamud.Interface.Windowing;
using PyonCam.Config;
using PyonCam.Config.Cam;
using PyonCam.Services;
using PyonCam.UI.Windows;

namespace PyonCam.UI;

public sealed class WindowContext : IWindowContext
{
	private readonly ConcurrentDictionary<Type, object> _windows = new ConcurrentDictionary<Type, object>();

	private WindowSystem? _windowSystem;

	private Configuration _config;

	private IServiceContext _services;

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
		throw new InvalidOperationException("Window not registered: " + typeof(TWindow).Name);
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

	public void Initialize(Configuration config, IServiceContext services, bool isLoggedIn)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		_windowSystem = new WindowSystem("PyonCam");
		_config = config;
		_services = services;
		WindowSystem? windowSystem = _windowSystem;
		ConfigWindow configWindow = new ConfigWindow(config, services, this);
		((Window)configWindow).IsOpen = isLoggedIn && config.UI.Config.IsOpen;
		windowSystem.AddWindow((IWindow)(object)Register(configWindow));
		WindowSystem? windowSystem2 = _windowSystem;
		KeybindsWindow keybindsWindow = new KeybindsWindow(config, services, this);
		((Window)keybindsWindow).IsOpen = false;
		windowSystem2.AddWindow((IWindow)(object)Register(keybindsWindow));
	}

	public void Draw()
	{
		WindowSystem? windowSystem = _windowSystem;
		if (windowSystem != null)
		{
			windowSystem.Draw();
		}
	}

	public void OnCommand(string command, string args)
	{
		if (string.IsNullOrEmpty(args))
		{
			((Window)Get<ConfigWindow>()).Toggle();
			return;
		}
		args = args.Trim().ToLower();
		string[] argParts = args.Split(' ');
		Array.ForEach(argParts, delegate(string x)
		{
			x = x.Trim();
		});
		switch (argParts[0])
		{
		case "config":
			((Window)Get<ConfigWindow>()).Toggle();
			break;
		case "freecam":
			OnFreeCamCommand();
			break;
		case "noclip":
		{
			Configuration config = _config;
			config.EnableCameraNoClippy = !config.EnableCameraNoClippy;
			CameraService cameraService = _services.Get<CameraService>();
			if (!cameraService.FreeCam.Enabled)
			{
				cameraService.ToggleNoClip();
			}
			_config.Save();
			break;
		}
		case "spectate":
		{
			CameraService cameraService2 = _services.Get<CameraService>();
			cameraService2.SpectatingEnabled = !cameraService2.SpectatingEnabled;
			break;
		}
		case "preset":
			if (argParts.Length >= 2)
			{
				PresetService presetService = _services.Get<PresetService>();
				CameraConfigPreset cameraConfigPreset = _config.Presets.FirstOrDefault((CameraConfigPreset p) => p.Name.Equals(argParts[1], StringComparison.CurrentCultureIgnoreCase));
				if (cameraConfigPreset != null && _config.SelectedPresetID != cameraConfigPreset.ID)
				{
					_config.SelectedPresetID = cameraConfigPreset.ID;
					presetService.CurrentPreset = cameraConfigPreset;
					_config.Save();
				}
				else if (argParts[1] == "default")
				{
					_config.SelectedPresetID = Guid.Empty;
					presetService.CurrentPreset = presetService.DefaultPreset;
					_config.Save();
				}
			}
			break;
		}
	}

	public void OnFreeCamCommand(string? command = null, string? args = null)
	{
		if (_services.TryGet<CameraService>(out CameraService service))
		{
			service.FreeCam.Toggle();
		}
	}

	public void Dispose()
	{
		WindowSystem? windowSystem = _windowSystem;
		if (windowSystem != null)
		{
			windowSystem.RemoveAllWindows();
		}
		_windowSystem = null;
		_windows.Clear();
	}
}
