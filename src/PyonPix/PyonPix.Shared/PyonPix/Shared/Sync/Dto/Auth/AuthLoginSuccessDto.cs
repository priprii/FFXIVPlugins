using System.Collections.Generic;
using PyonPix.Shared.Structs;
using PyonPix.Shared.Sync.Dto.Client;
using PyonPix.Shared.Sync.Dto.Session;
using PyonPix.Shared.Sync.Dto.Subbed;
using PyonPix.Shared.Sync.Dto.Syncable;

namespace PyonPix.Shared.Sync.Dto.Auth;

public record AuthLoginSuccessDto(ServerSessionDto ServerSession, PremiumStatus Premium, SyncedCharacterProperties Style, List<SyncablePixQueryItemDto> SyncablePixs, List<SubbedPixQueryItemDto> SubbedPixs);
