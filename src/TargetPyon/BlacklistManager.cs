using System;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using Dalamud.Memory;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace TargetPyon;

public class BlacklistManager
{
	private unsafe delegate nint ResolveTextCommandPlaceholderDelegate(nint a1, byte* placeholderText, byte a3, byte a4);

	private readonly nint placeholderNamePtr = Marshal.AllocHGlobal(128);

	private readonly string placeholder = $"<{Guid.NewGuid():N}>";

	private string? replacementName;

	[Signature("E8 ?? ?? ?? ?? 48 85 C0 0F 84 ?? ?? ?? ?? 48 8B D0 49 8D 4F", DetourName = "ResolveTextCommandPlaceholderDetour")]
	private Hook<ResolveTextCommandPlaceholderDelegate>? ResolveTextCommandPlaceholderHook { get; init; }

	private unsafe nint ResolveTextCommandPlaceholderDetour(nint a1, byte* placeholderText, byte a3, byte a4)
	{
		string text = MemoryHelper.ReadStringNullTerminated((IntPtr)(nint)placeholderText);
		if (replacementName == null || text != placeholder)
		{
			return ResolveTextCommandPlaceholderHook.Original(a1, placeholderText, a3, a4);
		}
		MemoryHelper.WriteString((IntPtr)placeholderNamePtr, replacementName);
		replacementName = null;
		return placeholderNamePtr;
	}

	public BlacklistManager()
	{
		Plugin.GameInteropProvider.InitializeFromAttributes((object)this);
		ResolveTextCommandPlaceholderHook?.Enable();
	}

	public unsafe void Block(string name, string world)
	{
		replacementName = name + "@" + world;
		((UIModule)UIModule.Instance()).ProcessChatBoxEntry(Utf8String.FromString("/blacklist add " + placeholder), (IntPtr)0, false);
	}

	public unsafe void Unblock(string name, string world)
	{
		replacementName = name + "@" + world;
		((UIModule)UIModule.Instance()).ProcessChatBoxEntry(Utf8String.FromString("/blacklist remove " + placeholder), (IntPtr)0, false);
	}

	public void Dispose()
	{
		Marshal.FreeHGlobal(placeholderNamePtr);
		ResolveTextCommandPlaceholderHook?.Dispose();
	}
}
