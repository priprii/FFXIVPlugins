using PyonPix.Shared.Sync.Dto.Client;

namespace PyonPix.Shared.Sync.Dto.Subbed;

public record SubbedPixStyleUpdateDto(long OwnerId, string OwnerAlias, StyleDto? OwnerAliasStyle, StyleDto? OwnerPixStyle);
