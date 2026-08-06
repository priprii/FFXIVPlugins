using System;
using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Common.Math;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using Ktisis.Common.Extensions;

namespace Ktisis.Common.Utility;

[StructLayout(LayoutKind.Explicit)]
public class Transform : IEquatable<Transform>
{
	[FieldOffset(0)]
	public Vector3 Position;

	[FieldOffset(16)]
	public Quaternion Rotation;

	[FieldOffset(32)]
	public Vector3 Scale;

	public Transform()
	{
		Position = Vector3.Zero;
		Rotation = Quaternion.Identity;
		Scale = Vector3.One;
	}

	public Transform(Vector3 pos, Quaternion rot, Vector3 scale)
	{
		Position = pos;
		Rotation = rot;
		Scale = scale;
	}

	public Transform(hkQsTransformf hk)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Position = hk.Translation.ToVector3();
		Rotation = hk.Rotation.ToQuaternion();
		Scale = hk.Scale.ToVector3();
	}

	public Transform(Transform trans)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Position = Vector3.op_Implicit(trans.Position);
		Rotation = Quaternion.op_Implicit(trans.Rotation);
		Scale = Vector3.op_Implicit(trans.Scale);
	}

	public Transform(Matrix4x4 mx)
	{
		DecomposeMatrix(mx);
	}

	public Transform(Matrix4x4 mx, Transform initial)
	{
		DecomposeMatrixPrecise(mx, initial);
	}

	public Transform(Vector3 pos)
	{
		Position = pos;
		Rotation = Quaternion.Identity;
		Scale = Vector3.One;
	}

	public Matrix4x4 ComposeMatrix()
	{
		Matrix4x4 matrix4x = Matrix4x4.CreateScale(Scale);
		Matrix4x4 matrix4x2 = Matrix4x4.CreateFromQuaternion(Rotation);
		Matrix4x4 matrix4x3 = Matrix4x4.CreateTranslation(Position);
		return matrix4x * matrix4x2 * matrix4x3;
	}

	public void DecomposeMatrix(Matrix4x4 mx)
	{
		Matrix4x4.Decompose(mx, out var scale, out var rotation, out var translation);
		Position = translation;
		Rotation = rotation;
		Scale = scale;
	}

	public void DecomposeMatrixPrecise(Matrix4x4 mx, Transform initial)
	{
		Vector3 position = initial.Position;
		Quaternion rotation = initial.Rotation;
		Vector3 scale = initial.Scale;
		if (!Matrix4x4.Decompose(mx, out var scale2, out var rotation2, out var translation))
		{
			Ktisis.Log.Warning("Failed to decompose matrix!");
		}
		Position = (((translation - position).LengthSquared() < 1E-12f) ? position : translation);
		if (Quaternion.Dot(rotation2, rotation) < 0f)
		{
			rotation2 = new Quaternion(0f - rotation2.X, 0f - rotation2.Y, 0f - rotation2.Z, 0f - rotation2.W);
		}
		float x = Math.Clamp(Quaternion.Dot(rotation2, rotation), -1f, 1f);
		float num = 2f * MathF.Acos(x);
		Rotation = ((num < 1E-06f) ? rotation : rotation2);
		Vector3 scale3 = scale2;
		scale3.X = (IsScaleJitter(scale.X, scale2.X, 1E-06f) ? scale.X : scale2.X);
		scale3.Y = (IsScaleJitter(scale.Y, scale2.Y, 1E-06f) ? scale.Y : scale2.Y);
		scale3.Z = (IsScaleJitter(scale.Z, scale2.Z, 1E-06f) ? scale.Z : scale2.Z);
		Scale = scale3;
	}

	private static bool IsScaleJitter(float a, float b, float relEps)
	{
		float num = MathF.Max(MathF.Abs(a), 1E-06f);
		return MathF.Abs(b - a) / num < relEps;
	}

	public Transform Set(Transform t)
	{
		Position = t.Position;
		Rotation = t.Rotation;
		Scale = t.Scale;
		return this;
	}

	public static implicit operator Transform(Transform trans)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		return new Transform
		{
			Position = Vector3.op_Implicit(trans.Position),
			Rotation = Quaternion.op_Implicit(trans.Rotation),
			Scale = Vector3.op_Implicit(trans.Scale)
		};
	}

	public bool Equals(Transform? trans)
	{
		if (trans != null && Position.Equals(trans.Position) && Rotation.Equals(trans.Rotation))
		{
			return Scale.Equals(trans.Scale);
		}
		return false;
	}
}
