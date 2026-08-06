using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Structs.Pix.Properties;

namespace PyonPix.Shared.Sync.Dto;

public class SyncedPixUpdate(string pixId, SyncedInfoPixProperties? info, SyncedBrowserPixProperties? browser, SyncedRendererPixProperties? renderer, SyncedLightPixProperties? light, SyncedAudioPixProperties? audio, SyncedSyncPixProperties? sync) : BaseSyncedPixUpdate(pixId)
{
	public override PixUpdateType UpdateType => PixUpdateType.All;

	public SyncedInfoPixProperties? Info { get; set; } = info;

	public SyncedBrowserPixProperties? Browser { get; set; } = browser;

	public SyncedRendererPixProperties? Renderer { get; set; } = renderer;

	public SyncedLightPixProperties? Light { get; set; } = light;

	public SyncedAudioPixProperties? Audio { get; set; } = audio;

	public SyncedSyncPixProperties? Sync { get; set; } = sync;
}
