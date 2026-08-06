using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Structs.Pix.Properties;

namespace PyonPix.Shared.Sync.Dto;

public class SyncedPixUpdateLightProperties(string pixId, SyncedLightPixProperties light) : BaseSyncedPixUpdate(pixId)
{
	public override PixUpdateType UpdateType => PixUpdateType.LightProperties;

	public SyncedLightPixProperties Light { get; set; } = light;
}
