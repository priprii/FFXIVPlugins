using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TriggerPyon;

public class CounterBaseConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(CounterBase);
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
		CounterBase counterBase = result switch
		{
			1u => new Counter(), 
			2u => new SharedCounter(), 
			3u => new DiscordCounter(), 
			_ => null, 
		};
		if (counterBase != null)
		{
			serializer.Populate(jObject.CreateReader(), counterBase);
		}
		return counterBase;
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
