using System;

namespace Ktisis.Structs.Common;

public struct ObjectUnion
{
	public unsafe nint** __vfTable;

	public nint Data;

	public nint GetObjectPointer()
	{
		if ((Data & 1) == 0)
		{
			return IntPtr.Zero;
		}
		return Data & -8;
	}

	public short GetObjectIndex()
	{
		if ((Data & 4) == 0)
		{
			return -1;
		}
		return (short)(Data >> 3);
	}
}
