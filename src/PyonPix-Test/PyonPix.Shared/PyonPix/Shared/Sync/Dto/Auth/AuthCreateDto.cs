namespace PyonPix.Shared.Sync.Dto.Auth;

public class AuthCreateDto(long characterId, string secretKey)
{
	public long CharacterId { get; set; } = characterId;

	public string SecretKey { get; set; } = secretKey;
}
