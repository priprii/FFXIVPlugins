namespace PyonPix.Shared.Sync.Dto;

public sealed class SyncedPixCreateSuccessDto(string requestId, string pixId, string? secretKey, int version)
{
	public string RequestId { get; set; } = requestId;

	public string PixId { get; set; } = pixId;

	public string? SecretKey { get; set; } = secretKey;

	public int Version { get; set; } = version;
}
