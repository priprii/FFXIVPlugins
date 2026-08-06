using System;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Structs.Pix.Properties;
using PyonPix.Shared.Sync.Dto.Client;

namespace PyonPix.Shared.Sync.Dto.Syncable;

public class SyncablePixQueryItemDto
{
	public string PixId { get; set; } = string.Empty;

	public long OwnerId { get; set; }

	public string OwnerAlias { get; set; } = string.Empty;

	public StyleDto? OwnerAliasStyle { get; set; }

	public StyleDto? OwnerPixStyle { get; set; }

	public string Name { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public string Uri { get; set; } = string.Empty;

	public PixType PixType { get; set; }

	public PixPrivacy Privacy { get; set; }

	public PixRank EditorRank { get; set; }

	public bool Nsfw { get; set; }

	public SyncedTerritoryPixProperties Territory { get; set; } = new SyncedTerritoryPixProperties();

	public DateTime UpdatedTimestamp { get; set; }
}
