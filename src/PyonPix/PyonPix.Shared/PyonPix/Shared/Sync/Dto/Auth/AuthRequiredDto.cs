using System;

namespace PyonPix.Shared.Sync.Dto.Auth;

public class AuthRequiredDto(string secretKey, DateTime expirationTimestamp)
{
	public string SecretKey { get; set; } = secretKey;

	public DateTime ExpirationTimestamp { get; set; } = expirationTimestamp;
}
