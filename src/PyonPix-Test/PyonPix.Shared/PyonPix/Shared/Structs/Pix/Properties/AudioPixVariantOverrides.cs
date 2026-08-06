using System;
using System.Text.Json.Serialization;

namespace PyonPix.Shared.Structs.Pix.Properties;

[Serializable]
public class AudioPixVariantOverrides
{
	public bool? SpatialEnabled;

	public float? Volume;

	public float? FalloffMaxDistance;

	public float? FalloffStrength;

	[JsonIgnore]
	public bool HasAny
	{
		get
		{
			if (!SpatialEnabled.HasValue && !Volume.HasValue && !FalloffMaxDistance.HasValue)
			{
				return FalloffStrength.HasValue;
			}
			return true;
		}
	}

	public void ApplyTo(AudioPixProperties target)
	{
		if (SpatialEnabled.HasValue)
		{
			target.SpatialEnabled = SpatialEnabled.Value;
		}
		if (Volume.HasValue)
		{
			target.Volume = Volume.Value;
		}
		if (FalloffMaxDistance.HasValue)
		{
			target.FalloffMaxDistance = FalloffMaxDistance.Value;
		}
		if (FalloffStrength.HasValue)
		{
			target.FalloffStrength = FalloffStrength.Value;
		}
	}
}
