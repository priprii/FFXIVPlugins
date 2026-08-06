using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;

namespace Ktisis.Structs.Env;

[StructLayout(LayoutKind.Explicit, Size = 1392)]
public struct WaterRendererEx
{
	[FieldOffset(0)]
	public WaterRenderer _base;

	[FieldOffset(320)]
	public float Unk1;

	[FieldOffset(324)]
	public float Unk2;

	[FieldOffset(328)]
	public float Unk3;

	[FieldOffset(332)]
	public float Unk4;
}
