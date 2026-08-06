using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TriggerPyon;

public class ActionBaseConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(ActionBase);
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
		ActionBase actionBase = result switch
		{
			2u => new TextAction(), 
			1u => new EmoteAction(), 
			3u => new DiscordAction(), 
			_ => null, 
		};
		if (actionBase != null)
		{
			serializer.Populate(jObject.CreateReader(), actionBase);
		}
		return actionBase;
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
