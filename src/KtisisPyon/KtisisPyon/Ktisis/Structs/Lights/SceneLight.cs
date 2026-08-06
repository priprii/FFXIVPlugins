using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;

namespace Ktisis.Structs.Lights;

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

	[FieldOffset(152)]
	public unsafe TextureResourceHandle* Texture;
}
