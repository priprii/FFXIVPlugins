using System.Collections.Generic;
using System.IO;
using System.Text;
using Ktisis.Data.Config.Gobos;

namespace Ktisis.Data.Serialization;

public class GoboReader
{
	public static GoboSchema ReadStream(Stream stream)
	{
		GoboSchema goboSchema = new GoboSchema();
		goboSchema.Gobos.AddRange(DeserializeGobos(stream));
		return goboSchema;
	}

	private static List<GoboEntry> DeserializeGobos(Stream stream)
	{
		StreamReader streamReader = new StreamReader(stream, new UTF8Encoding());
		List<GoboEntry> list = new List<GoboEntry>();
		string text = streamReader.ReadLine();
		string[] array = text?.Split(",");
		if (text == null || array.Length != 2)
		{
			return list;
		}
		text = streamReader.ReadLine();
		while (text != null)
		{
			if (!(text.Trim() == string.Empty))
			{
				array = text.Split(",");
				if (array.Length == 2)
				{
					list.Add(new GoboEntry
					{
						Path = array[0],
						Name = array[1]
					});
					text = streamReader.ReadLine();
				}
			}
		}
		return list;
	}
}
