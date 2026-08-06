using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ktisis.Data.Files;

namespace Ktisis.Data.Json.Converters;

public class JsonFileConverter : JsonConverter<JsonFile>
{
	public override bool CanConvert(Type t)
	{
		return t.BaseType == typeof(JsonFile);
	}

	public override JsonFile? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		JsonFile jsonFile = (JsonFile)Activator.CreateInstance(typeToConvert);
		using JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader);
		PropertyInfo[] properties = typeToConvert.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (!jsonDocument.RootElement.TryGetProperty(propertyInfo.Name, out var value))
			{
				DeserializerDefaultAttribute customAttribute = propertyInfo.GetCustomAttribute<DeserializerDefaultAttribute>();
				if (customAttribute != null)
				{
					propertyInfo.SetValue(jsonFile, customAttribute.Default);
				}
				continue;
			}
			try
			{
				object obj = value.Deserialize(propertyInfo.PropertyType, options);
				if (obj != null)
				{
					propertyInfo.SetValue(jsonFile, obj);
				}
			}
			catch
			{
				Ktisis.Log.Warning($"Failed to parse {propertyInfo.PropertyType.Name} value '{propertyInfo.Name}' (received {value.ValueKind} instead)");
			}
		}
		return jsonFile;
	}

	public override void Write(Utf8JsonWriter writer, JsonFile value, JsonSerializerOptions options)
	{
	}
}
