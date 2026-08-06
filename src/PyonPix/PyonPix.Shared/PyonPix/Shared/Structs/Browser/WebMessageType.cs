using System.Text.Json.Serialization;

namespace PyonPix.Shared.Structs.Browser;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WebMessageType
{
	MediaState,
	MediaReady,
	MediaResync,
	Navigate
}
