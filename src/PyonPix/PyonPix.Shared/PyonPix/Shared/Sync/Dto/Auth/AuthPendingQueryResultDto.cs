using System;

namespace PyonPix.Shared.Sync.Dto.Auth;

public record AuthPendingQueryResultDto(long CharacterId, DateTime ExpirationTimestamp);
