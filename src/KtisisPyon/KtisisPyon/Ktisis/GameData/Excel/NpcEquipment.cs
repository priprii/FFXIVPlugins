using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Ktisis.Common.Extensions;
using Ktisis.Structs.Characters;
using Lumina.Excel;

namespace Ktisis.GameData.Excel;

[Sheet("NpcEquip", 2125379932u)]
public struct NpcEquipment(ExcelPage page, uint offset, uint row) : IExcelRow<NpcEquipment>
{
	public ExcelPage ExcelPage => page;

	public uint RowOffset { get; } = offset;

	public uint RowId { get; } = row;

	public WeaponModelId MainHand { get; private set; } = default(WeaponModelId);

	public WeaponModelId OffHand { get; private set; } = default(WeaponModelId);

	public EquipmentContainer Equipment { get; set; } = default(EquipmentContainer);

	static NpcEquipment IExcelRow<NpcEquipment>.Create(ExcelPage page, uint offset, uint row)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		NpcEquipment result = new NpcEquipment(page, offset, row);
		result.MainHand = page.ReadWeapon(0, offset);
		result.OffHand = page.ReadWeapon(3, offset);
		result.Equipment = page.ReadEquipment(6, offset);
		return result;
	}
}
