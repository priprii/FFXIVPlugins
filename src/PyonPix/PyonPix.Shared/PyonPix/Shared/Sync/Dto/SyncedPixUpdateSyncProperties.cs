using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Structs.Pix.Properties;

namespace PyonPix.Shared.Sync.Dto;

public class SyncedPixUpdateSyncProperties(string pixId, SyncedSyncPixProperties sync) : BaseSyncedPixUpdate(pixId)
{
	public override PixUpdateType UpdateType => PixUpdateType.SyncProperties;

	public SyncedSyncPixProperties Sync { get; set; } = sync;
}
