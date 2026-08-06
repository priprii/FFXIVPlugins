using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Structs.Pix.Properties;

namespace PyonPix.Shared.Sync.Dto;

public class SyncedPixUpdateBrowserProperties(string pixId, SyncedBrowserPixProperties browser) : BaseSyncedPixUpdate(pixId)
{
	public override PixUpdateType UpdateType => PixUpdateType.BrowserProperties;

	public SyncedBrowserPixProperties Browser { get; set; } = browser;
}
