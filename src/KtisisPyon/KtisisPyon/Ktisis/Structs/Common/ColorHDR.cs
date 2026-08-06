using System.Numerics;
using System.Runtime.InteropServices;

namespace Ktisis.Structs.Common;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct ColorHDR
{
	[FieldOffset(0)]
	public Vector3 _vec3 = new Vector3(16f, 16f, 16f);

	[FieldOffset(0)]
	public float Red = 0f;

	[FieldOffset(4)]
	public float Green = 0f;

	[FieldOffset(8)]
	public float Blue = 0f;

	[FieldOffset(12)]
	public float Intensity = 1f;

	public Vector3 RGB
	{
		get
		{
			return Vector3.SquareRoot(_vec3) / 4f;
		}
		set
		{
			value *= 4f;
			_vec3 = value * value;
		}
	}

	public ColorHDR()
	{
	}
}
