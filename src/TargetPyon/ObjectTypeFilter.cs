using System;

namespace TargetPyon;

[Flags]
public enum ObjectTypeFilter
{
	None = 0,
	Aetheryte = 2,
	Area = 4,
	BattleNpc = 8,
	CardStand = 0x10,
	Cutscene = 0x20,
	Companion = 0x40,
	EventNpc = 0x80,
	EventObj = 0x100,
	GatheringPoint = 0x200,
	Housing = 0x400,
	Mount = 0x800,
	Ornament = 0x1000,
	Retainer = 0x2000,
	Treasure = 0x4000,
	BgObject = 0x8000,
	CharacterBase = 0x10000,
	EnvLocation = 0x20000,
	EnvSpace = 0x40000,
	Light = 0x80000,
	Object = 0x100000,
	VfxObject = 0x200000,
	Unknown = 0x400000
}
