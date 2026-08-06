namespace PyonPix.Shared.Sync.Dto;

public sealed class PixMemberRemoveDto
{
	public string PixId { get; set; } = string.Empty;

	public long CharacterId { get; set; }
}
