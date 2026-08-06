using System;

namespace Ktisis.Structs.Actors;

[Flags]
public enum CombatFlags : byte
{
	None = 0,
	WeaponDrawn = 0x40
}
