using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Ktisis.Common.Extensions;
using Ktisis.GameData.Excel.Types;
using Ktisis.Structs.Characters;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace Ktisis.GameData.Excel;

[Sheet("ENpcBase", 1537860006u)]
public struct EventNpc(ExcelPage page, uint offset, uint row) : IExcelRow<EventNpc>, INpcBase
{
	public ExcelPage ExcelPage => page;

	public uint RowOffset { get; } = offset;

	public uint RowId { get; } = row;

	public RowRef<ModelChara> ModelChara { get; init; } = default(RowRef<ModelChara>);

	public CustomizeContainer Customize { get; init; } = default(CustomizeContainer);

	public WeaponModelId MainHand { get; init; } = default(WeaponModelId);

	public WeaponModelId OffHand { get; init; } = default(WeaponModelId);

	public EquipmentContainer Equipment { get; init; } = default(EquipmentContainer);

	public string Name { get; set; } = string.Empty;

	static EventNpc IExcelRow<EventNpc>.Create(ExcelPage page, uint offset, uint row)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		return new EventNpc(page, offset, row)
		{
			Name = $"E:{row:D7}",
			ModelChara = page.ReadRowRef<ModelChara>(35, offset),
			Customize = page.ReadCustomize(36, offset),
			MainHand = page.ReadWeapon(65, offset),
			OffHand = page.ReadWeapon(68, offset),
			Equipment = ReadEquipment(page, offset)
		};
	}

	private unsafe static EquipmentContainer ReadEquipment(ExcelPage page, uint offset)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		ushort num = page.ReadColumn<ushort>(63, offset);
		EquipmentContainer result = page.ReadEquipment(71, offset);
		if ((num == 0 || num == 175) ? true : false)
		{
			return result;
		}
		RowRef<NpcEquipment> val = default(RowRef<NpcEquipment>);
		val._002Ector(page.Module, (uint)num, (Language?)page.Language);
		if (!val.IsValid)
		{
			return result;
		}
		for (uint num2 = 0u; num2 < 10; num2++)
		{
			EquipmentModelId value = val.Value.Equipment[num2];
			if (!((object)(*(EquipmentModelId*)(&value))/*cast due to constrained. prefix*/).Equals((object?)null))
			{
				result[num2] = value;
			}
		}
		return result;
	}

	public ushort GetModelId()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return (ushort)ModelChara.RowId;
	}

	public CustomizeContainer? GetCustomize()
	{
		return Customize;
	}

	public EquipmentContainer GetEquipment()
	{
		return Equipment;
	}

	public WeaponModelId? GetMainHand()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return MainHand;
	}

	public WeaponModelId? GetOffHand()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return OffHand;
	}
}
