using System;
using System.Numerics;

namespace Ktisis.Common.Utility;

public static class MathHelpers
{
	public static readonly float Deg2Rad = (float)Math.PI / 180f;

	public static readonly float Rad2Deg = 180f / (float)Math.PI;

	public static Quaternion EulerAnglesToQuaternion(this Vector3 vec)
	{
		Vector3 vector = vec.NormalizeAngles() * Deg2Rad;
		float x = vector.X * 0.5f;
		float w = MathF.Cos(x);
		float x2 = MathF.Sin(x);
		float x3 = vector.Y * 0.5f;
		float w2 = MathF.Cos(x3);
		float y = MathF.Sin(x3);
		float x4 = vector.Z * 0.5f;
		float w3 = MathF.Cos(x4);
		float z = MathF.Sin(x4);
		Quaternion quaternion = new Quaternion(x2, 0f, 0f, w);
		Quaternion quaternion2 = new Quaternion(0f, y, 0f, w2);
		return new Quaternion(0f, 0f, z, w3) * quaternion2 * quaternion;
	}

	private static float NormalizeAngle(float angle)
	{
		if (angle > 360f)
		{
			angle = 0f + angle % 360f;
		}
		else if (angle < -1E-45f)
		{
			angle = 360f - (360f - angle) % 360f;
		}
		return angle;
	}

	public static Vector3 NormalizeAngles(this Vector3 vec)
	{
		return new Vector3(NormalizeAngle(vec.X), NormalizeAngle(vec.Y), NormalizeAngle(vec.Z));
	}
}
