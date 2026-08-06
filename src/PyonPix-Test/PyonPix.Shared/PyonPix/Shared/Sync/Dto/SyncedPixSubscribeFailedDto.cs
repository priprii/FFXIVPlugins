namespace PyonPix.Shared.Sync.Dto;

public sealed class SyncedPixSubscribeFailedDto
{
	public string PixId { get; set; } = string.Empty;

	public string Reason { get; set; } = string.Empty;
}
