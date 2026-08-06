using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace PyonCam.Services;

[StructLayout(LayoutKind.Explicit)]
public struct CameraManager
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
}
