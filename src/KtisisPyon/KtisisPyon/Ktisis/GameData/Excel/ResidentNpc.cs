using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Ktisis.Common.Extensions;
using Ktisis.GameData.Excel.Types;
using Ktisis.Structs.Characters;
using Lumina.Data;
using Lumina.Excel;

namespace Ktisis.GameData.Excel;

[Sheet("ENpcResident", 4149192844u)]
public struct ResidentNpc(ExcelPage page, uint offset, uint row) : IExcelRow<ResidentNpc>, INpcBase
{
	public ExcelPage ExcelPage => page;

	public uint RowOffset { get; } = offset;

	public uint RowId { get; } = row;

	public byte Map { get; init; } = 0;

	private RowRef<EventNpc> EventNpc { get; init; } = default(RowRef<EventNpc>);

	public string Name { get; set; } = string.Empty;

	public uint HashId { get; set; } = 0u;

	static ResidentNpc IExcelRow<ResidentNpc>.Create(ExcelPage page, uint offset, uint row)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		string name = page.ReadColumn<string>(0, offset);
		sbyte article = page.ReadColumn<sbyte>(7, offset);
		return new ResidentNpc(page, offset, row)
		{
			Name = (name.FormatName(article) ?? $"E:{row:D7}"),
			Map = page.ReadColumn<byte>(9, offset),
			EventNpc = new RowRef<EventNpc>(page.Module, row, (Language?)page.Language)
		};
	}

	public ushort GetModelId()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (!EventNpc.IsValid)
		{
			return ushort.MaxValue;
		}
		return EventNpc.Value.GetModelId();
	}

	public CustomizeContainer? GetCustomize()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!EventNpc.IsValid)
		{
			return null;
		}
		return EventNpc.Value.GetCustomize();
	}

	public EquipmentContainer? GetEquipment()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!EventNpc.IsValid)
		{
			return null;
		}
		return EventNpc.Value.GetEquipment();
	}

	public WeaponModelId? GetMainHand()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!EventNpc.IsValid)
		{
			return null;
		}
		return EventNpc.Value.GetMainHand();
	}

	public WeaponModelId? GetOffHand()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!EventNpc.IsValid)
		{
			return null;
		}
		return EventNpc.Value.GetOffHand();
	}
}
