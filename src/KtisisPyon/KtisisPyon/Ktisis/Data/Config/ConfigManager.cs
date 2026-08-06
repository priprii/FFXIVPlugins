using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dalamud.Plugin;
using Ktisis.Core.Attributes;
using Ktisis.Data.Config.Bones;
using Ktisis.Data.Config.Sections;
using Ktisis.Data.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Ktisis.Data.Config;

[Singleton]
public class ConfigManager : IDisposable
{
	private readonly IDalamudPluginInterface _dpi;

	internal bool _isLoaded;

	private bool _isDisposing;

	public Configuration File { get; internal set; }

	public event OnConfigSaved? OnSaved;

	public ConfigManager(IDalamudPluginInterface dpi)
	{
		_dpi = dpi;
	}

	public void Load()
	{
		bool flag = false;
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		Configuration configuration = null;
		bool configFileExists = GetConfigFileExists();
		try
		{
			configuration = OpenConfigFile();
			if (configuration == null || configuration.Version != -1)
			{
				if (configuration != null && configuration.Version < 10)
				{
					configuration.Version = 10;
					MigrateSchema(configuration);
				}
				if (configuration != null && configuration.Version < 11)
				{
					configuration.Version = 11;
					GenerateDefaultPresets(configuration);
					MigrateSchema(configuration);
				}
			}
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to load configuration:\n{value}");
		}
		if (configuration == null)
		{
			if (configFileExists)
			{
				BackupConfigFile();
				Ktisis.Log.Warning("Configuration failed to load; existing file backed up");
				Ktisis.WarningNotification("Failed to load Ktisis configuration. A backup of the old file was saved.");
			}
			try
			{
				configuration = CreateDefault();
				GenerateDefaultPresets(configuration);
				flag = !configFileExists;
			}
			catch (Exception value2)
			{
				Ktisis.Log.Error($"Failed to create default configuration:\n{value2}");
				throw;
			}
		}
		File = configuration;
		_isLoaded = true;
		if (flag)
		{
			Save();
		}
		stopwatch.Stop();
		Ktisis.Log.Debug($"Configuration loaded in {stopwatch.Elapsed.TotalMilliseconds:0.00}ms");
	}

	private void BackupConfigFile()
	{
		try
		{
			string configFilePath = GetConfigFilePath();
			string destFileName = $"{configFilePath}.bak-{DateTime.Now:yyyyMMdd-HHmmss}";
			System.IO.File.Copy(configFilePath, destFileName, overwrite: true);
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to back up configuration file:\n{value}");
		}
	}

	public void Save()
	{
		if (!_isLoaded)
		{
			return;
		}
		try
		{
			SaveConfigFile();
			if (!_isDisposing)
			{
				this.OnSaved?.Invoke(File);
			}
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to save configuration:\n{value}");
		}
	}

	private void MigrateSchema(Configuration cfg)
	{
		Ktisis.Log.Debug("Updating category schema.");
		CategoryConfig categoryConfig = SchemaReader.ReadCategories();
		foreach (BoneCategory category in categoryConfig.CategoryList)
		{
			BoneCategory byName = cfg.Categories.GetByName(category.Name);
			if (byName != null)
			{
				category.BoneColor = byName.BoneColor;
				category.GroupColor = byName.GroupColor;
				category.LinkedColors = byName.LinkedColors;
			}
		}
		cfg.Categories = categoryConfig;
	}

	internal void GenerateDefaultPresets(Configuration cfg)
	{
		CategoryConfig categories = SchemaReader.ReadCategories();
		List<string> list = categories.CategoryList.SelectMany((BoneCategory x) => x.Presets).Distinct().ToList();
		Ktisis.Log.Info("All Presets: {0}", string.Join(", ", list));
		foreach (var (key, value) in list.ToDictionary((string x) => x, (string item) => categories.CategoryList.Where((BoneCategory x) => x.Presets.Contains(item)).SelectMany((BoneCategory x) => x.Bones.Select((CategoryBone y) => y.Name)).ToImmutableHashSet()))
		{
			cfg.Presets.Presets.TryAdd(key, value);
		}
	}

	public bool GetConfigFileExists()
	{
		return Path.Exists(GetConfigFilePath());
	}

	private Configuration? OpenConfigFile()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		Ktisis.Log.Verbose("Loading configuration...");
		string configFilePath = GetConfigFilePath();
		if (!Path.Exists(configFilePath))
		{
			return null;
		}
		string text = System.IO.File.ReadAllText(configFilePath);
		JsonSerializerSettings val = new JsonSerializerSettings
		{
			Error = delegate(object? _, ErrorEventArgs args)
			{
				Ktisis.Log.Warning("Skipping invalid configuration member '" + args.ErrorContext.Path + "':\n" + args.ErrorContext.Error.Message);
				args.ErrorContext.Handled = true;
			}
		};
		return JsonConvert.DeserializeObject<Configuration>(text, val);
	}

	private void SaveConfigFile()
	{
		Ktisis.Log.Verbose("Saving configuration...");
		string configFilePath = GetConfigFilePath();
		string contents = JsonConvert.SerializeObject((object)File, (Formatting)1);
		System.IO.File.WriteAllText(configFilePath, contents);
	}

	private string GetConfigFilePath()
	{
		return Path.Join(_dpi.GetPluginConfigDirectory(), "KtisisV3.json");
	}

	internal Configuration CreateDefault()
	{
		return new Configuration
		{
			Categories = SchemaReader.ReadCategories()
		};
	}

	internal Configuration GenerateOrLoad()
	{
		return (GetConfigFileExists() ? OpenConfigFile() : CreateDefault()) ?? CreateDefault();
	}

	public void Dispose()
	{
		_isDisposing = true;
		Save();
		GC.SuppressFinalize(this);
	}
}
