using Ktisis.Common.Extensions;
using Lumina.Excel;

namespace Ktisis.GameData.Excel;

[Sheet("Item", 3919789213u)]
public struct ItemSheet : IExcelRow<ItemSheet>
{
	[Sheet("EquipSlotCategory")]
	private struct EquipSlotCategoryRow(ExcelPage page, uint offset, uint row) : IExcelRow<EquipSlotCategoryRow>
	{
		public ExcelPage ExcelPage => page;

		public uint RowOffset { get; } = offset;

		public uint RowId { get; } = row;

		private bool[] Slots { get; set; } = new bool[14];

		public bool IsEquippable(EquipSlot slot)
		{
			return slot switch
			{
				EquipSlot.MainHand => Slots[1], 
				EquipSlot.OffHand => Slots[0], 
				_ => Slots[(int)slot], 
			};
		}

		static EquipSlotCategoryRow IExcelRow<EquipSlotCategoryRow>.Create(ExcelPage page, uint offset, uint row)
		{
			bool[] array = new bool[14];
			for (int i = 0; i < 14; i++)
			{
				array[i] = page.ReadColumn<sbyte>(i, offset) != 0;
			}
			EquipSlotCategoryRow result = new EquipSlotCategoryRow(page, offset, row);
			result.Slots = array;
			return result;
		}
	}

	public uint RowOffset { get; }

	public ExcelPage ExcelPage { get; }

	public uint RowId { get; }

	public string Name { get; }

	public ushort Icon { get; }

	public ItemModel Model { get; }

	public ItemModel SubModel { get; }

	private RowRef<EquipSlotCategoryRow> EquipSlotCategory { get; }

	public bool IsEquippable()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (EquipSlotCategory.IsValid)
		{
			return EquipSlotCategory.RowId != 0;
		}
		return false;
	}

	public bool IsEquippable(EquipSlot slot)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		bool flag = IsEquippable() && EquipSlotCategory.Value.IsEquippable(slot);
		if (slot == EquipSlot.MainHand)
		{
			flag |= EquipSlotCategory.Value.IsEquippable(EquipSlot.OffHand);
		}
		return flag;
	}

	public bool IsWeapon()
	{
		if (!IsEquippable(EquipSlot.MainHand))
		{
			return IsEquippable(EquipSlot.OffHand);
		}
		return true;
	}

	public ItemSheet(ExcelPage page, uint offset, uint row)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Model = null;
		SubModel = null;
		RowId = row;
		RowOffset = offset;
		ExcelPage = page;
		Name = page.ReadColumn<string>(9, offset);
		Icon = page.ReadColumn<ushort>(10, offset);
		EquipSlotCategory = page.ReadRowRef<EquipSlotCategoryRow>(17, offset);
		bool isWep = IsWeapon();
		Model = new ItemModel(page.ReadColumn<ulong>(47, offset), isWep);
		SubModel = new ItemModel(page.ReadColumn<ulong>(48, offset), isWep);
	}

	static ItemSheet IExcelRow<ItemSheet>.Create(ExcelPage page, uint offset, uint row)
	{
		return new ItemSheet(page, offset, row);
	}
}
