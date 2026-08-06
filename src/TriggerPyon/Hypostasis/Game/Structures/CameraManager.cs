using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Hypostasis.Dalamud;

namespace Hypostasis.Game.Structures;

[StructLayout(LayoutKind.Explicit)]
[GameStructure("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 33 F6 48 C7 41 ?? ?? ?? ?? ?? 48 8D 05")]
public struct CameraManager : IHypostasisStructure
{
	[FieldOffset(0)]
	public CameraManager CS;

	[FieldOffset(0)]
	public unsafe GameCamera* worldCamera;

	[FieldOffset(8)]
	public unsafe GameCamera* idleCamera;

	[FieldOffset(16)]
	public unsafe GameCamera* menuCamera;

	[FieldOffset(24)]
	public unsafe GameCamera* spectatorCamera;

	public bool Validate()
	{
		return true;
	}
}
