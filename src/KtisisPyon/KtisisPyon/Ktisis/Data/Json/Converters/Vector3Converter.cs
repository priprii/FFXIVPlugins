using System;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ktisis.Data.Json.Converters;

internal class Vector3Converter : JsonConverter<Vector3>
{
	public Vector3Converter(JsonFileSerializer json)
	{
	}

	public override Vector3 Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
	{
		string[] array = (reader.GetString() ?? "").Split(",");
		return new Vector3(float.Parse(array[0], CultureInfo.InvariantCulture), float.Parse(array[1], CultureInfo.InvariantCulture), float.Parse(array[2], CultureInfo.InvariantCulture));
	}

	public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}", value.X, value.Y, value.Z));
	}
}
