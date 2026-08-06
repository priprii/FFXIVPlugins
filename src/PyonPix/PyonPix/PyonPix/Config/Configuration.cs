using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;
using PyonPix.Config.Global;
using PyonPix.Config.Pix;
using PyonPix.Config.Sync;
using PyonPix.Config.UI;
using PyonPix.Structs.Browser;

namespace PyonPix.Config;

[Serializable]
public class Configuration : IPluginConfiguration
{
	public bool Enabled = true;

	public SyncProperties Sync = new SyncProperties();

	public GlobalProperties Global = new GlobalProperties();

	public UIProperties UI = new UIProperties();

	public Dictionary<long, Dictionary<string, PixVariant>> PixVariants = new Dictionary<long, Dictionary<string, PixVariant>>();

	public List<LocalPix> LocalPixs = new List<LocalPix>();

	public Dictionary<string, Extension> Extensions = new Dictionary<string, Extension>();

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

	public string GetConfigPath()
	{
		return PluginInterface.GetPluginConfigDirectory();
	}
}
