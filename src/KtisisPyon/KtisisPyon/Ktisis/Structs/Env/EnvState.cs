using System.Runtime.InteropServices;
using Ktisis.Structs.Env.Weather;

namespace Ktisis.Structs.Env;

[StructLayout(LayoutKind.Explicit, Size = 760)]
public struct EnvState
{
	[FieldOffset(8)]
	public uint SkyId;

	[FieldOffset(32)]
	public EnvLighting Lighting;

	[FieldOffset(152)]
	public EnvStars Stars;

	[FieldOffset(192)]
	public EnvFog Fog;

	[FieldOffset(328)]
	public EnvClouds Clouds;

	[FieldOffset(368)]
	public EnvRain Rain;

	[FieldOffset(420)]
	public EnvDust Dust;

	[FieldOffset(472)]
	public EnvWind Wind;
}
