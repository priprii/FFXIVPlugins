using System;
using PyonPix.Shared.Structs.Pix;

namespace PyonPix.Shared.Sync.Dto.Subbed;

public sealed class SubbedPixQueryItemRow
{
	public string PixId { get; set; } = string.Empty;

	public long OwnerId { get; set; }

	public bool OwnerIsSubscriber { get; set; }

	public string OwnerAlias { get; set; } = string.Empty;

	public string OwnerAliasStyle { get; set; } = string.Empty;

	public string OwnerPixStyle { get; set; } = string.Empty;

	public string SelfRank { get; set; } = $"{PixRank.Member}";

	public string Name { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public string PixType { get; set; } = $"{PyonPix.Shared.Structs.Pix.PixType.Video}";

	public string Privacy { get; set; } = $"{PixPrivacy.Public}";

	public string EditorRank { get; set; } = $"{PixRank.Owner}";

	public string? SecretKey { get; set; }

	public bool Nsfw { get; set; }

	public short WorldId { get; set; }

	public short TerritoryId { get; set; }

	public short Ward { get; set; }

	public short Plot { get; set; }

	public short Room { get; set; }

	public short Floor { get; set; }

	public bool Persistent { get; set; }

	public string Data { get; set; } = string.Empty;

	public DateTime CreatedTimestamp { get; set; }

	public DateTime UpdatedTimestamp { get; set; }
}
