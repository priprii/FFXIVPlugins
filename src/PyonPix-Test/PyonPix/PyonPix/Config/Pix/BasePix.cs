using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Structs.Pix.Properties;
using PyonPix.Shared.Sync.Dto;

namespace PyonPix.Config.Pix;

public class BasePix : IPix
{
	public int Version { get; set; } = 1;

	public string Id { get; set; } = string.Empty;

	public InfoPixProperties Info { get; set; } = new InfoPixProperties();

	public BrowserPixProperties Browser { get; set; } = new BrowserPixProperties();

	public TerritoryPixProperties Territory { get; set; } = new TerritoryPixProperties();

	public RendererPixProperties Renderer { get; set; } = new RendererPixProperties();

	public LightPixProperties Light { get; set; } = new LightPixProperties();

	public AudioPixProperties Audio { get; set; } = new AudioPixProperties();

	public SyncPixProperties Sync { get; set; } = new SyncPixProperties();

	public string GetDisplayName()
	{
		if (!string.IsNullOrWhiteSpace(Info.Name))
		{
			return Info.Name;
		}
		return Id;
	}

	public SyncedPixMetaDto GetSyncedMetaData()
	{
		return new SyncedPixMetaDto
		{
			Name = Info.Name,
			Description = Info.Description,
			PixType = Info.Type,
			Privacy = Sync.Privacy,
			EditorRank = Sync.EditorRank,
			SecretKey = Sync.SecretKey,
			Nsfw = Sync.Nsfw,
			Territory = Territory.ToSynced()
		};
	}
}
