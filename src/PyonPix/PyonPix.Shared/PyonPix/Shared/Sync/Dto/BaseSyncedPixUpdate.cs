using PyonPix.Shared.Structs.Pix;

namespace PyonPix.Shared.Sync.Dto;

public abstract class BaseSyncedPixUpdate(string pixId)
{
	public string PixId { get; set; } = pixId;

	public abstract PixUpdateType UpdateType { get; }
}
