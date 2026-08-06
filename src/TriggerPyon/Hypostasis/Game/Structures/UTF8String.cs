using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Hypostasis.Game.Structures;

[StructLayout(LayoutKind.Sequential, Size = 104)]
public readonly struct UTF8String : IDisposable
{
	public const int size = 104;

	public readonly nint stringPtr;

	public readonly ulong capacity;

	public readonly ulong length;

	public readonly ulong unknown;

	public readonly byte isEmpty;

	public readonly byte notReallocated;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
	public readonly byte[] str;

	public UTF8String(nint loc, string text)
		: this(loc, Encoding.UTF8.GetBytes(text))
	{
	}

	public UTF8String(nint loc, byte[] text)
	{
		capacity = 64uL;
		length = (ulong)text.Length + 1uL;
		str = new byte[capacity];
		if (length > capacity)
		{
			stringPtr = Marshal.AllocHGlobal(text.Length + 1);
			capacity = length;
			Marshal.Copy(text, 0, stringPtr, text.Length);
			Marshal.WriteByte(stringPtr, text.Length, 0);
			notReallocated = 0;
		}
		else
		{
			stringPtr = loc + 34;
			text.CopyTo(str, 0);
			notReallocated = 1;
		}
		isEmpty = ((length == 1) ? ((byte)1) : ((byte)0));
		unknown = 0uL;
	}

	public void Dispose()
	{
		if (notReallocated == 0)
		{
			Marshal.FreeHGlobal(stringPtr);
		}
	}
}
