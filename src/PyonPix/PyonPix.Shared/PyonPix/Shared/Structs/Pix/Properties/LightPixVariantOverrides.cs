using System;
using System.Numerics;
using System.Text.Json.Serialization;
using PyonPix.Shared.Structs.Light;

namespace PyonPix.Shared.Structs.Pix.Properties;

[Serializable]
public class LightPixVariantOverrides
{
	public bool? Enabled;

	public LightFlags? Flags;

	public LightType? LightType;

	public Vector3? Position;

	public Quaternion? Rotation;

	public Vector4? Colour;

	public float? Intensity;

	public float? ScreenColourInfluence;

	public float? InfluenceColourIntensity;

	public float? InfluenceBrightnessIntensity;

	public float? InfluenceGammaCurve;

	public float? Range;

	public float? LightAngle;

	public FalloffType? FalloffType;

	public float? FalloffAngle;

	public float? FalloffPower;

	public float? ShadowRange;

	public float? ShadowNear;

	public float? ShadowFar;

	[JsonIgnore]
	public bool HasAny
	{
		get
		{
			if (!Enabled.HasValue && !Flags.HasValue && !LightType.HasValue && !Position.HasValue && !Rotation.HasValue && !Colour.HasValue && !Intensity.HasValue && !ScreenColourInfluence.HasValue && !InfluenceColourIntensity.HasValue && !InfluenceBrightnessIntensity.HasValue && !InfluenceGammaCurve.HasValue && !Range.HasValue && !LightAngle.HasValue && !FalloffType.HasValue && !FalloffAngle.HasValue && !FalloffPower.HasValue && !ShadowRange.HasValue && !ShadowNear.HasValue)
			{
				return ShadowFar.HasValue;
			}
			return true;
		}
	}

	public void ApplyTo(LightPixProperties target)
	{
		if (Enabled.HasValue)
		{
			target.Enabled = Enabled.Value;
		}
		if (Flags.HasValue)
		{
			target.Flags = Flags.Value;
		}
		if (LightType.HasValue)
		{
			target.LightType = LightType.Value;
		}
		if (Position.HasValue)
		{
			target.Position = Position.Value;
		}
		if (Rotation.HasValue)
		{
			target.Rotation = Rotation.Value;
		}
		if (Colour.HasValue)
		{
			target.Colour = Colour.Value;
		}
		if (Intensity.HasValue)
		{
			target.Intensity = Intensity.Value;
		}
		if (ScreenColourInfluence.HasValue)
		{
			target.ScreenColourInfluence = ScreenColourInfluence.Value;
		}
		if (InfluenceColourIntensity.HasValue)
		{
			target.InfluenceColourIntensity = InfluenceColourIntensity.Value;
		}
		if (InfluenceBrightnessIntensity.HasValue)
		{
			target.InfluenceBrightnessIntensity = InfluenceBrightnessIntensity.Value;
		}
		if (InfluenceGammaCurve.HasValue)
		{
			target.InfluenceGammaCurve = InfluenceGammaCurve.Value;
		}
		if (Range.HasValue)
		{
			target.Range = Range.Value;
		}
		if (LightAngle.HasValue)
		{
			target.LightAngle = LightAngle.Value;
		}
		if (FalloffType.HasValue)
		{
			target.FalloffType = FalloffType.Value;
		}
		if (FalloffAngle.HasValue)
		{
			target.FalloffAngle = FalloffAngle.Value;
		}
		if (FalloffPower.HasValue)
		{
			target.FalloffPower = FalloffPower.Value;
		}
		if (ShadowRange.HasValue)
		{
			target.ShadowRange = ShadowRange.Value;
		}
		if (ShadowNear.HasValue)
		{
			target.ShadowNear = ShadowNear.Value;
		}
		if (ShadowFar.HasValue)
		{
			target.ShadowFar = ShadowFar.Value;
		}
	}
}
