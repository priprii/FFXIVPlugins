using System.Numerics;
using System.Runtime.InteropServices;

namespace PyonPix.Structs.Light;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct ColorHDR
{
	[FieldOffset(0)]
	public Vector3 _vec3;

	[FieldOffset(0)]
	public float Red;

	[FieldOffset(4)]
	public float Green;

	[FieldOffset(8)]
	public float Blue;

	[FieldOffset(12)]
	public float Intensity;

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

	public ColorHDR(Vector4 rgba, float intensityMultiplier)
	{
		Red = 0f;
		Green = 0f;
		Blue = 0f;
		_vec3 = new Vector3(rgba.X, rgba.Y, rgba.Z);
		Intensity = rgba.W * intensityMultiplier;
	}
}
