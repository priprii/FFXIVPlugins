using PyonPix.Shared.Structs.Pix;

namespace PyonPix.Shared.Sync.Dto;

public class SyncedPixUpdateUri(string pixId, string uri) : BaseSyncedPixUpdate(pixId)
{
	public override PixUpdateType UpdateType => PixUpdateType.Uri;

	public string Uri { get; set; } = uri;
}
