using System.Numerics;
using System.Runtime.InteropServices;

namespace Ktisis.Structs.Env.Weather;

[StructLayout(LayoutKind.Explicit, Size = 64)]
public struct EnvLighting
{
	[FieldOffset(0)]
	public Vector3 SunLightColor;

	[FieldOffset(12)]
	public Vector3 MoonLightColor;

	[FieldOffset(24)]
	public Vector3 Ambient;

	[FieldOffset(36)]
	public float _unk1;

	[FieldOffset(40)]
	public float AmbientSaturation;

	[FieldOffset(44)]
	public float Temperature;

	[FieldOffset(48)]
	public float _unk2;

	[FieldOffset(52)]
	public float _unk3;

	[FieldOffset(56)]
	public float _unk4;
}
