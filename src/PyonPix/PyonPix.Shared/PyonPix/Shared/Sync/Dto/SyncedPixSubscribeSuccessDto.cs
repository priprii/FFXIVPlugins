using PyonPix.Shared.Sync.Dto.Subbed;

namespace PyonPix.Shared.Sync.Dto;

public sealed class SyncedPixSubscribeSuccessDto
{
	public SubbedPixQueryItemDto? Pix { get; set; }
}
