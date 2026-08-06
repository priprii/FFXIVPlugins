using System.Text.Json;

namespace PyonPix.Shared.Structs.Browser;

public sealed class WebMessage
{
	public WebMessageType Type { get; set; }

	public JsonElement Payload { get; set; }
}
