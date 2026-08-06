using System.Diagnostics;
using System.Numerics;

namespace PyonPix.Shared.Utility;

public static class MathUtil
{
	public struct SyncedVector3
	{
		public float X { get; set; }

		public float Y { get; set; }

		public float Z { get; set; }
	}

	public struct SyncedVector4
	{
		public float X { get; set; }

		public float Y { get; set; }

		public float Z { get; set; }

		public float W { get; set; }
	}

	public struct SyncedQuaternion
	{
		public float X { get; set; }

		public float Y { get; set; }

		public float Z { get; set; }

		public float W { get; set; }
	}

	public static float Lerp(float a, float b, float t)
	{
		return a + (b - a) * t;
	}

	public static float TicksToSeconds(long ticks)
	{
		return (float)ticks / (float)Stopwatch.Frequency;
	}

	public static SyncedVector3 ToSynced(this Vector3 v)
	{
		return new SyncedVector3
		{
			X = v.X,
			Y = v.Y,
			Z = v.Z
		};
	}

	public static Vector3 ToLocal(this SyncedVector3 v)
	{
		return new Vector3
		{
			X = v.X,
			Y = v.Y,
			Z = v.Z
		};
	}

	public static SyncedVector4 ToSynced(this Vector4 v)
	{
		return new SyncedVector4
		{
			X = v.X,
			Y = v.Y,
			Z = v.Z,
			W = v.W
		};
	}

	public static Vector4 ToLocal(this SyncedVector4 v)
	{
		return new Vector4
		{
			X = v.X,
			Y = v.Y,
			Z = v.Z,
			W = v.W
		};
	}

	public static SyncedQuaternion ToSynced(this Quaternion v)
	{
		return new SyncedQuaternion
		{
			X = v.X,
			Y = v.Y,
			Z = v.Z,
			W = v.W
		};
	}

	public static Quaternion ToLocal(this SyncedQuaternion v)
	{
		return new Quaternion
		{
			X = v.X,
			Y = v.Y,
			Z = v.Z,
			W = v.W
		};
	}
}
