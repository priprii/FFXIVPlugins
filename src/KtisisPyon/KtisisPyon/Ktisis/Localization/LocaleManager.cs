using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Dalamud.Plugin;
using Ktisis.Core.Attributes;
using Ktisis.Data.Config;
using Ktisis.Data.Config.Bones;
using Ktisis.Data.Config.Sections;
using Ktisis.Editor.Posing.Types;

namespace Ktisis.Localization;

[Singleton]
public class LocaleManager : IDisposable
{
	internal delegate void LocaleChange();

	private ConfigManager? _cfg;

	private readonly IDalamudPluginInterface _dpi;

	private readonly LocaleDataLoader Loader = new LocaleDataLoader();

	private List<List<string>> CompatibleLanguages = new List<List<string>>
	{
		new List<string> { "zh_CN", "zh_SG" },
		new List<string> { "zh_TW", "zh_MO", "zh_HK" }
	};

	public List<LocaleMetaData> AvailableLocales = new List<LocaleMetaData>();

	public LocaleData? Data;

	public LocaleData? FallbackData;

	internal event LocaleChange LocaleChanged;

	public LocaleManager(IDalamudPluginInterface dpi)
	{
		_dpi = dpi;
	}

	public void Initialize(ConfigManager cfg)
	{
		_cfg = cfg;
		HandleLanguageChangeDelegate();
		foreach (string resource in from s in Assembly.GetExecutingAssembly().GetManifestResourceNames()
			where s.StartsWith("Ktisis.Localization.Data")
			select s)
		{
			if (AvailableLocales.All((LocaleMetaData l) => l.TechnicalName != resource.Split('.')[3]))
			{
				AvailableLocales.Add(Loader.LoadMeta(resource.Split('.')[3]));
			}
		}
		if (cfg != null && cfg._isLoaded)
		{
			Configuration file = cfg.File;
			if (file != null)
			{
				LocaleConfig locale = file.Locale;
				if (locale != null && locale.AutoDetect)
				{
					LanguageChanged(_dpi.UiLanguage);
					return;
				}
			}
		}
		if (!cfg._isLoaded)
		{
			LoadLocale("en_US");
			return;
		}
		LoadLocale(_cfg.File.Locale.LocaleId);
		if (_cfg.File.Locale.LocaleId != "en_US")
		{
			LoadFallbackLocale();
		}
	}

	public void HandleLanguageChangeDelegate()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		_dpi.LanguageChanged -= new LanguageChangedDelegate(LanguageChanged);
		ConfigManager cfg = _cfg;
		if (cfg == null)
		{
			return;
		}
		Configuration file = cfg.File;
		if (file != null)
		{
			LocaleConfig locale = file.Locale;
			if (locale != null && locale.AutoDetect)
			{
				LanguageChanged(_dpi.UiLanguage);
				_dpi.LanguageChanged += new LanguageChangedDelegate(LanguageChanged);
			}
		}
	}

	public void LanguageChanged(string lang)
	{
		string localeFile = lang + "_" + RegionInfo.CurrentRegion.TwoLetterISORegionName;
		LocaleData? data = Data;
		if (data != null && data.MetaData.TechnicalName.Equals(localeFile))
		{
			_cfg.File.Locale.LocaleId = localeFile;
			LoadLocale(localeFile);
		}
		else if (CompatibleLanguages.Any((List<string> l) => l.Contains(localeFile) && AvailableLocales.Any((LocaleMetaData p) => p.TechnicalName == l.First())))
		{
			string text = CompatibleLanguages.First((List<string> l) => l.Contains(localeFile)).First();
			_cfg.File.Locale.LocaleId = text;
			LoadLocale(text);
		}
		else if (AvailableLocales.Any((LocaleMetaData l) => l.TechnicalName == localeFile) && CompatibleLanguages.All((List<string> l) => !l.Contains(localeFile)))
		{
			_cfg.File.Locale.LocaleId = localeFile;
			LoadLocale(localeFile);
		}
		else if (AvailableLocales.Any((LocaleMetaData l) => l.TechnicalName.StartsWith(lang)))
		{
			_cfg.File.Locale.LocaleId = AvailableLocales.First((LocaleMetaData l) => l.TechnicalName.StartsWith(lang)).TechnicalName;
			LoadLocale(_cfg.File.Locale.LocaleId);
		}
		else
		{
			_cfg.File.Locale.LocaleId = "en_US";
			LoadLocale("en_US");
		}
	}

	public string Translate(string handle, Dictionary<string, string>? parameters = null)
	{
		return Data?.Translate(handle, parameters) ?? FallbackData?.Translate(handle, parameters) ?? handle;
	}

	public bool HasTranslationFor(string handle)
	{
		return Data?.HasTranslationFor(handle) ?? false;
	}

	public void LoadLocale(string technicalName)
	{
		Ktisis.Log.Verbose("Reading localization file for '" + technicalName + "'");
		if (Data == null || Data.MetaData.TechnicalName != technicalName)
		{
			Data = Loader.LoadData(technicalName);
			if (technicalName != "en_US")
			{
				LoadFallbackLocale();
			}
			else
			{
				FallbackData = null;
			}
			this.LocaleChanged?.Invoke();
		}
	}

	public void LoadFallbackLocale()
	{
		Ktisis.Log.Verbose("FALLBACK - Reading localization file for 'en_US'");
		if (FallbackData == null || FallbackData.MetaData.TechnicalName != "en_US")
		{
			FallbackData = Loader.LoadData("en_US");
		}
	}

	public string GetBoneName(PartialBoneInfo bone)
	{
		return GetBoneName(bone.Name);
	}

	public string GetBoneName(string name)
	{
		string handle = "bone." + name;
		if (!_cfg.File.Categories.ShowFriendlyBoneNames || !HasTranslationFor(handle))
		{
			return name;
		}
		return Translate(handle);
	}

	public string GetCategoryName(BoneCategory category)
	{
		string handle = "boneCategory." + category.Name;
		if (!HasTranslationFor(handle))
		{
			return category.Name;
		}
		return Translate(handle);
	}

	public int RandomHintKey()
	{
		int? num = Data?.KeysMatchingPrefix("hints.");
		if (!num.HasValue)
		{
			return 0;
		}
		return new Random().Next(0, num.Value) + 1;
	}

	public void Dispose()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		_dpi.LanguageChanged -= new LanguageChangedDelegate(LanguageChanged);
	}
}
