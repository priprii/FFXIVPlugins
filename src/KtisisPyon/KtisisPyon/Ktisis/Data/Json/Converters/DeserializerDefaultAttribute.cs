using System;

namespace Ktisis.Data.Json.Converters;

public class DeserializerDefaultAttribute(object value) : Attribute
{
	public readonly object Default = value;
}
