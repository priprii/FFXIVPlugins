using System.Text.RegularExpressions;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Sync.Dto.Client;

namespace PyonPix.Shared.Utility;

public static class NameUtil
{
	public const int AliasMinLength = 2;

	public const int AliasMaxLength = 20;

	public const int PixMinLength = 2;

	public const int PixMaxLength = 32;

	public const int PixDescMaxLength = 192;

	public const int PixPassMinLength = 2;

	public const int PixPassMaxLength = 32;

	public const int PixIdLength = 12;

	public const string PixIdLocalPrefix = "PIX";

	public const string PixIdSyncedPrefix = "PXS";

	private static readonly Regex AliasRegex = new Regex("^(?=.{2,20}$)(?! )[\\S ]+(?<! )$", RegexOptions.Compiled);

	private static readonly Regex PixNameRegex = new Regex("^(?=.{2,32}$)(?! )[\\S ]+(?<! )$", RegexOptions.Compiled);

	private static readonly Regex PixDescRegex = new Regex("^(?=.{1,192}$)(?! )[\\S ]+(?<! )$", RegexOptions.Compiled);

	private static readonly Regex PixPassRegex = new Regex("^(?=.{2,32}$)(?! )[\\S ]+(?<! )$", RegexOptions.Compiled);

	private static readonly Regex ConsecutiveStandardRegex = new Regex("([#*_'\\- ])\\1+", RegexOptions.Compiled);

	private static readonly Regex ConsecutiveSpaceHashRegex = new Regex("([# ])\\1+", RegexOptions.Compiled);

	public static bool ValidateAlias(string alias, PremiumStatus prem, out string? error)
	{
		error = null;
		if (string.IsNullOrWhiteSpace(alias))
		{
			error = "Alias Required";
			return false;
		}
		if (alias.StartsWith(' ') || alias.EndsWith(' '))
		{
			error = "Alias cannot start/end with a space character.";
			return false;
		}
		if (alias.EndsWith('#'))
		{
			error = "Alias cannot end with a hash character.";
			return false;
		}
		if (ConsecutiveSpaceHashRegex.IsMatch(alias))
		{
			error = "Alias has invalid consecutive special characters.";
			return false;
		}
		if (!AliasRegex.IsMatch(alias))
		{
			error = $"Alias must be {2}-{20} characters.";
			return false;
		}
		return true;
	}

	public static bool ValidatePix(string pixName, string? pixDesc, string? pixPass, PixPrivacy privacy, PremiumStatus prem, out string? error)
	{
		error = null;
		if (!ValidatePixName(pixName, prem, out error))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(pixDesc) && !ValidatePixDesc(pixDesc, out error))
		{
			return false;
		}
		if (privacy == PixPrivacy.Private && !ValidatePixPass(pixPass, out error))
		{
			return false;
		}
		return true;
	}

	private static bool ValidatePixName(string pixName, PremiumStatus prem, out string? error)
	{
		error = null;
		if (string.IsNullOrWhiteSpace(pixName))
		{
			error = "Pix Name Required";
			return false;
		}
		if (pixName.StartsWith(' ') || pixName.EndsWith(' '))
		{
			error = "Pix Name cannot start/end with a space character.";
			return false;
		}
		if (pixName.EndsWith('#'))
		{
			error = "Pix Name cannot end with a hash character.";
			return false;
		}
		if (ConsecutiveSpaceHashRegex.IsMatch(pixName))
		{
			error = "Pix Name has invalid consecutive special characters.";
			return false;
		}
		if (!PixNameRegex.IsMatch(pixName))
		{
			error = $"Pix Name must be {2}-{32} characters.";
			return false;
		}
		return true;
	}

	private static bool ValidatePixDesc(string pixDesc, out string? error)
	{
		error = null;
		if (pixDesc.StartsWith(' ') || pixDesc.EndsWith(' '))
		{
			error = "Description cannot start/end with a space character.";
			return false;
		}
		if (pixDesc.EndsWith('#'))
		{
			error = "Description cannot end with a hash character.";
			return false;
		}
		if (ConsecutiveSpaceHashRegex.IsMatch(pixDesc))
		{
			error = "Description has invalid consecutive special characters.";
			return false;
		}
		if (!PixDescRegex.IsMatch(pixDesc))
		{
			error = $"Description must be no more than {192} characters.";
			return false;
		}
		return true;
	}

	private static bool ValidatePixPass(string? pixPass, out string? error)
	{
		error = null;
		if (string.IsNullOrWhiteSpace(pixPass))
		{
			error = "Password required for Private Pix";
			return false;
		}
		if (pixPass.StartsWith(' ') || pixPass.EndsWith(' '))
		{
			error = "Password cannot start/end with a space character.";
			return false;
		}
		if (pixPass.EndsWith('#'))
		{
			error = "Password cannot end with a hash character.";
			return false;
		}
		if (ConsecutiveSpaceHashRegex.IsMatch(pixPass))
		{
			error = "Password has invalid consecutive special characters.";
			return false;
		}
		if (!PixPassRegex.IsMatch(pixPass))
		{
			error = $"Password must be {2}-{32} characters.";
			return false;
		}
		return true;
	}

	public static bool ValidateSyncedPixId(string? pixId, out string? error)
	{
		error = null;
		if (string.IsNullOrWhiteSpace(pixId))
		{
			error = "Pix Id Required";
			return false;
		}
		if (!pixId.StartsWith("PXS"))
		{
			error = "Invalid Pix Id";
			return false;
		}
		if (pixId.Length < 12)
		{
			error = "Invalid Pix Id";
			return false;
		}
		return true;
	}
}
