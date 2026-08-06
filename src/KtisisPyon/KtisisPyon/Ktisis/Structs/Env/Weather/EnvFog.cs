using System.Numerics;
using System.Runtime.InteropServices;

namespace Ktisis.Structs.Env.Weather;

[StructLayout(LayoutKind.Explicit, Size = 40)]
public struct EnvFog
{
	[FieldOffset(0)]
	public Vector4 Color;

	[FieldOffset(16)]
	public float Distance;

	[FieldOffset(20)]
	public float Thickness;

	[FieldOffset(24)]
	public float _unk1;

	[FieldOffset(28)]
	public float _unk2;

	[FieldOffset(32)]
	public float Opacity;

	[FieldOffset(36)]
	public float SkyVisibility;
}
