using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ktisis.Data.Json.Converters;

internal class Vector4Converter : JsonConverter<Vector4>
{
	public Vector4Converter(JsonFileSerializer json)
	{
	}

	public override Vector4 Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
	{
		string[] array = (reader.GetString() ?? "").Split(",");
		return new Vector4(float.Parse(array[0], CultureInfo.InvariantCulture), float.Parse(array[1], CultureInfo.InvariantCulture), float.Parse(array[2], CultureInfo.InvariantCulture), float.Parse(array[3], CultureInfo.InvariantCulture));
	}

	public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		InlineArray4<object> buffer = default(InlineArray4<object>);
		buffer[0] = value.X;
		buffer[1] = value.Y;
		buffer[2] = value.Z;
		buffer[3] = value.W;
		writer.WriteStringValue(string.Format((IFormatProvider?)invariantCulture, "{0}, {1}, {2}, {3}", (ReadOnlySpan<object?>)buffer));
	}
}
