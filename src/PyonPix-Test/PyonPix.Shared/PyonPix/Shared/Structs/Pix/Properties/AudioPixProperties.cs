namespace PyonPix.Shared.Structs.Pix.Properties;

public class AudioPixProperties : ILocal<SyncedAudioPixProperties>
{
	public bool SpatialEnabled = true;

	public float Volume = 1f;

	public float FalloffMaxDistance = 25f;

	public float FalloffStrength = 4f;

	public SyncedAudioPixProperties ToSynced()
	{
		return new SyncedAudioPixProperties
		{
			SpatialEnabled = SpatialEnabled,
			Volume = Volume,
			FalloffMaxDistance = FalloffMaxDistance,
			FalloffStrength = FalloffStrength
		};
	}
}
