using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Structs.Pix.Properties;

namespace PyonPix.Shared.Sync.Dto;

public class SyncedPixUpdateAudioProperties(string pixId, SyncedAudioPixProperties audio) : BaseSyncedPixUpdate(pixId)
{
	public override PixUpdateType UpdateType => PixUpdateType.AudioProperties;

	public SyncedAudioPixProperties Audio { get; set; } = audio;
}
