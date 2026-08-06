using PyonPix.Shared.Structs.Browser;

namespace PyonPix.Shared.Structs.Pix.Properties;

public class SyncedBrowserPixProperties : ISynced<BrowserPixProperties>
{
	public string Uri { get; set; } = string.Empty;

	public BrowserScaleMode ScaleMode { get; set; }

	public uint CustomScaleWidth { get; set; }

	public uint CustomScaleHeight { get; set; }

	public bool GpuAcceleration { get; set; }

	public void ApplyTo(BrowserPixProperties target)
	{
		target.Uri = Uri;
		target.ScaleMode = ScaleMode;
		target.CustomScaleWidth = CustomScaleWidth;
		target.CustomScaleHeight = CustomScaleHeight;
		target.GpuAcceleration = GpuAcceleration;
	}
}
