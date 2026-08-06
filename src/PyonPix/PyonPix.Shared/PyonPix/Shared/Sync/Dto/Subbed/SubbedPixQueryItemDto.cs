using System;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Sync.Dto.Client;

namespace PyonPix.Shared.Sync.Dto.Subbed;

public sealed class SubbedPixQueryItemDto
{
	public string PixId { get; set; } = string.Empty;

	public long OwnerId { get; set; }

	public string OwnerAlias { get; set; } = string.Empty;

	public StyleDto? OwnerAliasStyle { get; set; }

	public StyleDto? OwnerPixStyle { get; set; }

	public PixRank SelfRank { get; set; }

	public SyncedPixMetaDto Meta { get; set; } = new SyncedPixMetaDto();

	public PixDto Pix { get; set; } = new PixDto();

	public DateTime CreatedTimestamp { get; set; }

	public DateTime UpdatedTimestamp { get; set; }
}
