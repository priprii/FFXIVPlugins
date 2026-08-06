using System;
using Hypostasis.Dalamud;

namespace Hypostasis.Game;

public sealed class VirtualFunction<T> : GameFunction<T> where T : Delegate
{
	public unsafe nint* VTable { get; }

	public int VFuncIndex { get; }

	public unsafe VirtualFunction(nint* vtbl, int i, string sig = null)
	{
		VTable = vtbl;
		VFuncIndex = i;
		base.Signature = sig;
		SetupAddress(required: true);
	}

	protected unsafe override nint ScanAddress()
	{
		if (VTable == null)
		{
			return IntPtr.Zero;
		}
		if (!string.IsNullOrEmpty(base.Signature))
		{
			return DalamudApi.SigScanner.Scan(VTable[VFuncIndex], base.Signature.Length / 2, base.Signature);
		}
		return VTable[VFuncIndex];
	}
}
