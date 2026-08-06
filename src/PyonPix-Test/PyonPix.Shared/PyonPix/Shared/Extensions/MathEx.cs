using System;
using System.Drawing;
using System.Numerics;

namespace PyonPix.Shared.Extensions;

public static class MathEx
{
	public static Vector3 ToVector3(this Color c)
	{
		return new Vector3((float)(int)c.R / 255f, (float)(int)c.G / 255f, (float)(int)c.B / 255f);
	}

	public static Color ToColor(this Vector3 v)
	{
		return Color.FromArgb(v.X.ToByte(), v.Y.ToByte(), v.Z.ToByte());
	}

	public static int ToByte(this float v)
	{
		return Math.Clamp((int)MathF.Round(v * 255f), 0, 255);
	}
}
