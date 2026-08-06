using System;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace PvPyon.Api.Nameplates.Model;

public class SafeNameplateInfo
{
	public readonly nint Pointer;

	public readonly NamePlateInfo Data;

	internal nint NameAddress => GetStringPtr("Name");

	internal nint FcNameAddress => GetStringPtr("FcName");

	internal nint TitleAddress => GetStringPtr("Title");

	internal nint DisplayTitleAddress => GetStringPtr("DisplayTitle");

	internal nint LevelTextAddress => GetStringPtr("LevelText");

	public string Name => GetString(NameAddress);

	public string FcName => GetString(FcNameAddress);

	public string Title => GetString(TitleAddress);

	public string DisplayTitle => GetString(DisplayTitleAddress);

	public string LevelText => GetString(LevelTextAddress);

	public SafeNameplateInfo(nint pointer)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Pointer = pointer;
		Data = Marshal.PtrToStructure<NamePlateInfo>(Pointer);
	}

	private nint GetStringPtr(string name)
	{
		return Marshal.ReadIntPtr(Pointer + ((IntPtr)Marshal.OffsetOf(typeof(NamePlateInfo), name)).ToInt32() + ((IntPtr)Marshal.OffsetOf(typeof(Utf8String), "StringPtr")).ToInt32());
	}

	private string GetString(nint stringPtr)
	{
		return Marshal.PtrToStringUTF8(stringPtr);
	}
}
