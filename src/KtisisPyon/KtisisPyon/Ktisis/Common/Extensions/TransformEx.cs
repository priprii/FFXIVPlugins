using System;
using System.Numerics;
using FFXIVClientStructs.Havok.Common.Base.Math.Quaternion;
using FFXIVClientStructs.Havok.Common.Base.Math.Vector;
using Ktisis.Common.Utility;

namespace Ktisis.Common.Extensions;

public static class TransformEx
{
	public static Vector3 ModelToWorldPos(this Vector3 target, Transform offset)
	{
		return Vector3.Transform(target, offset.Rotation) * offset.Scale;
	}

	public static Vector3 WorldToModelPos(this Vector3 target, Transform offset)
	{
		return Vector3.Transform(target - offset.Position, Quaternion.Inverse(offset.Rotation)) / offset.Scale;
	}

	public static Vector3 ToVector3(this hkVector4f hkVec)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(hkVec.X, hkVec.Y, hkVec.Z);
	}

	public static hkVector4f ToHavok(this Vector3 v)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		return new hkVector4f
		{
			X = v.X,
			Y = v.Y,
			Z = v.Z,
			W = 0f
		};
	}

	public static hkVector4f ToHavokRounded(this Vector3 v)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		return new hkVector4f
		{
			X = MathF.Round(v.X, 4),
			Y = MathF.Round(v.Y, 4),
			Z = MathF.Round(v.Z, 4),
			W = 0f
		};
	}

	public static Quaternion ToQuaternion(this hkQuaternionf hkQuat)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return new Quaternion(hkQuat.X, hkQuat.Y, hkQuat.Z, hkQuat.W);
	}

	public static hkQuaternionf ToHavok(this Quaternion quat)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		return new hkQuaternionf
		{
			X = quat.X,
			Y = quat.Y,
			Z = quat.Z,
			W = quat.W
		};
	}

	public static hkQuaternionf ToHavokRounded(this Quaternion quat)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		return new hkQuaternionf
		{
			X = MathF.Round(quat.X, 4),
			Y = MathF.Round(quat.Y, 4),
			Z = MathF.Round(quat.Z, 4),
			W = MathF.Round(quat.W, 4)
		};
	}
}
