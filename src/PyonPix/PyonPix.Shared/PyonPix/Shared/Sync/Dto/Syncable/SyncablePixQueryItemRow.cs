using System;
using PyonPix.Shared.Structs.Pix;

namespace PyonPix.Shared.Sync.Dto.Syncable;

public class SyncablePixQueryItemRow
{
	public string PixId { get; set; } = string.Empty;

	public long OwnerId { get; set; }

	public bool OwnerIsSubscriber { get; set; }

	public string OwnerAlias { get; set; } = string.Empty;

	public string OwnerAliasStyle { get; set; } = string.Empty;

	public string OwnerPixStyle { get; set; } = string.Empty;

	public string Name { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public string PixType { get; set; } = $"{PyonPix.Shared.Structs.Pix.PixType.Video}";

	public string Privacy { get; set; } = $"{PixPrivacy.Public}";

	public string EditorRank { get; set; } = $"{PixRank.Owner}";

	public bool Nsfw { get; set; }

	public short WorldId { get; set; }

	public short TerritoryId { get; set; }

	public short Ward { get; set; }

	public short Plot { get; set; }

	public short Room { get; set; }

	public short Floor { get; set; }

	public bool Persistent { get; set; }

	public string Data { get; set; } = string.Empty;

	public DateTime UpdatedTimestamp { get; set; }
}
