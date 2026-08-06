using System.Text.Json;

namespace PyonPix.Shared.Sync;

public class SocketMessage(MessageType type, JsonElement? data)
{
	public MessageType Type { get; set; } = type;

	public JsonElement? Data { get; set; } = data;
}
