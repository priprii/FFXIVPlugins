using PyonPix.Shared.Structs.Pix.Properties;

namespace PyonPix.Shared.Structs.Pix;

public class PixDto
{
	public int Version { get; set; }

	public SyncedBrowserPixProperties Browser { get; set; } = new SyncedBrowserPixProperties();

	public SyncedRendererPixProperties Renderer { get; set; } = new SyncedRendererPixProperties();

	public SyncedLightPixProperties Light { get; set; } = new SyncedLightPixProperties();

	public SyncedAudioPixProperties Audio { get; set; } = new SyncedAudioPixProperties();
}
