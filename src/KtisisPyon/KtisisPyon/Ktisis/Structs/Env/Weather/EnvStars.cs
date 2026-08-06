using System.Numerics;
using System.Runtime.InteropServices;

namespace Ktisis.Structs.Env.Weather;

[StructLayout(LayoutKind.Explicit, Size = 40)]
public struct EnvStars
{
	[FieldOffset(0)]
	public float ConstellationIntensity;

	[FieldOffset(4)]
	public float Constellations;

	[FieldOffset(8)]
	public float Stars;

	[FieldOffset(12)]
	public float GalaxyIntensity;

	[FieldOffset(16)]
	public float StarIntensity;

	[FieldOffset(20)]
	public Vector4 MoonColor;

	[FieldOffset(36)]
	public float MoonBrightness;
}
