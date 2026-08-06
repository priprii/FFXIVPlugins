using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;
using DotNet.Globbing;

namespace SoundPyon;

[Serializable]
public class Config : IPluginConfiguration
{
	public bool Enabled = true;

	public uint LogLimit = 50u;

	[NonSerialized]
	private IDalamudPluginInterface? PluginInterface;

	public int Version { get; set; } = 1;

	private Dictionary<string, Glob> CachedGlobs { get; } = new Dictionary<string, Glob>();

	public List<FilterGroup> Filters { get; set; } = new List<FilterGroup>();

	internal IReadOnlyDictionary<Glob, bool> Globs
	{
		get
		{
			Dictionary<Glob, bool> dictionary = new Dictionary<Glob, bool>();
			foreach (FilterGroup filter in Filters)
			{
				foreach (string glob2 in filter.Globs)
				{
					if (CachedGlobs.TryGetValue(glob2, out Glob value))
					{
						dictionary[value] = filter.Enabled;
						continue;
					}
					Glob glob = Glob.Parse(glob2);
					CachedGlobs[glob2] = glob;
					dictionary[glob] = filter.Enabled;
				}
			}
			return dictionary;
		}
	}

	public void Initialize(IDalamudPluginInterface pluginInterface)
	{
		PluginInterface = pluginInterface;
	}

	public void Save()
	{
		PluginInterface.SavePluginConfig((IPluginConfiguration)(object)this);
	}
}
