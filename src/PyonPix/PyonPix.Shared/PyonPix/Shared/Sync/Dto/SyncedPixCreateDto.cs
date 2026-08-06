using PyonPix.Shared.Structs.Pix;

namespace PyonPix.Shared.Sync.Dto;

public sealed class SyncedPixCreateDto
{
	public string RequestId { get; set; } = string.Empty;

	public string LocalPixId { get; set; } = string.Empty;

	public PixDto Pix { get; set; } = new PixDto();

	public SyncedPixMetaDto Meta { get; set; } = new SyncedPixMetaDto();
}
