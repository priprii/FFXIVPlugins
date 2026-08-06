using System.Runtime.InteropServices;

namespace Ktisis.Structs.Env.Weather;

[StructLayout(LayoutKind.Explicit, Size = 12)]
public struct EnvWind
{
	[FieldOffset(0)]
	public float Direction;

	[FieldOffset(4)]
	public float Angle;

	[FieldOffset(8)]
	public float Speed;
}
