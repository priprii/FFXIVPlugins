using System;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Ktisis.Structs.Characters;
using Lumina.Data;
using Lumina.Data.Parsing;
using Lumina.Data.Structs.Excel;
using Lumina.Excel;
using Lumina.Text.ReadOnly;

namespace Ktisis.Common.Extensions;

public static class LuminaEx
{
	public static RowRef<T> ReadRowRef<T>(this ExcelPage page, int columnIndex, uint offset) where T : struct, IExcelRow<T>
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		object andReadColumn = GetAndReadColumn(page, columnIndex, offset);
		return new RowRef<T>(page.Module, Convert.ToUInt32(andReadColumn), (Language?)page.Language);
	}

	public static T ReadColumn<T>(this ExcelPage page, int columnIndex, uint offset)
	{
		return (T)GetAndReadColumn(page, columnIndex, offset);
	}

	private static object GetAndReadColumn(ExcelPage page, int columnIndex, uint offset)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected I4, but got Unknown
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		ExcelColumnDefinition val = page.Sheet.Columns[columnIndex];
		ExcelColumnDataType type = val.Type;
		switch ((int)type)
		{
		case 0:
		{
			ReadOnlySeString val2 = page.ReadString((UIntPtr)(val.Offset + offset), (UIntPtr)offset);
			return ((ReadOnlySeString)(ref val2)).ExtractText();
		}
		case 1:
			return page.ReadBool((UIntPtr)(val.Offset + offset));
		case 2:
			return page.ReadInt8((UIntPtr)(val.Offset + offset));
		case 3:
			return page.ReadUInt8((UIntPtr)(val.Offset + offset));
		case 4:
			return page.ReadInt16((UIntPtr)(val.Offset + offset));
		case 5:
			return page.ReadUInt16((UIntPtr)(val.Offset + offset));
		case 6:
			return page.ReadInt32((UIntPtr)(val.Offset + offset));
		case 7:
			return page.ReadUInt32((UIntPtr)(val.Offset + offset));
		case 9:
			return page.ReadFloat32((UIntPtr)(val.Offset + offset));
		case 10:
			return page.ReadInt64((UIntPtr)(val.Offset + offset));
		case 11:
			return page.ReadUInt64((UIntPtr)(val.Offset + offset));
		case 25:
			return page.ReadPackedBool((UIntPtr)(val.Offset + offset), (byte)0);
		case 26:
			return page.ReadPackedBool((UIntPtr)(val.Offset + offset), (byte)1);
		case 27:
			return page.ReadPackedBool((UIntPtr)(val.Offset + offset), (byte)2);
		case 28:
			return page.ReadPackedBool((UIntPtr)(val.Offset + offset), (byte)3);
		case 29:
			return page.ReadPackedBool((UIntPtr)(val.Offset + offset), (byte)4);
		case 30:
			return page.ReadPackedBool((UIntPtr)(val.Offset + offset), (byte)5);
		case 31:
			return page.ReadPackedBool((UIntPtr)(val.Offset + offset), (byte)6);
		case 32:
			return page.ReadPackedBool((UIntPtr)(val.Offset + offset), (byte)7);
		default:
			throw new Exception($"Unknown type: {val.Type}");
		}
	}

	public static CustomizeContainer ReadCustomize(this ExcelPage parser, int index, uint offset)
	{
		CustomizeContainer result = default(CustomizeContainer);
		for (int i = 0; i < 26; i++)
		{
			result[(uint)i] = parser.ReadColumn<byte>(index + i, offset);
		}
		return result;
	}

	public static WeaponModelId ReadWeapon(this ExcelPage parser, int index, uint offset)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		Quad val = (Quad)parser.ReadColumn<ulong>(index, offset);
		byte stain = parser.ReadColumn<byte>(index + 1, offset);
		byte stain2 = parser.ReadColumn<byte>(index + 2, offset);
		return new WeaponModelId
		{
			Id = ((Quad)(ref val)).A,
			Type = ((Quad)(ref val)).B,
			Variant = ((Quad)(ref val)).C,
			Stain0 = stain,
			Stain1 = stain2
		};
	}

	public static EquipmentModelId ReadEquipItem(this ExcelPage parser, int index, uint offset)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		uint num = parser.ReadColumn<uint>(index, offset);
		byte stain = parser.ReadColumn<byte>(index + 1, offset);
		byte stain2 = parser.ReadColumn<byte>(index + 2, offset);
		return new EquipmentModelId
		{
			Id = (ushort)num,
			Variant = (byte)(num >> 16),
			Stain0 = stain,
			Stain1 = stain2
		};
	}

	public static EquipmentContainer ReadEquipment(this ExcelPage parser, int index, uint offset)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		EquipmentContainer result = default(EquipmentContainer);
		for (int i = 0; i < 10; i++)
		{
			result[(uint)i] = parser.ReadEquipItem(index + i * 3 + ((i > 0) ? 2 : 0), offset);
		}
		return result;
	}
}
