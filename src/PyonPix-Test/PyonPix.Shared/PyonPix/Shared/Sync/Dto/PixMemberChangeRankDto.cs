using PyonPix.Shared.Structs.Pix;

namespace PyonPix.Shared.Sync.Dto;

public sealed class PixMemberChangeRankDto
{
	public string PixId { get; set; } = string.Empty;

	public long CharacterId { get; set; }

	public PixRank NewRank { get; set; }
}
