namespace PyonPix.Shared.Structs.Pix.Properties;

public class SyncPixProperties : ILocal<SyncedSyncPixProperties>
{
	public bool IsSynced;

	public string? SyncedPixId;

	public string? SecretKey;

	public PixPrivacy Privacy = PixPrivacy.Private;

	public PixRank EditorRank;

	public bool Nsfw;

	public SyncedSyncPixProperties ToSynced()
	{
		return new SyncedSyncPixProperties
		{
			SecretKey = ((Privacy == PixPrivacy.Private) ? SecretKey : null),
			Privacy = Privacy,
			EditorRank = EditorRank,
			Nsfw = Nsfw
		};
	}
}
