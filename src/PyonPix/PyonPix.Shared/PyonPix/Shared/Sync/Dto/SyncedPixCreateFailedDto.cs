namespace PyonPix.Shared.Sync.Dto;

public sealed class SyncedPixCreateFailedDto(string requestId, string reason)
{
	public string RequestId { get; set; } = requestId;

	public string Reason { get; set; } = reason;
}
