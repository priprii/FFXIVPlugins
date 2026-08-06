using System;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.System.Memory;

namespace PvPyon;

public static class GameInterfaceHelper
{
	public static SeString ReadSeString(nint ptr)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		if (ptr == IntPtr.Zero)
		{
			return new SeString();
		}
		if (!TryReadStringBytes(ptr, out byte[] bytes) || bytes == null)
		{
			return new SeString();
		}
		return SeString.Parse(bytes);
	}

	public static bool TryReadSeString(nint ptr, out SeString? seString)
	{
		seString = null;
		if (ptr == IntPtr.Zero)
		{
			return false;
		}
		if (TryReadStringBytes(ptr, out byte[] bytes) && bytes != null)
		{
			seString = SeString.Parse(bytes);
			return true;
		}
		return false;
	}

	public static string? ReadString(nint ptr)
	{
		if (ptr == IntPtr.Zero)
		{
			return null;
		}
		if (TryReadStringBytes(ptr, out byte[] bytes) && bytes != null)
		{
			return Encoding.UTF8.GetString(bytes);
		}
		return null;
	}

	public static bool TryReadString(nint ptr, out string? str)
	{
		str = null;
		if (ptr == IntPtr.Zero)
		{
			return false;
		}
		if (TryReadStringBytes(ptr, out byte[] bytes) && bytes != null)
		{
			str = Encoding.UTF8.GetString(bytes);
			return true;
		}
		return false;
	}

	public static bool TryReadStringBytes(nint ptr, out byte[]? bytes)
	{
		bytes = null;
		if (ptr == IntPtr.Zero)
		{
			return false;
		}
		int i;
		for (i = 0; Marshal.ReadByte(ptr, i) != 0; i++)
		{
		}
		bytes = new byte[i];
		Marshal.Copy(ptr, bytes, 0, i);
		return true;
	}

	public static nint PluginAllocate(byte[] bytes)
	{
		nint num = Marshal.AllocHGlobal(bytes.Length + 1);
		Marshal.Copy(bytes, 0, num, bytes.Length);
		Marshal.WriteByte(num, bytes.Length, 0);
		return num;
	}

	public static nint PluginAllocate(SeString seString)
	{
		return PluginAllocate(seString.Encode());
	}

	public static void PluginFree(ref nint ptr)
	{
		Marshal.FreeHGlobal(ptr);
		ptr = IntPtr.Zero;
	}

	public static byte[] NullTerminate(this byte[] bytes)
	{
		if (bytes.Length == 0 || bytes[^1] != 0)
		{
			byte[] array = new byte[bytes.Length + 1];
			Array.Copy(bytes, array, bytes.Length);
			array[^1] = 0;
			return array;
		}
		return bytes;
	}

	public unsafe static nint GameUIAllocate(ulong size)
	{
		return (nint)((IMemorySpace)IMemorySpace.GetUISpace()).Malloc(size, 0uL);
	}

	public unsafe static void GameFree(ref nint ptr, ulong size)
	{
		if (ptr != IntPtr.Zero)
		{
			IMemorySpace.Free((void*)ptr, size);
			ptr = IntPtr.Zero;
		}
	}
}
