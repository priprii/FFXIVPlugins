namespace PyonPix.Shared.Sync.Dto.Session;

public class ServerSessionDto(uint userCount, uint pixCount)
{
	public uint UserCount { get; set; } = userCount;

	public uint PixCount { get; set; } = pixCount;
}
