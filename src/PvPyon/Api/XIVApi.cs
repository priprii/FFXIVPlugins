using System;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using PvPyon.Api.Nameplates.Model;

namespace PvPyon.Api;

public class XIVApi
{
	private static nint _RaptureAtkModulePtr = IntPtr.Zero;

	public unsafe static nint RaptureAtkModulePtr
	{
		get
		{
			if (_RaptureAtkModulePtr == IntPtr.Zero)
			{
				UIModule* uiModule = ((Framework)Framework.Instance()).GetUiModule();
				_RaptureAtkModulePtr = new IntPtr(((UIModule)uiModule).GetRaptureAtkModule());
			}
			return _RaptureAtkModulePtr;
		}
	}

	public static SafeAddonNameplate GetSafeAddonNamePlate()
	{
		return new SafeAddonNameplate(PluginServices.PluginInterface);
	}
}
