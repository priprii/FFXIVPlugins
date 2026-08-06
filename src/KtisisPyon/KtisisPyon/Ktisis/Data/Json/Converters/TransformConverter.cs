using System;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ktisis.Common.Utility;

namespace Ktisis.Data.Json.Converters;

internal class TransformConverter(JsonFileSerializer json) : JsonConverter<Transform>
{
	public override Transform Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
	{
		Transform transform = new Transform();
		reader.Read();
		for (int i = 0; i < 4; i++)
		{
			if (reader.TokenType == JsonTokenType.EndObject)
			{
				break;
			}
			string text = reader.GetString();
			reader.Read();
			if (!(text == "BoneDepth"))
			{
				if (text == "Rotation")
				{
					transform.Rotation = ((QuaternionConverter)json.GetConverter<Quaternion>()).Read(ref reader, type, options);
				}
				else
				{
					Vector3 vector = ((Vector3Converter)json.GetConverter<Vector3>()).Read(ref reader, type, options);
					if (text == "Position")
					{
						transform.Position = vector;
					}
					else if (text == "Scale")
					{
						transform.Scale = vector;
					}
				}
			}
			reader.Read();
		}
		return transform;
	}

	public override void Write(Utf8JsonWriter writer, Transform value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		FieldInfo[] fields = typeof(Transform).GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			writer.WritePropertyName(fieldInfo.Name);
			object value2 = fieldInfo.GetValue(value);
			if (value2 is Vector3)
			{
				((Vector3Converter)json.GetConverter<Vector3>()).Write(writer, (Vector3)value2, options);
			}
			else if (value2 is Quaternion)
			{
				((QuaternionConverter)json.GetConverter<Quaternion>()).Write(writer, (Quaternion)value2, options);
			}
		}
		writer.WriteEndObject();
	}
}
