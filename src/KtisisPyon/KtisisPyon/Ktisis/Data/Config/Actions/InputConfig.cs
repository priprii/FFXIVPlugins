using System.Collections.Generic;

namespace Ktisis.Data.Config.Actions;

public class InputConfig
{
	public bool BlockTargetLeftClick;

	public bool BlockTargetRightClick;

	public bool ScrollModifier;

	public bool ScrollAllow = true;

	public bool Enabled = true;

	public Dictionary<string, ActionKeybind> Keybinds = new Dictionary<string, ActionKeybind>();

	public ActionKeybind GetOrSetDefault(string name, ActionKeybind defaultValue)
	{
		if (Keybinds.TryGetValue(name, out ActionKeybind value))
		{
			return value;
		}
		Keybinds.Add(name, defaultValue);
		return defaultValue;
	}

	public void SetDefault(string name, ActionKeybind defaultValue)
	{
		Keybinds[name] = defaultValue;
	}
}
