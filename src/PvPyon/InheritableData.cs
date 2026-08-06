using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PvPyon;

[Serializable]
public struct InheritableData
{
	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("Behavior")]
	public InheritableBehavior Behavior;

	[JsonProperty("Value")]
	public object Value;
}
