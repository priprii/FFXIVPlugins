using System.Numerics;
using System.Runtime.InteropServices;

namespace Ktisis.Structs.Env.Weather;

[StructLayout(LayoutKind.Explicit, Size = 40)]
public struct EnvClouds
{
	[FieldOffset(0)]
	public Vector3 CloudColor;

	[FieldOffset(12)]
	public Vector3 Color2;

	[FieldOffset(24)]
	public float Gradient;

	[FieldOffset(28)]
	public float SideHeight;

	[FieldOffset(32)]
	public uint CloudTexture;

	[FieldOffset(36)]
	public uint CloudSideTexture;
}
