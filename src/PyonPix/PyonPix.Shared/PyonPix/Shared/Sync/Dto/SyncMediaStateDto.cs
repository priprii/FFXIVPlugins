using PyonPix.Shared.Structs.Browser.WebMessages;

namespace PyonPix.Shared.Sync.Dto;

public record SyncMediaStateDto(string PixId, MediaState? Media)
{
	public string PixId { get; set; } = PixId;

	public MediaState? Media { get; set; } = Media;
}
