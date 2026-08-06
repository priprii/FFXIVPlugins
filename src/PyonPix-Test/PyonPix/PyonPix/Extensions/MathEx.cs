using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using PyonPix.Shared.Structs.Renderer;

namespace PyonPix.Extensions;

public static class MathEx
{
	public static float DegToRad(this float degrees)
	{
		return degrees * ((float)Math.PI / 180f);
	}

	public static float RadToDeg(this float radians)
	{
		return radians * (180f / (float)Math.PI);
	}

	public static Vector3 QuaternionToEulerDeg(this Quaternion q)
	{
		q = Quaternion.Normalize(q);
		float radians = MathF.Atan2(2f * (q.W * q.Y + q.X * q.Z), 1f - 2f * (q.Y * q.Y + q.X * q.X));
		float num = 2f * (q.W * q.X - q.Z * q.Y);
		float radians2 = ((MathF.Abs(num) >= 1f) ? MathF.CopySign((float)Math.PI / 2f, num) : MathF.Asin(num));
		return new Vector3(z: MathF.Atan2(2f * (q.W * q.Z + q.Y * q.X), 1f - 2f * (q.Z * q.Z + q.X * q.X)).RadToDeg(), x: radians2.RadToDeg(), y: radians.RadToDeg());
	}

	public static nint ToLParam(this Vector2 value)
	{
		return ((int)value.Y << 16) | ((int)value.X & 0xFFFF);
	}

	public static LUID ToLUID(this long value)
	{
		return new LUID
		{
			LowPart = (uint)(value & 0xFFFFFFFFu),
			HighPart = (int)(value >> 32)
		};
	}

	public static uint ToU32(this Vector4 value)
	{
		return ImGui.GetColorU32(value);
	}

	public static Vector4 ToVector4(this uint value)
	{
		return ImGui.ColorConvertU32ToFloat4(value);
	}
}
