using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;

namespace PyonPix.Structs.Light;

[StructLayout(LayoutKind.Explicit, Size = 160)]
public struct SceneLight
{
	[FieldOffset(0)]
	public unsafe nint* _vf;

	[FieldOffset(0)]
	public DrawObject DrawObject;

	[FieldOffset(80)]
	public Transform Transform;

	[FieldOffset(128)]
	public nint Culling;

	[FieldOffset(136)]
	public byte Flags00;

	[FieldOffset(137)]
	public byte Flags01;

	[FieldOffset(144)]
	public unsafe RenderLight* RenderLight;
}
