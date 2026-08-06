using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TriggerPyon;

public class ReactionBaseConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(ReactionBase);
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
		ReactionBase reactionBase = result switch
		{
			2u => new TextReaction(), 
			1u => new EmoteReaction(), 
			_ => null, 
		};
		if (reactionBase != null)
		{
			serializer.Populate(jObject.CreateReader(), reactionBase);
		}
		return reactionBase;
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
