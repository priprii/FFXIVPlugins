using System;

namespace PyonPix.Structs.Browser;

[Flags]
public enum DespawnBehaviour
{
	None = 0,
	Hide = 2,
	Collapse = 4,
	Mute = 8,
	Shutdown = 0x10
}
