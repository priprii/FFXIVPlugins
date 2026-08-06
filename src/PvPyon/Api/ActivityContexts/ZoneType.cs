using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PvPyon.Api.ActivityContexts;

[JsonConverter(typeof(StringEnumConverter))]
public enum ZoneType
{
	Overworld,
	Dungeon,
	Raid,
	AllianceRaid,
	Foray
}
