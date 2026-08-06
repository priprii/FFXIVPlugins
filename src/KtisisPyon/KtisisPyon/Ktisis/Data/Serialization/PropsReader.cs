using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ktisis.Data.Config.Props;
using Newtonsoft.Json;

namespace Ktisis.Data.Serialization;

public static class PropsReader
{
	public static PropSchema ReadStream(Stream stream)
	{
		PropSchema propSchema = new PropSchema();
		propSchema.Props.AddRange(DeserializeProps(stream));
		return propSchema;
	}

	private static List<PropEntry> DeserializeProps(Stream stream)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		JsonSerializer val = new JsonSerializer();
		StreamReader streamReader = new StreamReader(stream, new UTF8Encoding());
		List<PropEntry> result = new List<PropEntry>();
		JsonTextReader val2 = new JsonTextReader((TextReader)streamReader);
		try
		{
			((JsonReader)val2).CloseInput = false;
			((JsonReader)val2).SupportMultipleContent = true;
			while (((JsonReader)val2).Read())
			{
				result = val.Deserialize<List<PropEntry>>((JsonReader)(object)val2);
			}
			return result;
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}
}
