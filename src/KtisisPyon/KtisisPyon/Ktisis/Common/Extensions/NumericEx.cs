using System;

namespace Ktisis.Common.Extensions;

public static class NumericEx
{
	public static byte GetAlpha(this uint rgba)
	{
		return (byte)(rgba & 0xFF000000u);
	}

	public static uint SetAlpha(this uint rgba, byte alpha)
	{
		return (rgba & 0xFFFFFF) | (uint)(alpha << 24);
	}

	public static uint SetAlpha(this uint rgba, float alpha)
	{
		return rgba.SetAlpha((byte)Math.Floor(alpha * 255f));
	}

	public static uint FlipEndian(this uint value)
	{
		return ((value & 0xFF000000u) >> 24) | ((value & 0xFF0000) >> 16 << 8) | ((value & 0xFF00) >> 8 << 16) | ((value & 0xFF) << 24);
	}
}
