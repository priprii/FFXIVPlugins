using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using DotNet.Globbing;
using FFXIVClientStructs.FFXIV.Client.System.Resource;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;
using FFXIVClientStructs.STD;

namespace SoundPyon;

internal class Filter : IDisposable
{
	private static class Signatures
	{
		internal const string PlaySpecificSound = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 33 F6 8B DA 48 8B F9 0F BA E2 0F";

		internal const string GetResourceSync = "E8 ?? ?? ?? ?? 48 8B C8 8B C3 F0 0F C0 81";

		internal const string GetResourceAsync = "E8 ?? ?? ?? 00 48 8B D8 EB ?? F0 FF 83 ?? ?? 00 00";

		internal const string LoadSoundFile = "E8 ?? ?? ?? ?? 48 85 C0 75 12 B0 F6";
	}

	private unsafe delegate void* PlaySpecificSoundDelegate(long a1, int idx);

	private unsafe delegate ResourceHandle* GetResourceSyncPrototype(ResourceManager* resourceManager, ResourceCategory* pCategoryId, ResourceType* pResourceType, int* pResourceHash, byte* pPath, GetResourceParameters* pGetResParams, nint unk7, uint unk8);

	private unsafe delegate ResourceHandle* GetResourceAsyncPrototype(ResourceManager* resourceManager, ResourceCategory* pCategoryId, ResourceType* pResourceType, int* pResourceHash, byte* pPath, GetResourceParameters* pGetResParams, byte isUnknown, nint unk8, uint unk9);

	private delegate nint LoadSoundFileDelegate(nint resourceHandle, uint a2);

	private const int ResourceDataPointerOffset = 176;

	internal readonly ConcurrentDictionary<string, LoggedSound> Recent = new ConcurrentDictionary<string, LoggedSound>();

	private Hook<PlaySpecificSoundDelegate>? PlaySpecificSoundHook { get; set; }

	private Hook<GetResourceSyncPrototype>? GetResourceSyncHook { get; set; }

	private Hook<GetResourceAsyncPrototype>? GetResourceAsyncHook { get; set; }

	private Hook<LoadSoundFileDelegate>? LoadSoundFileHook { get; set; }

	private Plugin Plugin { get; }

	private ConcurrentDictionary<nint, string> Scds { get; } = new ConcurrentDictionary<nint, string>();

	private nint NoSoundPtr { get; }

	private nint InfoPtr { get; }

	internal Filter(Plugin plugin)
	{
		Plugin = plugin;
		(nint noSoundPtr, nint infoPtr) tuple = SetUpNoSound();
		nint item = tuple.noSoundPtr;
		nint item2 = tuple.infoPtr;
		NoSoundPtr = item;
		InfoPtr = item2;
	}

	private static byte[] GetNoSoundScd()
	{
		Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SoundPyon.Resources.nosound.scd");
		if (manifestResourceStream == null)
		{
			return Array.Empty<byte>();
		}
		using MemoryStream memoryStream = new MemoryStream();
		manifestResourceStream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	private static (nint noSoundPtr, nint infoPtr) SetUpNoSound()
	{
		byte[] noSoundScd = GetNoSoundScd();
		nint num = Marshal.AllocHGlobal(noSoundScd.Length);
		Marshal.Copy(noSoundScd, 0, num, noSoundScd.Length);
		nint num2 = Marshal.AllocHGlobal(256);
		Marshal.WriteIntPtr(num2 + 8, num);
		Marshal.WriteInt32(num2 + 136, 84);
		Marshal.WriteInt16(num2 + 148, 0);
		return (noSoundPtr: num, infoPtr: num2);
	}

	internal unsafe void Enable()
	{
		nint num = default(nint);
		if (PlaySpecificSoundHook == null && Plugin.SigScanner.TryScanText("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 33 F6 8B DA 48 8B F9 0F BA E2 0F", ref num))
		{
			PlaySpecificSoundHook = Plugin.GameInteropProvider.HookFromAddress<PlaySpecificSoundDelegate>((IntPtr)num, (PlaySpecificSoundDelegate)PlaySpecificSoundDetour, (HookBackend)0);
		}
		nint num2 = default(nint);
		if (GetResourceSyncHook == null && Plugin.SigScanner.TryScanText("E8 ?? ?? ?? ?? 48 8B C8 8B C3 F0 0F C0 81", ref num2))
		{
			GetResourceSyncHook = Plugin.GameInteropProvider.HookFromAddress<GetResourceSyncPrototype>((IntPtr)num2, (GetResourceSyncPrototype)GetResourceSyncDetour, (HookBackend)0);
		}
		nint num3 = default(nint);
		if (GetResourceAsyncHook == null && Plugin.SigScanner.TryScanText("E8 ?? ?? ?? 00 48 8B D8 EB ?? F0 FF 83 ?? ?? 00 00", ref num3))
		{
			GetResourceAsyncHook = Plugin.GameInteropProvider.HookFromAddress<GetResourceAsyncPrototype>((IntPtr)num3, (GetResourceAsyncPrototype)GetResourceAsyncDetour, (HookBackend)0);
		}
		nint num4 = default(nint);
		if (LoadSoundFileHook == null && Plugin.SigScanner.TryScanText("E8 ?? ?? ?? ?? 48 85 C0 75 12 B0 F6", ref num4))
		{
			LoadSoundFileHook = Plugin.GameInteropProvider.HookFromAddress<LoadSoundFileDelegate>((IntPtr)num4, (LoadSoundFileDelegate)LoadSoundFileDetour, (HookBackend)0);
		}
		PlaySpecificSoundHook?.Enable();
		LoadSoundFileHook?.Enable();
		GetResourceSyncHook?.Enable();
		GetResourceAsyncHook?.Enable();
	}

	internal void Disable()
	{
		PlaySpecificSoundHook?.Disable();
		LoadSoundFileHook?.Disable();
		GetResourceSyncHook?.Disable();
		GetResourceAsyncHook?.Disable();
	}

	public void Dispose()
	{
		PlaySpecificSoundHook?.Dispose();
		LoadSoundFileHook?.Dispose();
		GetResourceSyncHook?.Dispose();
		GetResourceAsyncHook?.Dispose();
		Marshal.FreeHGlobal(InfoPtr);
		Marshal.FreeHGlobal(NoSoundPtr);
	}

	private unsafe void* PlaySpecificSoundDetour(long a1, int idx)
	{
		try
		{
			if (PlaySpecificSoundDetourInner(a1, idx))
			{
				a1 = InfoPtr;
				idx = 0;
			}
		}
		catch (Exception ex)
		{
			SoundPyon.Plugin.Log.Error(ex, "Error in PlaySpecificSoundDetour", Array.Empty<object>());
		}
		return PlaySpecificSoundHook.Original(a1, idx);
	}

	private unsafe bool PlaySpecificSoundDetourInner(long a1, int idx)
	{
		if (a1 == 0L)
		{
			return false;
		}
		byte* ptr = *(byte**)(a1 + 8);
		if (ptr == null)
		{
			return false;
		}
		if (!Scds.TryGetValue((nint)ptr, out string value))
		{
			return false;
		}
		value = value.ToLowerInvariant();
		string specificPath = $"{value}/{idx}";
		bool flag = SoundPyon.Plugin.Config.Globs.Where<KeyValuePair<Glob, bool>>((KeyValuePair<Glob, bool> entry) => entry.Value).Any((KeyValuePair<Glob, bool> entry) => entry.Key.IsMatch(specificPath));
		if (SoundPyon.Plugin.MainWindow.LogSounds && (!flag || SoundPyon.Plugin.MainWindow.LogFilteredSounds))
		{
			Recent.AddOrUpdate(specificPath, (string _) => new LoggedSound
			{
				Path = specificPath,
				Count = 1,
				LastPlayed = DateTime.UtcNow
			}, delegate(string _, LoggedSound existing)
			{
				existing.Count = Math.Min(existing.Count + 1, 999);
				existing.LastPlayed = DateTime.UtcNow;
				return existing;
			});
			if (Recent.Count > SoundPyon.Plugin.Config.LogLimit)
			{
				foreach (LoggedSound item in Recent.Values.OrderBy((LoggedSound sound) => sound.LastPlayed).Take((int)(Recent.Count - SoundPyon.Plugin.Config.LogLimit)))
				{
					Recent.TryRemove(item.Path, out LoggedSound _);
				}
			}
		}
		return flag;
	}

	private unsafe ResourceHandle* GetResourceSyncDetour(ResourceManager* resourceManager, ResourceCategory* categoryId, ResourceType* resourceType, int* resourceHash, byte* path, GetResourceParameters* pGetResParams, nint unk8, uint unk9)
	{
		return GetResourceHandler(isSync: true, resourceManager, categoryId, resourceType, resourceHash, path, pGetResParams, 0, unk8, unk9);
	}

	private unsafe ResourceHandle* GetResourceAsyncDetour(ResourceManager* resourceManager, ResourceCategory* categoryId, ResourceType* resourceType, int* resourceHash, byte* path, GetResourceParameters* pGetResParams, byte isUnk, nint unk8, uint unk9)
	{
		return GetResourceHandler(isSync: false, resourceManager, categoryId, resourceType, resourceHash, path, pGetResParams, isUnk, unk8, unk9);
	}

	private unsafe ResourceHandle* GetResourceHandler(bool isSync, ResourceManager* resourceManager, ResourceCategory* categoryId, ResourceType* resourceType, int* resourceHash, byte* path, GetResourceParameters* pGetResParams, byte isUnk, nint unk8, uint unk9)
	{
		ResourceHandle* ptr = (isSync ? GetResourceSyncHook.Original(resourceManager, categoryId, resourceType, resourceHash, path, pGetResParams, unk8, unk9) : GetResourceAsyncHook.Original(resourceManager, categoryId, resourceType, resourceHash, path, pGetResParams, isUnk, unk8, unk9));
		string text = ReadTerminatedString(path);
		if (ptr != null && text.EndsWith(".scd"))
		{
			nint num = Marshal.ReadIntPtr((nint)((byte*)ptr + 176));
			if (num != IntPtr.Zero)
			{
				Scds[num] = text;
			}
		}
		return ptr;
	}

	private unsafe nint LoadSoundFileDetour(nint resourceHandle, uint a2)
	{
		nint result = LoadSoundFileHook.Original(resourceHandle, a2);
		try
		{
			string text = ((object)(*(StdString*)(&((ResourceHandle)resourceHandle).FileName))/*cast due to constrained. prefix*/).ToString();
			if (text.EndsWith(".scd"))
			{
				nint key = Marshal.ReadIntPtr(resourceHandle + 176);
				Scds[key] = text;
			}
		}
		catch (Exception ex)
		{
			SoundPyon.Plugin.Log.Error(ex, "Error in LoadSoundFileDetour", Array.Empty<object>());
		}
		return result;
	}

	private unsafe static byte[] ReadTerminatedBytes(byte* ptr)
	{
		if (ptr == null)
		{
			return Array.Empty<byte>();
		}
		List<byte> list = new List<byte>();
		while (*ptr != 0)
		{
			list.Add(*ptr);
			ptr++;
		}
		return list.ToArray();
	}

	internal unsafe static string ReadTerminatedString(byte* ptr)
	{
		return Encoding.UTF8.GetString(ReadTerminatedBytes(ptr));
	}
}
