using System;

namespace TriggerPyon;

public static class IntEx
{
	public static double DegreesToRadians(this int degrees)
	{
		return (double)degrees * (Math.PI / 180.0);
	}

	public static double RadiansToDegrees(this int radians)
	{
		return (double)radians * (180.0 / Math.PI);
	}

	public static float DegreesToRadiansF(this int degrees)
	{
		return (float)degrees * ((float)Math.PI / 180f);
	}

	public static float RadiansToDegreesF(this int radians)
	{
		return (float)radians * (180f / (float)Math.PI);
	}
}
