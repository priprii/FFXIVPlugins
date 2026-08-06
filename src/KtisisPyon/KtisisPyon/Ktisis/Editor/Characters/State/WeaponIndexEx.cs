using System;
using Ktisis.GameData.Excel;

namespace Ktisis.Editor.Characters.State;

public static class WeaponIndexEx
{
	public static EquipSlot ToEquipSlot(this WeaponIndex index)
	{
		switch (index)
		{
		case WeaponIndex.MainHand:
		case WeaponIndex.OffHand:
			return (EquipSlot)index;
		case WeaponIndex.Prop:
			return EquipSlot.OffHand;
		default:
			throw new Exception($"Cannot convert invalid weapon index ({index})");
		}
	}
}
