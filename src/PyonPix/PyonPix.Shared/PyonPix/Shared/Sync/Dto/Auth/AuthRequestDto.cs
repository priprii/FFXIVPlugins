using System;
using PyonPix.Shared.Sync.Dto.Client;

namespace PyonPix.Shared.Sync.Dto.Auth;

public record AuthRequestDto(Version Version, long CharacterId, string SecretKey, TerritoryDto Territory);
