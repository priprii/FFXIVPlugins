using System.Collections.Generic;
using PyonPix.Shared.Structs;
using PyonPix.Shared.Sync.Dto.Client;
using PyonPix.Shared.Sync.Dto.Syncable;

namespace PyonPix.Shared.Sync.Dto.Auth;

public record AuthCreateSuccessDto(string SecretKey, PremiumStatus Premium, SyncedCharacterProperties Style, List<SyncablePixQueryItemDto> SyncablePixs);
