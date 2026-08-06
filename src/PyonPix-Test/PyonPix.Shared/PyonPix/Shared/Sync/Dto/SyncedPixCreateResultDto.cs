namespace PyonPix.Shared.Sync.Dto;

public class SyncedPixCreateResultDto
{
	public string PixId { get; set; } = string.Empty;

	public string? SecretKey { get; set; }

	public int Version { get; set; }
}
