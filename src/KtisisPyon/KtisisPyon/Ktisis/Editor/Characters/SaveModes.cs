using System;

namespace Ktisis.Editor.Characters;

[Flags]
public enum SaveModes
{
	None = 0,
	EquipmentGear = 1,
	EquipmentAccessories = 2,
	EquipmentWeapons = 4,
	AppearanceHair = 8,
	AppearanceFace = 0x10,
	AppearanceBody = 0x20,
	AppearanceExtended = 0x40,
	Equipment = 3,
	Appearance = 0x38,
	All = 0x3F
}
