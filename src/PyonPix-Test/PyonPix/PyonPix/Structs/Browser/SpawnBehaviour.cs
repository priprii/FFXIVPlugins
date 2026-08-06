using System;

namespace PyonPix.Structs.Browser;

[Flags]
public enum SpawnBehaviour
{
	None = 0,
	Show = 2,
	Expand = 4,
	Unmute = 8,
	Navigate = 0x10
}
