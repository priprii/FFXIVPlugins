using PyonPix.Shared.Structs.Light;
using PyonPix.Shared.Utility;

namespace PyonPix.Shared.Structs.Pix.Properties;

public class SyncedLightPixProperties : ISynced<LightPixProperties>
{
	public bool Enabled { get; set; }

	public LightFlags Flags { get; set; }

	public LightType LightType { get; set; }

	public MathUtil.SyncedVector3 Position { get; set; }

	public MathUtil.SyncedQuaternion Rotation { get; set; }

	public MathUtil.SyncedVector4 Colour { get; set; }

	public float Intensity { get; set; }

	public float ScreenColourInfluence { get; set; }

	public float InfluenceColourIntensity { get; set; }

	public float InfluenceBrightnessIntensity { get; set; }

	public float InfluenceGammaCurve { get; set; }

	public float Range { get; set; }

	public float LightAngle { get; set; }

	public FalloffType FalloffType { get; set; }

	public float FalloffAngle { get; set; }

	public float FalloffPower { get; set; }

	public float ShadowRange { get; set; }

	public float ShadowNear { get; set; }

	public float ShadowFar { get; set; }

	public void ApplyTo(LightPixProperties target)
	{
		target.Enabled = Enabled;
		target.Flags = Flags;
		target.LightType = LightType;
		target.Position = Position.ToLocal();
		target.Rotation = Rotation.ToLocal();
		target.Colour = Colour.ToLocal();
		target.Intensity = Intensity;
		target.ScreenColourInfluence = ScreenColourInfluence;
		target.InfluenceColourIntensity = InfluenceColourIntensity;
		target.InfluenceBrightnessIntensity = InfluenceBrightnessIntensity;
		target.InfluenceGammaCurve = InfluenceGammaCurve;
		target.Range = Range;
		target.LightAngle = LightAngle;
		target.FalloffType = FalloffType;
		target.FalloffAngle = FalloffAngle;
		target.FalloffPower = FalloffPower;
		target.ShadowRange = ShadowRange;
		target.ShadowNear = ShadowNear;
		target.ShadowFar = ShadowFar;
	}
}
