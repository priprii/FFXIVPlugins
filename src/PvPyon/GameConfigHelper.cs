using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace PvPyon;

public class GameConfigHelper
{
	private static GameConfigHelper instance = null;

	private unsafe static ConfigModule* configModule = null;

	public static GameConfigHelper Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new GameConfigHelper();
			}
			return instance;
		}
	}

	private unsafe GameConfigHelper()
	{
		configModule = ConfigModule.Instance();
	}

	private uint? GetIntValue(ConfigOption option)
	{
		uint value = default(uint);
		if (PluginServices.GameConfig.UiConfig.TryGetUInt("LogNameType", ref value))
		{
			return value;
		}
		return null;
	}

	public LogNameType? GetLogNameType()
	{
		uint? intValue = GetIntValue((ConfigOption)606);
		if (intValue.HasValue)
		{
			return (LogNameType)intValue.Value;
		}
		return null;
	}
}
