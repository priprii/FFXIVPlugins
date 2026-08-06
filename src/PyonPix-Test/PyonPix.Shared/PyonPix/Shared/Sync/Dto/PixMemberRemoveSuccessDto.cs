namespace PyonPix.Shared.Sync.Dto;

public sealed class PixMemberRemoveSuccessDto
{
	public string PixId { get; set; } = string.Empty;

	public long CharacterId { get; set; }
}
