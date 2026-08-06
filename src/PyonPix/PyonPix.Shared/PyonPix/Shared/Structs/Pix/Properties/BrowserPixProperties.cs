using System.Numerics;
using PyonPix.Shared.Structs.Browser;

namespace PyonPix.Shared.Structs.Pix.Properties;

public class BrowserPixProperties : ILocal<SyncedBrowserPixProperties>
{
	public string Uri = string.Empty;

	public BrowserScaleMode ScaleMode;

	public uint CustomScaleWidth = 1920u;

	public uint CustomScaleHeight = 1080u;

	public bool GpuAcceleration = true;

	public Vector2 CustomScale => new Vector2(CustomScaleWidth, CustomScaleHeight);

	public SyncedBrowserPixProperties ToSynced()
	{
		return new SyncedBrowserPixProperties
		{
			Uri = Uri,
			ScaleMode = ScaleMode,
			CustomScaleWidth = CustomScaleWidth,
			CustomScaleHeight = CustomScaleHeight,
			GpuAcceleration = GpuAcceleration
		};
	}
}
