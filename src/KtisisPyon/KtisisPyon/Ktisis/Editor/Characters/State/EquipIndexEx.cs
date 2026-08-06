using Ktisis.GameData.Excel;

namespace Ktisis.Editor.Characters.State;

public static class EquipIndexEx
{
	public static EquipSlot ToEquipSlot(this EquipIndex index)
	{
		return index switch
		{
			EquipIndex.RingLeft => EquipSlot.RingLeft, 
			EquipIndex.RingRight => EquipSlot.RingRight, 
			_ => (EquipSlot)((int)index + (((int)index > 2) ? 3 : 2)), 
		};
	}

	public static EquipIndex ToEquipIndex(this EquipSlot slot)
	{
		return slot switch
		{
			EquipSlot.RingLeft => EquipIndex.RingLeft, 
			EquipSlot.RingRight => EquipIndex.RingRight, 
			_ => (EquipIndex)(slot - ((slot >= EquipSlot.Waist) ? 3 : 2)), 
		};
	}
}
