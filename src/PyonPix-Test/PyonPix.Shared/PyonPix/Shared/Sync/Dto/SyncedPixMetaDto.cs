using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Structs.Pix.Properties;

namespace PyonPix.Shared.Sync.Dto;

public sealed class SyncedPixMetaDto
{
	public string Name { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public PixType PixType { get; set; }

	public PixPrivacy Privacy { get; set; }

	public PixRank EditorRank { get; set; }

	public string? SecretKey { get; set; }

	public bool Nsfw { get; set; }

	public SyncedTerritoryPixProperties Territory { get; set; } = new SyncedTerritoryPixProperties();

	public void ApplyTo(InfoPixProperties targetInfo, SyncPixProperties targetSync)
	{
		targetInfo.Name = Name;
		targetInfo.Description = Description;
		targetInfo.Type = PixType;
		targetSync.SecretKey = SecretKey;
		targetSync.Privacy = Privacy;
		targetSync.EditorRank = EditorRank;
		targetSync.Nsfw = Nsfw;
	}
}
