using System;
using FFXIVClientStructs.FFXIV.Client.System.Memory;

namespace Ktisis.Interop;

public class Alloc<T> : IDisposable where T : unmanaged
{
	public nint Address { get; private set; }

	public unsafe T* Data => (T*)Address;

	public bool IsDisposed { get; private set; }

	public unsafe Alloc(ulong align = 8uL)
	{
		Address = (nint)((IMemorySpace)IMemorySpace.GetDefaultSpace()).Malloc<T>(align);
	}

	public unsafe void Dispose()
	{
		if (!IsDisposed)
		{
			if (Address != IntPtr.Zero)
			{
				IMemorySpace.Free<T>(Data);
				Address = IntPtr.Zero;
			}
			IsDisposed = true;
			GC.SuppressFinalize(this);
		}
	}

	~Alloc()
	{
		Dispose();
	}
}
