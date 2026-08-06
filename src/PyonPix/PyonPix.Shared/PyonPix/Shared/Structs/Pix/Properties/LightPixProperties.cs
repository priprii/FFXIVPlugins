using System.Numerics;
using PyonPix.Shared.Structs.Light;
using PyonPix.Shared.Utility;

namespace PyonPix.Shared.Structs.Pix.Properties;

public class LightPixProperties : ILocal<SyncedLightPixProperties>
{
	public bool Enabled = true;

	public LightFlags Flags = LightFlags.Reflections | LightFlags.DynamicShadows | LightFlags.CharacterShadows | LightFlags.ObjectShadows;

	public LightType LightType = LightType.PointLight;

	public Vector3 Position;

	public Quaternion Rotation;

	public Vector4 Colour = Vector4.One;

	public float Intensity = 1f;

	public float ScreenColourInfluence = 1f;

	public float InfluenceColourIntensity = 2f;

	public float InfluenceBrightnessIntensity = 1f;

	public float InfluenceGammaCurve = 0.5f;

	public float Range = 5f;

	public float LightAngle = 180f;

	public FalloffType FalloffType = FalloffType.Quadratic;

	public float FalloffAngle = 2f;

	public float FalloffPower = 0.3f;

	public float ShadowRange = 10f;

	public float ShadowNear;

	public float ShadowFar = 10f;

	public SyncedLightPixProperties ToSynced()
	{
		return new SyncedLightPixProperties
		{
			Enabled = Enabled,
			Flags = Flags,
			LightType = LightType,
			Position = Position.ToSynced(),
			Rotation = Rotation.ToSynced(),
			Colour = Colour.ToSynced(),
			Intensity = Intensity,
			ScreenColourInfluence = ScreenColourInfluence,
			InfluenceColourIntensity = InfluenceColourIntensity,
			InfluenceBrightnessIntensity = InfluenceBrightnessIntensity,
			InfluenceGammaCurve = InfluenceGammaCurve,
			Range = Range,
			LightAngle = LightAngle,
			FalloffType = FalloffType,
			FalloffAngle = FalloffAngle,
			FalloffPower = FalloffPower,
			ShadowRange = ShadowRange,
			ShadowNear = ShadowNear,
			ShadowFar = ShadowFar
		};
	}
}
