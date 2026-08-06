using System.Text.Json;

namespace Ktisis.Data.Json;

public static class JsonReaderExtensions
{
	public static void SkipIt(this ref BlockBufferJsonReader reader)
	{
		if (reader.Reader.TrySkip())
		{
			return;
		}
		if (reader.Reader.TokenType == JsonTokenType.PropertyName)
		{
			reader.Read();
		}
		if (reader.Reader.TokenType == JsonTokenType.StartObject || reader.Reader.TokenType == JsonTokenType.StartArray)
		{
			int currentDepth = reader.Reader.CurrentDepth;
			do
			{
				reader.Read();
			}
			while (reader.Reader.CurrentDepth > currentDepth);
		}
	}
}
