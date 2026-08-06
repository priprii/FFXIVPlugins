using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;

namespace Ktisis.Structs.Vfx;

[StructLayout(LayoutKind.Explicit, Size = 832)]
public struct VfxObject
{
	[FieldOffset(0)]
	public Object Object;

	[FieldOffset(608)]
	public Vector4 Color;

	[FieldOffset(672)]
	public unsafe VfxResourceInstance* ResourceInstance;
}
