using System.Numerics;
using System.Runtime.InteropServices;

namespace Ktisis.Structs.Env.Weather;

[StructLayout(LayoutKind.Explicit, Size = 52)]
public struct EnvRain
{
	[FieldOffset(0)]
	public float Raindrops;

	[FieldOffset(4)]
	public float Intensity;

	[FieldOffset(8)]
	public float Weight;

	[FieldOffset(12)]
	public float Scatter;

	[FieldOffset(16)]
	public float _unk1;

	[FieldOffset(20)]
	public float Size;

	[FieldOffset(24)]
	public Vector4 Color;

	[FieldOffset(40)]
	public float _unk2;

	[FieldOffset(44)]
	public float _unk3;

	[FieldOffset(48)]
	public uint _unk4;
}
