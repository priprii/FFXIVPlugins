namespace PyonPix.Shared.Structs.Pix.Properties;

public class SyncedAudioPixProperties : ISynced<AudioPixProperties>
{
	public bool SpatialEnabled { get; set; }

	public float Volume { get; set; }

	public float FalloffMaxDistance { get; set; }

	public float FalloffStrength { get; set; }

	public void ApplyTo(AudioPixProperties target)
	{
		target.SpatialEnabled = SpatialEnabled;
		target.Volume = Volume;
		target.FalloffMaxDistance = FalloffMaxDistance;
		target.FalloffStrength = FalloffStrength;
	}
}
