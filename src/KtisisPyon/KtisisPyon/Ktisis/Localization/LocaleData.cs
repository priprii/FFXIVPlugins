using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ktisis.Localization;

public class LocaleData
{
	private readonly Dictionary<string, string> _translationData;

	private readonly HashSet<string> warnedKeys = new HashSet<string>();

	public LocaleMetaData MetaData { get; }

	public LocaleData(LocaleMetaData metaData, Dictionary<string, string> translationData)
	{
		_translationData = translationData;
		MetaData = metaData;
	}

	public string? Translate(string key, Dictionary<string, string>? parameters = null)
	{
		if (!_translationData.TryGetValue(key, out string value))
		{
			if (warnedKeys.Add(key))
			{
				Ktisis.Log.Warning("Unassigned translation key '{0}' for locale '{1}'", key, MetaData.TechnicalName);
			}
			return null;
		}
		return ReplaceParameters(key, value, parameters);
	}

	public bool HasTranslationFor(string key)
	{
		return _translationData.ContainsKey(key);
	}

	public int KeysMatchingPrefix(string prefix)
	{
		return _translationData.Keys.Count((string k) => k.StartsWith(prefix));
	}

	private string ReplaceParameters(string handle, string translationString, Dictionary<string, string>? parameters)
	{
		StringBuilder stringBuilder = new StringBuilder(translationString.Length);
		StringBuilder stringBuilder2 = new StringBuilder(16);
		bool flag = false;
		foreach (char c in translationString)
		{
			if (!flag)
			{
				if (c == '%')
				{
					flag = true;
				}
				else
				{
					stringBuilder.Append(c);
				}
			}
			else if (c == '%')
			{
				if (stringBuilder2.Length == 0)
				{
					stringBuilder.Append('%');
				}
				else
				{
					string text = stringBuilder2.ToString();
					string value = null;
					parameters?.TryGetValue(text, out value);
					if (value == null)
					{
						Ktisis.Log.Warning("Unassigned parameter '{0}' in value for '{1}' in locale '{2}'", text, handle, MetaData.TechnicalName);
						value = "%" + text + "%";
					}
					stringBuilder.Append(value);
					stringBuilder2.Clear();
				}
				flag = false;
			}
			else
			{
				stringBuilder2.Append(c);
			}
		}
		return stringBuilder.ToString();
	}
}
