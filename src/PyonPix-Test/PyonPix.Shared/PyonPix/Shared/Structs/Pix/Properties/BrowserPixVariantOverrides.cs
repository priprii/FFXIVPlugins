using System;
using System.Text.Json.Serialization;
using PyonPix.Shared.Structs.Browser;

namespace PyonPix.Shared.Structs.Pix.Properties;

[Serializable]
public class BrowserPixVariantOverrides
{
	public BrowserScaleMode? ScaleMode;

	public uint? CustomScaleWidth;

	public uint? CustomScaleHeight;

	public bool? GpuAcceleration;

	[JsonIgnore]
	public bool HasAny
	{
		get
		{
			if (!ScaleMode.HasValue && !CustomScaleWidth.HasValue && !CustomScaleHeight.HasValue)
			{
				return GpuAcceleration.HasValue;
			}
			return true;
		}
	}

	public void ApplyTo(BrowserPixProperties target)
	{
		if (ScaleMode.HasValue)
		{
			target.ScaleMode = ScaleMode.Value;
		}
		if (CustomScaleWidth.HasValue)
		{
			target.CustomScaleWidth = CustomScaleWidth.Value;
		}
		if (CustomScaleHeight.HasValue)
		{
			target.CustomScaleHeight = CustomScaleHeight.Value;
		}
		if (GpuAcceleration.HasValue)
		{
			target.GpuAcceleration = GpuAcceleration.Value;
		}
	}
}
