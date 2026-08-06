using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Keys;

namespace Ktisis.Actions.Binds;

public class KeyCombo
{
	public VirtualKey Key;

	public VirtualKey[] Modifiers;

	public KeyCombo(VirtualKey key = (VirtualKey)0, params VirtualKey[] mods)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Key = key;
		Modifiers = mods;
	}

	public string GetShortcutString()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		IEnumerable<string> values = from key in Modifiers.Append(Key)
			select VirtualKeyExtensions.GetFancyName(key);
		return string.Join(" + ", values);
	}

	public void AddModifier(VirtualKey key)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Modifiers = Modifiers.Append(key).ToArray();
	}
}
