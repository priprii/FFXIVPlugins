using System;
using System.Linq;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using PvPyon.Api.Nameplates.EventArgs;
using PvPyon.Api.Nameplates.Model;

namespace PvPyon.Api.Nameplates;

public class NameplateHooks : IDisposable
{
	public delegate void AddonNamePlate_SetPlayerNameEventHandler(AddonNamePlate_SetPlayerNameEventArgs eventArgs);

	public delegate void AddonNamePlate_SetPlayerNameManagedEventHandler(AddonNamePlate_SetPlayerNameManagedEventArgs eventArgs);

	private delegate nint AddonNamePlate_SetPlayerNameplateDetour(nint playerNameplateObjectPtr, bool isTitleAboveName, bool isTitleVisible, nint titlePtr, nint namePtr, nint freeCompanyPtr, int iconId);

	[Signature("48 89 5C 24 ?? 48 89 6C 24 ?? 56 57 41 54 41 56 41 57 48 83 EC 40 44 0F B6 E2", DetourName = "SetPlayerNameplateDetour")]
	private readonly Hook<AddonNamePlate_SetPlayerNameplateDetour>? hook_AddonNamePlate_SetPlayerNameplateDetour;

	public bool IsValid => (byte)(1u & (IsHookEnabled<AddonNamePlate_SetPlayerNameplateDetour>(hook_AddonNamePlate_SetPlayerNameplateDetour) ? 1u : 0u)) != 0;

	public event AddonNamePlate_SetPlayerNameEventHandler AddonNamePlate_SetPlayerName;

	public event AddonNamePlate_SetPlayerNameManagedEventHandler AddonNamePlate_SetPlayerNameManaged;

	public NameplateHooks()
	{
		PluginServices.GameInteropProvider.InitializeFromAttributes((object)this);
	}

	~NameplateHooks()
	{
		Dispose();
	}

	public void Dispose()
	{
		Unhook();
	}

	internal void Initialize()
	{
		hook_AddonNamePlate_SetPlayerNameplateDetour?.Enable();
	}

	internal void Unhook()
	{
		hook_AddonNamePlate_SetPlayerNameplateDetour?.Disable();
	}

	private static bool IsHookEnabled<T>(Hook<T> hook) where T : Delegate
	{
		return hook?.IsEnabled ?? false;
	}

	private nint SetPlayerNameplateDetour(nint playerNameplateObjectPtr, bool isTitleAboveName, bool isTitleVisible, nint titlePtr, nint namePtr, nint freeCompanyPtr, int iconId)
	{
		nint result = IntPtr.Zero;
		AddonNamePlate_SetPlayerNameEventArgs eventArgs;
		if (IsHookEnabled<AddonNamePlate_SetPlayerNameplateDetour>(hook_AddonNamePlate_SetPlayerNameplateDetour))
		{
			eventArgs = new AddonNamePlate_SetPlayerNameEventArgs
			{
				PlayerNameplateObjectPtr = playerNameplateObjectPtr,
				TitlePtr = titlePtr,
				NamePtr = namePtr,
				FreeCompanyPtr = freeCompanyPtr,
				IsTitleAboveName = isTitleAboveName,
				IsTitleVisible = isTitleVisible,
				IconID = iconId
			};
			eventArgs.CallOriginal += () => hook_AddonNamePlate_SetPlayerNameplateDetour.Original(eventArgs.PlayerNameplateObjectPtr, eventArgs.IsTitleAboveName, eventArgs.IsTitleVisible, eventArgs.TitlePtr, eventArgs.NamePtr, eventArgs.FreeCompanyPtr, eventArgs.IconID);
			bool flag = this.AddonNamePlate_SetPlayerName != null;
			this.AddonNamePlate_SetPlayerName?.Invoke(eventArgs);
			if (this.AddonNamePlate_SetPlayerNameManaged != null)
			{
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				SafeNameplateObject safeNameplateObject = new SafeNameplateObject(playerNameplateObjectPtr);
				AddonNamePlate_SetPlayerNameManagedEventArgs e = new AddonNamePlate_SetPlayerNameManagedEventArgs
				{
					OriginalEventArgs = eventArgs,
					SafeNameplateObject = safeNameplateObject,
					Title = GameInterfaceHelper.ReadSeString(eventArgs.TitlePtr),
					Name = GameInterfaceHelper.ReadSeString(eventArgs.NamePtr),
					FreeCompany = GameInterfaceHelper.ReadSeString(eventArgs.FreeCompanyPtr)
				};
				byte[] first = e.Title.Encode();
				byte[] first2 = e.Name.Encode();
				byte[] first3 = e.FreeCompany.Encode();
				this.AddonNamePlate_SetPlayerNameManaged(e);
				byte[] array = e.Title.Encode();
				if (!Enumerable.SequenceEqual(first, array))
				{
					eventArgs.TitlePtr = GameInterfaceHelper.PluginAllocate(array);
					flag2 = true;
				}
				byte[] array2 = e.Name.Encode();
				if (!Enumerable.SequenceEqual(first2, array2))
				{
					eventArgs.NamePtr = GameInterfaceHelper.PluginAllocate(array2);
					flag3 = true;
				}
				byte[] array3 = e.FreeCompany.Encode();
				if (!Enumerable.SequenceEqual(first3, array3))
				{
					eventArgs.FreeCompanyPtr = GameInterfaceHelper.PluginAllocate(array3);
					flag4 = true;
				}
				callOriginal();
				if (flag2)
				{
					GameInterfaceHelper.PluginFree(eventArgs.TitlePtr);
				}
				if (flag3)
				{
					GameInterfaceHelper.PluginFree(eventArgs.NamePtr);
				}
				if (flag4)
				{
					GameInterfaceHelper.PluginFree(eventArgs.FreeCompanyPtr);
				}
			}
			else if (!flag)
			{
				callOriginal();
			}
			result = eventArgs.Result;
		}
		return result;
		void callOriginal()
		{
			eventArgs.Result = eventArgs.Original();
		}
	}
}
