using System;

namespace PyonPix.Shared.Sync.Dto.Auth;

public record AuthLoginQueryResultDto(long DiscordId, bool IsSupporter, bool IsSubscriber, DateTime? SessionTimestamp, string Alias, string? AliasStyle, string? PixStyle);
