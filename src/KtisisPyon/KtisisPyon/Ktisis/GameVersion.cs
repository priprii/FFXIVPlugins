using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace Ktisis;

public static class GameVersion
{
	public const string Validated = "2023.11.09.0000.0000";

	public unsafe static string GetCurrent()
	{
		Framework* ptr = Framework.Instance();
		if (ptr == null)
		{
			return string.Empty;
		}
		return ((Framework)ptr).GameVersionString;
	}
}
