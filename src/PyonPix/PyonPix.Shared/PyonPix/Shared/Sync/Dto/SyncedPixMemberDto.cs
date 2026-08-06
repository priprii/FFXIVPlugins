using System;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Sync.Dto.Client;

namespace PyonPix.Shared.Sync.Dto;

public sealed class SyncedPixMemberDto
{
	public long CharacterId { get; set; }

	public string Alias { get; set; } = string.Empty;

	public StyleDto? AliasStyle { get; set; }

	public PremiumStatus Premium { get; set; } = new PremiumStatus(IsSupporter: false, IsSubscriber: false);

	public PixRank Rank { get; set; }

	public DateTime JoinedTimestamp { get; set; }

	public DateTime LastJoinedTimestamp { get; set; }

	public SyncedPixMemberState State { get; set; }
}
