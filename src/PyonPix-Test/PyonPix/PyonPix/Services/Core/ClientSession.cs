using System;
using PyonPix.Shared.Structs;
using PyonPix.Shared.Sync.Dto.Client;

namespace PyonPix.Services.Core;

public class ClientSession
{
	public bool IsSecretKeyInvalid;

	public bool IsAuthenticated;

	public PremiumStatus Premium = new PremiumStatus(IsSupporter: false, IsSubscriber: false);

	public CharacterProperties Style = new CharacterProperties();

	public string? AuthKey;

	public DateTime? AuthExpiration;

	public TimeSpan? AuthExpirationTime
	{
		get
		{
			if (!AuthExpiration.HasValue)
			{
				return null;
			}
			return AuthExpiration - DateTime.UtcNow;
		}
	}

	public string GetAuthExpirationTime()
	{
		if (!AuthExpirationTime.HasValue)
		{
			return string.Empty;
		}
		return $"{AuthExpirationTime.Value:mm\\:ss}";
	}
}
