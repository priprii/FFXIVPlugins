using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Ktisis.Data.Config;
using Ktisis.Data.Config.Sections;
using Ktisis.Data.Files;
using Ktisis.Data.Json;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Posing.Data;
using Ktisis.Editor.Posing.Types;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Character;
using Ktisis.Scene.Types;
using Ktisis.Services.Data;

namespace Ktisis.Editor.Posing.AutoSave;

public class PoseAutoSave : IDisposable
{
	private readonly IEditorContext _ctx;

	private readonly IFramework _framework;

	private readonly FormatService _format;

	private readonly SceneDataService _sceneData;

	private readonly Timer _timer = new Timer();

	private readonly Queue<string> _prefixes = new Queue<string>();

	private AutoSaveConfig _cfg;

	private IPosingManager Posing => _ctx.Posing;

	private ISceneManager Scene => _ctx.Scene;

	public PoseAutoSave(IEditorContext ctx, IFramework framework, FormatService format, SceneDataService sceneService)
	{
		_ctx = ctx;
		_framework = framework;
		_format = format;
		_sceneData = sceneService;
	}

	public void Initialize(Configuration cfg)
	{
		_timer.Elapsed += OnElapsed;
		Configure(cfg);
	}

	public void Configure(Configuration cfg)
	{
		_cfg = cfg.AutoSave;
		_timer.Interval = TimeSpan.FromSeconds(_cfg.Interval).TotalMilliseconds;
		if (_timer.Enabled != _cfg.Enabled)
		{
			_timer.Enabled = _cfg.Enabled;
		}
	}

	private async void OnElapsed(object? sender, ElapsedEventArgs e)
	{
		if (!Posing.IsValid)
		{
			_timer.Stop();
		}
		else if (_cfg.Enabled && Posing.IsEnabled)
		{
			try
			{
				await _framework.RunOnFrameworkThread((Action)Save);
			}
			catch (Exception value)
			{
				Ktisis.Log.Error($"Failed to save poses:\n{value}");
			}
		}
	}

	public void Save()
	{
		string text = _format.Replace(_cfg.FolderFormat);
		string text2 = Path.Combine(_cfg.FilePath, text);
		_prefixes.Enqueue(text);
		if (!Directory.Exists(text2))
		{
			Directory.CreateDirectory(text2);
		}
		List<CharaEntity> list = Scene.Children.Where((SceneEntity entity) => entity is CharaEntity).Cast<CharaEntity>().ToList();
		if (list.Count == 0)
		{
			Ktisis.Log.Warning("No valid entities, skipping auto save.");
			return;
		}
		Ktisis.Log.Info($"Auto saving poses for {list.Count} character(s)");
		foreach (CharaEntity item in list)
		{
			if (item.Pose != null)
			{
				int num = 1;
				string text3 = _format.StripInvalidChars(item.Name);
				string path = Path.Combine(text2, text3 + ".pose");
				while (Path.Exists(path))
				{
					path = Path.Combine(text2, $"{text3} ({++num}).pose");
				}
				JsonFileSerializer jsonFileSerializer = new JsonFileSerializer();
				PoseFile obj = new EntityPoseConverter(item.Pose).SaveFile();
				File.WriteAllText(path, jsonFileSerializer.Serialize(obj));
			}
		}
		_sceneData.WriteFile(text2 + "\\autosave.ktscene");
		Ktisis.Log.Verbose($"Prefix count: {_prefixes.Count} max: {_cfg.Count}");
		while (_prefixes.Count > _cfg.Count)
		{
			DeleteOldest();
		}
	}

	private void DeleteOldest()
	{
		string path = _prefixes.Dequeue();
		string text = Path.Combine(_cfg.FilePath, path);
		if (Directory.Exists(text))
		{
			Ktisis.Log.Verbose("Deleting " + text);
			Directory.Delete(text, recursive: true);
		}
		DeleteEmptyDirs(_cfg.FilePath);
	}

	private static void DeleteEmptyDirs(string dir)
	{
		if (StringExtensions.IsNullOrEmpty(dir))
		{
			throw new ArgumentException("Starting directory is a null reference or empty string", "dir");
		}
		try
		{
			foreach (string item in Directory.EnumerateDirectories(dir))
			{
				DeleteEmptyDirs(item);
			}
			if (Directory.EnumerateFileSystemEntries(dir).Any())
			{
				return;
			}
			try
			{
				Directory.Delete(dir);
			}
			catch (DirectoryNotFoundException)
			{
			}
		}
		catch (UnauthorizedAccessException ex2)
		{
			Ktisis.Log.Error(ex2.ToString());
		}
	}

	private void Clear()
	{
		try
		{
			while (_cfg.ClearOnExit && _prefixes.Count > 0)
			{
				DeleteOldest();
			}
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to clear auto saves:\n{value}");
		}
	}

	public void Dispose()
	{
		_timer.Elapsed -= OnElapsed;
		_timer.Stop();
		_timer.Dispose();
		Clear();
		GC.SuppressFinalize(this);
	}
}
