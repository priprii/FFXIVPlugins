using System;

namespace Ktisis.Actions.Binds;

[Flags]
public enum KeybindTrigger
{
	None = 0,
	OnDown = 1,
	OnHeld = 2,
	OnRelease = 4
}
