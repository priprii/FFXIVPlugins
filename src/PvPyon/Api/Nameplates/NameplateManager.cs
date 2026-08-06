using System;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using PvPyon.Api.Nameplates.Model;

namespace PvPyon.Api.Nameplates;

public class NameplateManager : IDisposable
{
	public NameplateHooks Hooks { get; init; } = new NameplateHooks();

	public bool IsValid => Hooks.IsValid;

	public NameplateManager()
	{
		Hooks.Initialize();
	}

	~NameplateManager()
	{
		Dispose();
	}

	public void Dispose()
	{
		Hooks?.Dispose();
	}

	public static T? GetNameplateGameObject<T>(SafeNameplateObject namePlateObject) where T : GameObject
	{
		return GetNameplateGameObject<T>(namePlateObject.Pointer);
	}

	public unsafe static T? GetNameplateGameObject<T>(nint nameplateObjectPtr) where T : GameObject
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		nint num = Marshal.ReadIntPtr((nint)PluginServices.GameGui.GetAddonByName("NamePlate", 1) + ((IntPtr)Marshal.OffsetOf(typeof(AddonNamePlate), "NamePlateObjectArray")).ToInt32());
		if (num == IntPtr.Zero)
		{
			return default(T);
		}
		int num2 = Marshal.SizeOf(typeof(NamePlateObject));
		long num3 = (((IntPtr)nameplateObjectPtr).ToInt64() - ((IntPtr)(num + 0)).ToInt64()) / num2;
		if (num3 < 0 || num3 >= AddonNamePlate.NumNamePlateObjects)
		{
			return default(T);
		}
		nint zero = IntPtr.Zero;
		Framework* ptr = Framework.Instance();
		uint objectID = Marshal.PtrToStructure<NamePlateInfo>(new IntPtr(((IntPtr)new IntPtr(&((RaptureAtkModule)((UIModule)((Framework)ptr).GetUiModule()).GetRaptureAtkModule()).NamePlateInfoArray)).ToInt64() + Marshal.SizeOf(typeof(NamePlateInfo)) * num3)).ObjectID.ObjectID;
		GameObject obj = PluginServices.ObjectTable.SearchById((ulong)objectID);
		return (T)(object)((obj is T) ? obj : null);
	}
}
