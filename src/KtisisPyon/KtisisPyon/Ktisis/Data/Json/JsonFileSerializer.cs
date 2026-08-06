using System.Text.Json;
using System.Text.Json.Serialization;
using Ktisis.Core.Attributes;
using Ktisis.Data.Json.Converters;

namespace Ktisis.Data.Json;

[Singleton]
public class JsonFileSerializer
{
	private readonly JsonSerializerOptions Options;

	private readonly JsonSerializerOptions DeserializeOptions;

	public JsonFileSerializer()
	{
		Options = new JsonSerializerOptions
		{
			WriteIndented = true,
			PropertyNameCaseInsensitive = false,
			AllowTrailingCommas = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			IncludeFields = true
		};
		Options.Converters.Add(new JsonStringEnumConverter());
		Options.Converters.Add(new QuaternionConverter(this));
		Options.Converters.Add(new Vector3Converter(this));
		Options.Converters.Add(new Vector4Converter(this));
		Options.Converters.Add(new TransformConverter(this));
		DeserializeOptions = new JsonSerializerOptions(Options);
		DeserializeOptions.Converters.Add(new JsonFileConverter());
	}

	public JsonConverter<T> GetConverter<T>()
	{
		return (JsonConverter<T>)Options.GetConverter(typeof(T));
	}

	public string Serialize(object obj)
	{
		return JsonSerializer.Serialize(obj, Options);
	}

	public T? Deserialize<T>(string json) where T : notnull
	{
		return JsonSerializer.Deserialize<T>(json, DeserializeOptions);
	}
}
