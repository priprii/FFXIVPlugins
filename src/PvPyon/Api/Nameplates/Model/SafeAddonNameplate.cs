using System;
using System.Runtime.InteropServices;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace PvPyon.Api.Nameplates.Model;

public class SafeAddonNameplate
{
	private readonly DalamudPluginInterface Interface;

	public nint Pointer => PluginServices.GameGui.GetAddonByName("NamePlate", 1);

	public SafeAddonNameplate(DalamudPluginInterface pluginInterface)
	{
		Interface = pluginInterface;
	}

	public SafeNameplateObject GetNamePlateObject(int index)
	{
		SafeNameplateObject result = null;
		if (Pointer != IntPtr.Zero)
		{
			nint num = Marshal.ReadIntPtr(Pointer + ((IntPtr)Marshal.OffsetOf(typeof(AddonNamePlate), "NamePlateObjectArray")).ToInt32());
			if (num != IntPtr.Zero)
			{
				result = new SafeNameplateObject(num + Marshal.SizeOf(typeof(NamePlateObject)) * index, index);
			}
		}
		return result;
	}
}
