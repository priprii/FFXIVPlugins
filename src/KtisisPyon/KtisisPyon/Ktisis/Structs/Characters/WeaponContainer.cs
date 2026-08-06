using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace Ktisis.Structs.Characters;

[StructLayout(LayoutKind.Explicit, Size = 12)]
public struct WeaponContainer
{
	public const int Length = 3;

	public const int Size = 12;

	[FieldOffset(0)]
	public unsafe fixed byte Bytes[12];

	[FieldOffset(0)]
	public WeaponModelId MainHand;

	[FieldOffset(4)]
	public WeaponModelId OffHand;

	[FieldOffset(8)]
	public WeaponModelId Prop;

	public WeaponModelId this[uint index]
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

	private unsafe WeaponModelId Get(uint index)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return *GetData(index);
	}

	private unsafe void Set(uint index, WeaponModelId equip)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Unsafe.Write(GetData(index), equip);
	}

	public unsafe WeaponModelId* GetData(uint index)
	{
		if (index >= 3)
		{
			throw new IndexOutOfRangeException($"Index {index} is out of range (< {3}).");
		}
		fixed (byte* bytes = Bytes)
		{
			return (WeaponModelId*)(bytes + index * Unsafe.SizeOf<WeaponModelId>());
		}
	}
}
