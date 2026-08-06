using Dalamud.Game.ClientState.Keys;
using Ktisis.Actions.Binds;

namespace Ktisis.Data.Config.Actions;

public class ActionKeybind
{
	public bool Enabled = true;

	public KeyCombo Combo = new KeyCombo((VirtualKey)0);
}
