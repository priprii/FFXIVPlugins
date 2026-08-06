using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dalamud;
using Hypostasis.Dalamud;

namespace Hypostasis.Game;

public sealed class AsmPatch : IDisposable
{
	private static readonly List<AsmPatch> asmPatches = new List<AsmPatch>();

	public nint Address { get; }

	public string Signature { get; } = string.Empty;

	public byte[] NewBytes { get; }

	public byte[] OldBytes { get; }

	public bool IsEnabled { get; private set; }

	public bool IsValid => Address != IntPtr.Zero;

	public string ReadBytes
	{
		get
		{
			if (IsValid)
			{
				return OldBytes.Aggregate(string.Empty, (string current, byte b) => current + b.ToString("X2") + " ");
			}
			return string.Empty;
		}
	}

	public AsmPatch(nint address, IReadOnlyCollection<byte?> bytes, bool startEnabled = false)
	{
		if (address != IntPtr.Zero)
		{
			byte?[] trimmedBytes = bytes.SkipWhile((byte? b) => !b.HasValue).ToArray();
			int num = bytes.Count - trimmedBytes.Length;
			address += num;
			Address = address;
			byte[] oldBytes = default(byte[]);
			SafeMemory.ReadBytes((IntPtr)address, trimmedBytes.Length, ref oldBytes);
			OldBytes = oldBytes;
			NewBytes = (from i in Enumerable.Range(0, trimmedBytes.Length)
				select trimmedBytes[i] ?? oldBytes[i]).ToArray();
			asmPatches.Add(this);
			if (startEnabled)
			{
				Enable();
			}
		}
	}

	public AsmPatch(nint address, string bytesString, bool startEnabled = false)
		: this(address, ParseByteString(bytesString), startEnabled)
	{
	}

	public AsmPatch(string sig, IReadOnlyCollection<byte?> bytes, bool startEnabled = false)
		: this(Scan(sig), bytes, startEnabled)
	{
		Signature = sig;
	}

	public AsmPatch(string sig, string bytesString, bool startEnabled = false)
		: this(sig, ParseByteString(bytesString), startEnabled)
	{
	}

	private static nint Scan(string sig)
	{
		try
		{
			return DalamudApi.SigScanner.DalamudSigScanner.ScanModule(sig);
		}
		catch (Exception exception)
		{
			DalamudApi.LogWarning("Failed to find signature " + sig, exception);
			return IntPtr.Zero;
		}
	}

	private static byte?[] ParseByteString(string bytesString)
	{
		bytesString = bytesString.Replace(" ", string.Empty);
		byte?[] array = new byte?[bytesString.Length / 2];
		for (int i = 0; i < bytesString.Length; i += 2)
		{
			string text = bytesString.Substring(i, 2);
			byte?[] array2 = array;
			int num = i / 2;
			bool flag = ((text == "??" || text == "**") ? true : false);
			array2[num] = ((!flag) ? new byte?(byte.Parse(text, NumberStyles.AllowHexSpecifier)) : ((byte?)null));
		}
		return array;
	}

	public void Enable()
	{
		if (!IsEnabled && IsValid)
		{
			SafeMemory.WriteBytes((IntPtr)Address, NewBytes);
			IsEnabled = true;
		}
	}

	public void Disable()
	{
		if (IsEnabled && IsValid)
		{
			SafeMemory.WriteBytes((IntPtr)Address, OldBytes);
			IsEnabled = false;
		}
	}

	public void Toggle()
	{
		if (!IsEnabled)
		{
			Enable();
		}
		else
		{
			Disable();
		}
	}

	public void Toggle(bool enable)
	{
		if (enable)
		{
			Enable();
		}
		else
		{
			Disable();
		}
	}

	public void Dispose()
	{
		if (IsEnabled)
		{
			Disable();
		}
	}

	public static void DisposeAll()
	{
		foreach (AsmPatch asmPatch in asmPatches)
		{
			asmPatch?.Dispose();
		}
	}
}
