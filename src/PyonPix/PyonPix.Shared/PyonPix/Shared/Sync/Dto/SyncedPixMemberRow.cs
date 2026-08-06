using System;

namespace PyonPix.Shared.Sync.Dto;

public sealed class SyncedPixMemberRow
{
	public long CharacterId { get; set; }

	public string Alias { get; set; } = string.Empty;

	public string AliasStyle { get; set; } = string.Empty;

	public bool IsSupporter { get; set; }

	public bool IsSubscriber { get; set; }

	public string Rank { get; set; } = "Member";

	public DateTime JoinedTimestamp { get; set; }

	public DateTime LastJoinedTimestamp { get; set; }
}
