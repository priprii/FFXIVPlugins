using System;

namespace Ktisis.Structs.Characters;

[Flags]
public enum FacialFeature : byte
{
	None = 0,
	First = 1,
	Second = 2,
	Third = 4,
	Fourth = 8,
	Fifth = 0x10,
	Sixth = 0x20,
	Seventh = 0x40,
	Legacy = 0x80
}
