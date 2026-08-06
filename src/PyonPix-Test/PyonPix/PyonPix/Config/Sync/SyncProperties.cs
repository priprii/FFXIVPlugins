using System.Collections.Generic;
using PyonPix.Services.Game;
using PyonPix.Shared.Structs;

namespace PyonPix.Config.Sync;

public class SyncProperties
{
	public bool AutoConnect;

	public string SecretKey = string.Empty;

	public Dictionary<long, CharacterProperties> Characters = new Dictionary<long, CharacterProperties>();

	public CharacterProperties GetCurrentCharacterProperties(Configuration config, StateService state)
	{
		if (!Characters.TryGetValue(state.LocalPlayerContentId, out CharacterProperties value))
		{
			value = new CharacterProperties();
			Characters[state.LocalPlayerContentId] = value;
			config.Save();
		}
		return value;
	}
}
