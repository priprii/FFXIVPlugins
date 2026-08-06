using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Structs.Pix.Properties;

namespace PyonPix.Shared.Sync.Dto;

public class SyncedPixUpdateInfoProperties(string pixId, SyncedInfoPixProperties info) : BaseSyncedPixUpdate(pixId)
{
	public override PixUpdateType UpdateType => PixUpdateType.InfoProperties;

	public SyncedInfoPixProperties Info { get; set; } = info;
}
