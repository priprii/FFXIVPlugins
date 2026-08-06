using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace Ktisis.Structs.Characters;

[StructLayout(LayoutKind.Explicit, Size = 80)]
public struct EquipmentContainer
{
	public const int Length = 10;

	public const int Size = 80;

	[FieldOffset(0)]
	public unsafe fixed byte Bytes[80];

	[FieldOffset(0)]
	public EquipmentModelId Head;

	[FieldOffset(8)]
	public EquipmentModelId Chest;

	[FieldOffset(16)]
	public EquipmentModelId Hands;

	[FieldOffset(24)]
	public EquipmentModelId Legs;

	[FieldOffset(32)]
	public EquipmentModelId Feet;

	[FieldOffset(40)]
	public EquipmentModelId Earring;

	[FieldOffset(48)]
	public EquipmentModelId Necklace;

	[FieldOffset(56)]
	public EquipmentModelId Bracelet;

	[FieldOffset(64)]
	public EquipmentModelId RingRight;

	[FieldOffset(72)]
	public EquipmentModelId RingLeft;

	public EquipmentModelId this[uint index]
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return Get(index);
		}
		set
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			Set(index, value);
		}
	}

	private unsafe EquipmentModelId Get(uint index)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return *GetData(index);
	}

	private unsafe void Set(uint index, EquipmentModelId equip)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Unsafe.Write(GetData(index), equip);
	}

	public unsafe EquipmentModelId* GetData(uint index)
	{
		if (index >= 10)
		{
			throw new IndexOutOfRangeException($"Index {index} is out of range (< {10}).");
		}
		fixed (byte* bytes = Bytes)
		{
			return (EquipmentModelId*)(bytes + index * Unsafe.SizeOf<EquipmentModelId>());
		}
	}
}
