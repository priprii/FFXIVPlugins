using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TriggerPyon;

public class ReceiverBaseConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(ReceiverBase);
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		JObject jObject = JObject.Load(reader);
		JToken jToken = jObject["ObjType"];
		if (jToken == null || !uint.TryParse(jToken.ToString(), out var result))
		{
			return null;
		}
		ReceiverBase receiverBase = result switch
		{
			2u => new ChannelTextReceiver(), 
			1u => new EmoteTargetReceiver(), 
			_ => null, 
		};
		if (receiverBase != null)
		{
			serializer.Populate(jObject.CreateReader(), receiverBase);
		}
		return receiverBase;
	}

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
		}
		else
		{
			JObject.FromObject(value, serializer).WriteTo(writer);
		}
	}
}
