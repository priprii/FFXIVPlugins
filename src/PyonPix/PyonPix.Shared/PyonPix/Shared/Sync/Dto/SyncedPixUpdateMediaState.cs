using PyonPix.Shared.Structs.Browser.WebMessages;
using PyonPix.Shared.Structs.Pix;

namespace PyonPix.Shared.Sync.Dto;

public class SyncedPixUpdateMediaState(string pixId, MediaState? media) : BaseSyncedPixUpdate(pixId)
{
	public override PixUpdateType UpdateType => PixUpdateType.MediaState;

	public MediaState? Media { get; set; } = media;
}
