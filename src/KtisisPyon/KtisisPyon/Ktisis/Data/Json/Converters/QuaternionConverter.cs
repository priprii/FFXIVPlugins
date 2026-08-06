using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ktisis.Data.Json.Converters;

internal class QuaternionConverter : JsonConverter<Quaternion>
{
	public QuaternionConverter(JsonFileSerializer json)
	{
	}

	public override Quaternion Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
	{
		string[] array = (reader.GetString() ?? "").Split(",");
		return new Quaternion(float.Parse(array[0], CultureInfo.InvariantCulture), float.Parse(array[1], CultureInfo.InvariantCulture), float.Parse(array[2], CultureInfo.InvariantCulture), float.Parse(array[3], CultureInfo.InvariantCulture));
	}

	public override void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializerOptions options)
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
