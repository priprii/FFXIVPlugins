using System;
using System.Numerics;

namespace Ktisis.Common.Utility;

internal static class HkaEulerAngles
{
	private enum Axis
	{
		X,
		Y,
		Z,
		W
	}

	internal const float Deg2Rad = (float)Math.PI / 180f;

	internal const float Rad2Deg = 180f / (float)Math.PI;

	private static int EulFrmS = 0;

	private static int EulFrmR = 1;

	private static int EulRepNo = 0;

	private static int EulRepYes = 1;

	private static int EulParEven = 0;

	private static int EulParOdd = 1;

	private static int[] EulSafe = new int[4] { 0, 1, 2, 0 };

	private static int[] EulNext = new int[4] { 1, 2, 0, 1 };

	private static int Order = EulOrd(Axis.Z, EulParEven, EulRepNo, EulFrmS);

	private static void EulGetOrd(int ord, out int i, out int j, out int k, out int h, out int n, out int s, out int f)
	{
		int num = ord;
		f = num & 1;
		num >>= 1;
		s = num & 1;
		num >>= 1;
		n = num & 1;
		num >>= 1;
		i = EulSafe[num & 3];
		j = EulNext[i + n];
		k = EulNext[i + 1 - n];
		h = ((s == 1) ? k : i);
	}

	private static int EulOrd(Axis i, int p, int r, int f)
	{
		return ((((int)i << 1) + p << 1) + r << 1) + f;
	}

	internal static Vector3 MatrixToEuler(Matrix4x4 m)
	{
		Vector3 vector = default(Vector3);
		float[,] array = new float[4, 4]
		{
			{ m.M11, m.M12, m.M13, m.M14 },
			{ m.M21, m.M22, m.M23, m.M24 },
			{ m.M31, m.M32, m.M33, m.M34 },
			{ m.M41, m.M42, m.M43, m.M44 }
		};
		EulGetOrd(Order, out var i, out var j, out var k, out var _, out var n, out var s, out var f);
		if (s == EulRepYes)
		{
			float num = MathF.Sqrt(array[i, j] * array[i, j] + array[i, k] * array[i, k]);
			if (num > 2.2E-44f)
			{
				vector.X = MathF.Atan2(array[i, j], array[i, k]);
				vector.Y = MathF.Atan2(num, array[i, i]);
				vector.Z = MathF.Atan2(array[j, i], 0f - array[k, i]);
			}
			else
			{
				vector.X = MathF.Atan2(0f - array[j, k], array[j, j]);
				vector.Y = MathF.Atan2(num, array[i, i]);
				vector.Z = 0f;
			}
		}
		else
		{
			float num2 = MathF.Sqrt(array[i, i] * array[i, i] + array[j, i] * array[j, i]);
			if (num2 > 2.2E-44f)
			{
				vector.X = MathF.Atan2(array[k, j], array[k, k]);
				vector.Y = MathF.Atan2(0f - array[k, i], num2);
				vector.Z = MathF.Atan2(array[j, i], array[i, i]);
			}
			else
			{
				vector.X = MathF.Atan2(0f - array[j, k], array[j, j]);
				vector.Y = MathF.Atan2(0f - array[k, i], num2);
				vector.Z = 0f;
			}
		}
		if (n == EulParOdd)
		{
			vector.X = 0f - vector.X;
			vector.Y = 0f - vector.Y;
			vector.Z = 0f - vector.Z;
		}
		if (f == EulFrmR)
		{
			float x = vector.X;
			vector.X = vector.Z;
			vector.Z = x;
		}
		return new Vector3(vector.Y, vector.Z, vector.X) * (180f / (float)Math.PI);
	}

	internal static Matrix4x4 EulerToMatrix(Vector3 v)
	{
		Vector3 vector = new Vector3(v.Z, v.X, v.Y) * ((float)Math.PI / 180f);
		EulGetOrd(Order, out var i, out var j, out var k, out var _, out var n, out var s, out var f);
		if (f == EulFrmR)
		{
			float x = vector.X;
			vector.X = vector.Z;
			vector.Z = x;
		}
		if (n == EulParOdd)
		{
			vector.X = 0f - vector.X;
			vector.Y = 0f - vector.Y;
			vector.Z = 0f - vector.Z;
		}
		float[,] array = new float[4, 4];
		float x2 = vector.X;
		float y = vector.Y;
		float z = vector.Z;
		float num = MathF.Cos(x2);
		float num2 = MathF.Cos(y);
		float num3 = MathF.Cos(z);
		float num4 = MathF.Sin(x2);
		float num5 = MathF.Sin(y);
		float num6 = MathF.Sin(z);
		float num7 = num * num3;
		float num8 = num * num6;
		float num9 = num4 * num3;
		float num10 = num4 * num6;
		if (s == EulRepYes)
		{
			array[i, i] = num2;
			array[i, j] = num5 * num4;
			array[i, k] = num5 * num;
			array[j, i] = num5 * num6;
			array[j, j] = (0f - num2) * num10 + num7;
			array[j, k] = (0f - num2) * num8 - num9;
			array[k, i] = (0f - num5) * num3;
			array[k, j] = num2 * num9 + num8;
			array[k, k] = num2 * num7 - num10;
		}
		else
		{
			array[i, i] = num2 * num3;
			array[i, j] = num5 * num9 - num8;
			array[i, k] = num5 * num7 + num10;
			array[j, i] = num2 * num6;
			array[j, j] = num5 * num10 + num7;
			array[j, k] = num5 * num8 - num9;
			array[k, i] = 0f - num5;
			array[k, j] = num2 * num4;
			array[k, k] = num2 * num;
		}
		return new Matrix4x4(array[0, 0], array[0, 1], array[0, 2], array[0, 3], array[1, 0], array[1, 1], array[1, 2], array[1, 3], array[2, 0], array[2, 1], array[2, 2], array[2, 3], array[3, 0], array[3, 1], array[3, 2], array[3, 3]);
	}

	internal static Vector3 ToEuler(Quaternion q)
	{
		float num = q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W;
		float num2 = ((num > 0f) ? (2f / num) : 0f);
		float num3 = q.X * num2;
		float num4 = q.Y * num2;
		float num5 = q.Z * num2;
		float num6 = q.W * num3;
		float num7 = q.W * num4;
		float num8 = q.W * num5;
		float num9 = q.X * num3;
		float num10 = q.X * num4;
		float num11 = q.X * num5;
		float num12 = q.Y * num4;
		float num13 = q.Y * num5;
		float num14 = q.Z * num5;
		return MatrixToEuler(new Matrix4x4(1f - (num12 + num14), num10 - num8, num11 + num7, 0f, num10 + num8, 1f - (num9 + num14), num13 - num6, 0f, num11 - num7, num13 + num6, 1f - (num9 + num12), 0f, 0f, 0f, 0f, 1f)).NormalizeAngles();
	}

	public static Quaternion ToQuaternion(Vector3 v)
	{
		Vector3 vector = new Vector3(v.Z, v.X, v.Y) * ((float)Math.PI / 180f);
		Quaternion result = default(Quaternion);
		float[] array = new float[3];
		EulGetOrd(Order, out var i, out var j, out var k, out var _, out var n, out var s, out var f);
		if (f == EulFrmR)
		{
			float x = vector.X;
			vector.X = vector.Z;
			vector.Z = x;
		}
		if (n == EulParOdd)
		{
			vector.Y = 0f - vector.Y;
		}
		float x2 = vector.X * 0.5f;
		float x3 = vector.Y * 0.5f;
		float x4 = vector.Z * 0.5f;
		float num = MathF.Cos(x2);
		float num2 = MathF.Cos(x3);
		float num3 = MathF.Cos(x4);
		float num4 = MathF.Sin(x2);
		float num5 = MathF.Sin(x3);
		float num6 = MathF.Sin(x4);
		float num7 = num * num3;
		float num8 = num * num6;
		float num9 = num4 * num3;
		float num10 = num4 * num6;
		if (s == EulRepYes)
		{
			array[i] = num2 * (num8 + num9);
			array[j] = num5 * (num7 + num10);
			array[k] = num5 * (num8 - num9);
			result.W = num2 * (num7 - num10);
		}
		else
		{
			array[i] = num2 * num9 - num5 * num8;
			array[j] = num2 * num10 + num5 * num7;
			array[k] = num2 * num8 - num5 * num9;
			result.W = num2 * num7 + num5 * num10;
		}
		if (n == EulParOdd)
		{
			array[j] = 0f - array[j];
		}
		result.X = array[0];
		result.Y = array[1];
		result.Z = array[2];
		return result;
	}

	public static float GetYaw(Quaternion q)
	{
		return MathF.Atan2(2f * (q.W * q.Y + q.X * q.Z), 1f - 2f * (q.Y * q.Y + q.Z * q.Z));
	}
}
