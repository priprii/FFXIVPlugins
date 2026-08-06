using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PvPyon.Api.ActivityContexts;

[JsonConverter(typeof(StringEnumConverter))]
public enum ActivityType
{
	None,
	PveDuty,
	PvpDuty
}
