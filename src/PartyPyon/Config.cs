using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace PartyPyon;

[Serializable]
public class Config : IPluginConfiguration
{
	[NonSerialized]
	private IDalamudPluginInterface? PluginInterface;

	public int Version { get; set; } = 1;

	public Guid? SelectedTemplate { get; set; }

	public Dictionary<Guid, string> Templates { get; set; } = new Dictionary<Guid, string>();

	public void Initialize(IDalamudPluginInterface pluginInterface)
	{
		PluginInterface = pluginInterface;
	}

	public void Save()
	{
		PluginInterface.SavePluginConfig((IPluginConfiguration)(object)this);
	}
}
