namespace PyonPix.Shared.Structs.Pix.Properties;

public class SyncedSyncPixProperties : ISynced<SyncPixProperties>
{
	public string? SecretKey { get; set; }

	public PixPrivacy Privacy { get; set; }

	public PixRank EditorRank { get; set; }

	public bool Nsfw { get; set; }

	public void ApplyTo(SyncPixProperties target)
	{
		target.SecretKey = SecretKey;
		target.Privacy = Privacy;
		target.EditorRank = EditorRank;
		target.Nsfw = Nsfw;
	}
}
