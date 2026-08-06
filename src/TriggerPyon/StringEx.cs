using System.Linq;

namespace TriggerPyon;

public static class StringEx
{
	public static string GetForename(this string nameWorld)
	{
		if (!nameWorld.Contains(' '))
		{
			return nameWorld;
		}
		return nameWorld.Split(' ')[0];
	}

	public static (string, string?) GetSurnameWorld(this string nameWorld)
	{
		string surname = (nameWorld.Contains(' ') ? nameWorld.Split(' ')[1] : nameWorld);
		string text = string.Empty;
		if (!Plugin.Worlds.Any((string x) => x == surname))
		{
			text = Plugin.Worlds.FirstOrDefault((string x) => surname.EndsWith(x));
			if (text != null)
			{
				surname = surname.Substring(0, surname.Length - text.Length);
			}
		}
		if (string.IsNullOrWhiteSpace(text))
		{
			EntityInfo entityInfo = PlayerManager.NearbyPlayers.FirstOrDefault((EntityInfo x) => x.Name == nameWorld);
			if (entityInfo != null)
			{
				text = entityInfo.HomeWorld;
			}
		}
		return (surname, text);
	}
}
