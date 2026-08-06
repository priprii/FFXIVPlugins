using PyonPix.Ipc;

namespace PyonPix.Shared.Structs.Browser.WebMessages;

public sealed class MediaState
{
	public MediaStateAction Action { get; set; }

	public bool IsPlaying { get; set; }

	public long SeekTime { get; set; }

	public long Duration { get; set; }

	public long Timestamp { get; set; }
}
