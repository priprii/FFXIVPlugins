namespace PyonPix.Shared.Sync.Dto;

public sealed class SyncedPixJoinDto
{
	public string PixId { get; set; } = string.Empty;

	public string? SecretKey { get; set; }
}
