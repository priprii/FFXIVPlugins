using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Ktisis.Common.Extensions;
using Ktisis.GameData.Excel.Types;
using Ktisis.Structs.Characters;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace Ktisis.GameData.Excel;

[Sheet("BNpcBase", 2904699651u)]
public struct BattleNpc(ExcelPage page, uint offset, uint row) : IExcelRow<BattleNpc>, INpcBase
{
	[Sheet("BNpcCustomize", 418406612u)]
	private struct BNpcCustomize(ExcelPage page, uint offset, uint row) : IExcelRow<BNpcCustomize>
	{
		public ExcelPage ExcelPage => page;

		public uint RowOffset { get; } = offset;

		public uint RowId { get; } = row;

		public CustomizeContainer Customize { get; private init; } = default(CustomizeContainer);

		static BNpcCustomize IExcelRow<BNpcCustomize>.Create(ExcelPage page, uint offset, uint row)
		{
			return new BNpcCustomize(page, offset, row)
			{
				Customize = page.ReadCustomize(0, offset)
			};
		}
	}

	public ExcelPage ExcelPage => page;

	public uint RowOffset { get; } = offset;

	public uint RowId { get; } = row;

	public float Scale { get; init; } = 0f;

	private RowRef<ModelChara> ModelChara { get; init; } = default(RowRef<ModelChara>);

	private RowRef<BNpcCustomize> Customize { get; init; } = default(RowRef<BNpcCustomize>);

	private RowRef<NpcEquipment> Equipment { get; init; } = default(RowRef<NpcEquipment>);

	public string Name { get; set; } = string.Empty;

	static BattleNpc IExcelRow<BattleNpc>.Create(ExcelPage page, uint offset, uint row)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		return new BattleNpc(page, offset, row)
		{
			Scale = page.ReadColumn<float>(4, offset),
			ModelChara = page.ReadRowRef<ModelChara>(5, offset),
			Customize = page.ReadRowRef<BNpcCustomize>(6, offset),
			Equipment = page.ReadRowRef<NpcEquipment>(7, offset)
		};
	}

	public ushort GetModelId()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return (ushort)ModelChara.RowId;
	}

	public CustomizeContainer? GetCustomize()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		RowRef<BNpcCustomize> customize = Customize;
		if (!customize.IsValid || customize.RowId == 0)
		{
			return null;
		}
		return Customize.Value.Customize;
	}

	public EquipmentContainer? GetEquipment()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!Equipment.IsValid)
		{
			return null;
		}
		return Equipment.Value.Equipment;
	}

	public WeaponModelId? GetMainHand()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!Equipment.IsValid)
		{
			return null;
		}
		return Equipment.Value.MainHand;
	}

	public WeaponModelId? GetOffHand()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!Equipment.IsValid)
		{
			return null;
		}
		return Equipment.Value.OffHand;
	}
}
