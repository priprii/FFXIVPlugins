using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Ktisis.Data.Json;

namespace Ktisis.Localization;

public class LocaleDataLoader
{
	private static readonly JsonReaderOptions readerOptions = new JsonReaderOptions
	{
		AllowTrailingCommas = true,
		CommentHandling = JsonCommentHandling.Skip
	};

	private Stream GetResourceStream(string technicalName)
	{
		return Assembly.GetExecutingAssembly().GetManifestResourceStream("KtisisPyon.Localization.Data.en_US.json") ?? throw new Exception("Cannot find data file '" + technicalName + "'");
	}

	public LocaleMetaData LoadMeta(string technicalName)
	{
		using Stream stream = GetResourceStream(technicalName);
		Stream stream2 = stream;
		Span<byte> blockBuffer = stackalloc byte[4096];
		BlockBufferJsonReader reader = new BlockBufferJsonReader(stream2, blockBuffer, readerOptions);
		reader.Read();
		if (reader.Reader.TokenType != JsonTokenType.StartObject)
		{
			throw new Exception("Locale Data file '" + technicalName + ".json' does not contain a top-level object.");
		}
		while (reader.Read())
		{
			switch (reader.Reader.TokenType)
			{
			case JsonTokenType.PropertyName:
				if (reader.Reader.GetString() == "$meta")
				{
					return ReadMetaObject(technicalName, ref reader);
				}
				break;
			case JsonTokenType.EndObject:
				throw new Exception("Locale Data file '" + technicalName + "' is is missing the top-level '$meta' object.");
			default:
				throw new Exception("Should not reach this point.");
			}
			reader.SkipIt();
		}
		throw new Exception("Locale Data file '" + technicalName + ".json' is missing its meta data (top-level '$meta' key not found)");
	}

	private LocaleMetaData ReadMetaObject(string technicalName, ref BlockBufferJsonReader reader)
	{
		reader.Read();
		if (reader.Reader.TokenType != JsonTokenType.StartObject)
		{
			throw new Exception("Locale Data file '" + technicalName + ".json' has a non-object at the top-level '$meta' key.");
		}
		string text = null;
		string text2 = null;
		string[] array = null;
		while (true)
		{
			reader.Reader.Read();
			switch (reader.Reader.TokenType)
			{
			case JsonTokenType.PropertyName:
			{
				string text3 = reader.Reader.GetString();
				reader.Read();
				switch (text3)
				{
				case "__comment":
					break;
				case "displayName":
					if (reader.Reader.TokenType != JsonTokenType.String)
					{
						throw new Exception("Locale data file '" + technicalName + ".json' has an invalid '%.$meta.displayName' value (not a string).");
					}
					text = reader.Reader.GetString();
					break;
				case "selfName":
					if (reader.Reader.TokenType != JsonTokenType.String)
					{
						throw new Exception("Locale data file '" + technicalName + ".json' has an invalid '%.$meta.selfName' value (not a string).");
					}
					text2 = reader.Reader.GetString();
					break;
				case "maintainers":
				{
					if (reader.Reader.TokenType != JsonTokenType.StartArray)
					{
						throw new Exception("Locale data file '" + technicalName + ".json' has an invalid '%.$meta.maintainers' value (not an array).");
					}
					List<string> list = new List<string>();
					for (int i = 0; reader.Read(); i++)
					{
						switch (reader.Reader.TokenType)
						{
						case JsonTokenType.Null:
							list.Add(null);
							continue;
						case JsonTokenType.String:
							list.Add(reader.Reader.GetString());
							continue;
						default:
							throw new Exception($"Locale data file '{technicalName}' has an invalid value at '%.$meta.maintainers.{i}' (not a string or null).");
						case JsonTokenType.EndArray:
							break;
						}
						break;
					}
					array = list.ToArray();
					break;
				}
				default:
					Ktisis.Log.Warning($"Locale data file '{technicalName}.json' has unknown meta key at '%.$meta.{reader.Reader.GetString()}'");
					reader.SkipIt();
					break;
				}
				break;
			}
			case JsonTokenType.EndObject:
				if (text == null)
				{
					throw new Exception("Locale data file '" + technicalName + ".json' is missing the '%.$meta.displayName' value.");
				}
				if (text2 == null)
				{
					throw new Exception("Locale data file '" + technicalName + ".json' is missing the '%.$meta.selfName' value.");
				}
				if (array == null)
				{
					array = new string[1];
				}
				return new LocaleMetaData(technicalName, text, text2, array);
			}
		}
	}

	public LocaleData LoadData(string technicalName)
	{
		return _LoadData(technicalName, null);
	}

	public LocaleData LoadData(LocaleMetaData metaData)
	{
		return _LoadData(metaData.TechnicalName, metaData);
	}

	private LocaleData _LoadData(string technicalName, LocaleMetaData? meta)
	{
		using Stream stream = GetResourceStream(technicalName);
		Stream stream2 = stream;
		Span<byte> blockBuffer = stackalloc byte[4096];
		BlockBufferJsonReader reader = new BlockBufferJsonReader(stream2, blockBuffer, readerOptions);
		reader.Read();
		if (reader.Reader.TokenType != JsonTokenType.StartObject)
		{
			throw new Exception("Locale Data file '" + technicalName + "' does not contain a top-level object.");
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		Stack<string> stack = new Stack<string>();
		string text = null;
		int num = 0;
		while (reader.Read())
		{
			switch (reader.Reader.TokenType)
			{
			case JsonTokenType.PropertyName:
				if (stack.Count == 0 && reader.Reader.GetString() == "$meta")
				{
					num++;
					if (meta == null)
					{
						meta = ReadMetaObject(technicalName, ref reader);
					}
					else
					{
						reader.SkipIt();
					}
				}
				else if (reader.Reader.GetString() == "__comment")
				{
					reader.SkipIt();
				}
				else
				{
					stack.TryPeek(out var result);
					text = ((result == null) ? reader.Reader.GetString() : (result + "." + reader.Reader.GetString()));
				}
				continue;
			case JsonTokenType.String:
				dictionary.Add(text, reader.Reader.GetString());
				continue;
			case JsonTokenType.StartObject:
				stack.Push(text);
				continue;
			case JsonTokenType.EndObject:
				break;
			case JsonTokenType.StartArray:
				WarnUnsupported(technicalName, "array", text);
				reader.SkipIt();
				continue;
			case JsonTokenType.True:
			case JsonTokenType.False:
				WarnUnsupported(technicalName, "boolean", text);
				continue;
			case JsonTokenType.Number:
				WarnUnsupported(technicalName, "number", text);
				continue;
			case JsonTokenType.Null:
				WarnUnsupported(technicalName, "null", text);
				continue;
			default:
				continue;
			}
			if (!stack.TryPop(out var _))
			{
				break;
			}
		}
		if (num <= 1)
		{
			if (num == 0)
			{
				throw new Exception("Locale Data file '" + technicalName + ".json' is is missing the top-level '$meta' object.");
			}
		}
		else
		{
			Ktisis.Log.Warning("Locale Data file '" + technicalName + ".json' has {0} top-level '$meta' objects?!", num);
		}
		dictionary.TrimExcess();
		return new LocaleData(meta, dictionary);
	}

	private void WarnUnsupported(string technicalName, string elementType, string currentKey)
	{
		Ktisis.Log.Warning("Locale Data File '{0}.json' has an unsupported {1} at '%.{2}'.", technicalName, elementType, currentKey);
	}
}
