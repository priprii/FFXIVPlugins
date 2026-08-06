using System.Collections.Generic;
using System.Collections.Immutable;

namespace Ktisis.Data.Config.Sections;

public class PresetConfig
{
	internal delegate void PresetRemoved(string presetName);

	internal static PresetRemoved? PresetRemovedEvent;

	public SortedDictionary<string, ImmutableHashSet<string>> Presets = new SortedDictionary<string, ImmutableHashSet<string>>();

	public HashSet<string> DefaultPresets = new HashSet<string>();

	public bool PresetIsDefault(string name)
	{
		return DefaultPresets.Contains(name);
	}
}
