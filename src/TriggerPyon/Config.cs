using System;
using System.Collections.Generic;
using System.IO;
using Dalamud.Configuration;
using Dalamud.Plugin;
using Newtonsoft.Json;

namespace TriggerPyon;

[Serializable]
public class Config : IPluginConfiguration
{
	public bool Enabled = true;

	public bool ShowUpdates = true;

	public int UpdateIndex;

	[NonSerialized]
	private IDalamudPluginInterface? PluginInterface;

	public int Version { get; set; } = 1;

	public DiscordOptions Discord { get; set; } = new DiscordOptions();

	public int CounterCooldown { get; set; } = 500;

	public int CounterDuration { get; set; } = 5000;

	public List<Trigger> Triggers { get; set; } = new List<Trigger>();

	public void Initialize(IDalamudPluginInterface pluginInterface)
	{
		PluginInterface = pluginInterface;
	}

	public void Save()
	{
		FileInfo configFile = Plugin.PluginInterface.ConfigFile;
		if (configFile != null)
		{
			string contents = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
			{
				Converters = Plugin.Converters
			});
			File.WriteAllText(configFile.FullName, contents);
		}
	}
}
