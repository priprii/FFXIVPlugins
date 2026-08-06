using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin;
using PyonCam.Config.Cam;
using PyonCam.Config.UI;
using PyonCam.Services;

namespace PyonCam.Config;

[Serializable]
public class Configuration : IPluginConfiguration
{
	public bool Enabled = true;

	public UIProperties UI = new UIProperties();

	public Guid SelectedPresetID = Guid.Empty;

	public List<CameraConfigPreset> Presets = new List<CameraConfigPreset>();

	public Dictionary<KeybindInput, VirtualKey> Keybinds = new Dictionary<KeybindInput, VirtualKey>
	{
		{
			KeybindInput.Forward,
			(VirtualKey)87
		},
		{
			KeybindInput.Left,
			(VirtualKey)65
		},
		{
			KeybindInput.Back,
			(VirtualKey)83
		},
		{
			KeybindInput.Right,
			(VirtualKey)68
		},
		{
			KeybindInput.Ascend,
			(VirtualKey)32
		},
		{
			KeybindInput.Descend,
			(VirtualKey)81
		},
		{
			KeybindInput.Fast_Speed,
			(VirtualKey)16
		},
		{
			KeybindInput.Slow_Speed,
			(VirtualKey)17
		}
	};

	public bool EnableCameraNoClippy;

	public DeathCamSetting DeathCamMode;

	public bool DisableCullingInGpose = true;

	public bool DisableCullingInFreeCam = true;

	[NonSerialized]
	private IDalamudPluginInterface PluginInterface;

	public int Version { get; set; } = 1;

	public void Initialize(IDalamudPluginInterface pi)
	{
		PluginInterface = pi;
	}

	public void Save()
	{
		PluginInterface.SavePluginConfig((IPluginConfiguration)(object)this);
	}
}
