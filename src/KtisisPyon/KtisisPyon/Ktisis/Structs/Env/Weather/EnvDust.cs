using System.Numerics;
using System.Runtime.InteropServices;

namespace Ktisis.Structs.Env.Weather;

[StructLayout(LayoutKind.Explicit, Size = 52)]
public struct EnvDust
{
	[FieldOffset(0)]
	public float _unk1;

	[FieldOffset(4)]
	public float Intensity;

	[FieldOffset(8)]
	public float Weight;

	[FieldOffset(12)]
	public float Spread;

	[FieldOffset(16)]
	public float Speed;

	[FieldOffset(20)]
	public float Size;

	[FieldOffset(24)]
	public Vector4 Color;

	[FieldOffset(40)]
	public float Glow;

	[FieldOffset(44)]
	public float Spin;

	[FieldOffset(48)]
	public uint TextureId;
}
