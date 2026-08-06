using System;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace PvPyon;

[Serializable]
public class Config : IPluginConfiguration
{
	[NonSerialized]
	private DalamudPluginInterface? PluginInterface;

	public int Version { get; set; }

	public bool Debug { get; set; }

	public bool Enabled { get; set; } = true;

	public bool FilterPlayers { get; set; } = true;

	public string IncludedNames { get; set; } = "";

	public bool ColourDeadNameplate { get; set; }

	public bool StatusIconToNameplateText { get; set; }

	public void Initialize(DalamudPluginInterface pluginInterface)
	{
		PluginInterface = pluginInterface;
	}

	public void Save()
	{
		PluginInterface.SavePluginConfig((IPluginConfiguration)(object)this);
	}
}
