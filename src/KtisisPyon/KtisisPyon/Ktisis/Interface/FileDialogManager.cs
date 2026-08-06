using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Utility;
using GLib.Popups.ImFileDialog;
using GLib.Popups.ImFileDialog.Data;
using Ktisis.Core.Attributes;
using Ktisis.Data.Config;
using Ktisis.Data.Files;
using Ktisis.Data.Json;
using Ktisis.Services.Meta;

namespace Ktisis.Interface;

[Singleton]
public class FileDialogManager
{
	private readonly ConfigManager _cfg;

	private readonly ImageDataProvider _img;

	private readonly JsonFileSerializer _serializer = new JsonFileSerializer();

	private readonly FileDialogManager _fileManager = new FileDialogManager();

	private DialogType _openDialog = DialogType.None;

	private readonly FileDialogOptions ImageOptions = new FileDialogOptions
	{
		Flags = FileDialogFlags.OpenMode,
		Filters = "Images{.png,.jpg,.jpeg}"
	};

	private FileDialogLocation? AutoSaveLoc;

	public event Action<FileDialog>? OnOpenDialog;

	public event EventHandler<string>? OnSelectionChanged;

	public FileDialogManager(ConfigManager cfg, ImageDataProvider img)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		_cfg = cfg;
		_img = img;
		_fileManager.SelectionChanged += SelectionChange;
	}

	private void SelectionChange(object? sender, string path)
	{
		this.OnSelectionChanged?.Invoke(sender, path);
	}

	public void Initialize()
	{
		_img.Initialize();
	}

	public void Draw()
	{
		_fileManager.Draw();
	}

	public bool IsDialogOpen()
	{
		return _openDialog == DialogType.Pose;
	}

	private T OpenDialog<T>(T dialog) where T : FileDialog
	{
		if (_cfg.File.File.LastOpenedPaths.TryGetValue(dialog.Title, out string value))
		{
			dialog.Open(value);
		}
		else
		{
			dialog.Open();
		}
		this.OnOpenDialog?.Invoke(dialog);
		return dialog;
	}

	private void SaveDialogState(FileDialog dialog)
	{
		if (dialog.ActiveDirectory != null)
		{
			_cfg.File.File.LastOpenedPaths[dialog.Title] = dialog.ActiveDirectory;
		}
	}

	public void OpenFile(string name, Action<string> handler, FileDialogOptions? options = null, DialogType type = DialogType.Other)
	{
		_openDialog = type;
		if ((object)options == null)
		{
			options = new FileDialogOptions();
		}
		PopulateOptions(options);
		Ktisis.Log.Debug("Opening file dialog...");
		EnsureFileDialogOptions(options);
		_fileManager.OpenFileDialog(name, options.Filters, (Action<bool, List<string>>)delegate(bool isOk, List<string> paths)
		{
			_openDialog = DialogType.None;
			if (isOk)
			{
				string text = paths.FirstOrDefault();
				if (!StringExtensions.IsNullOrEmpty(text))
				{
					handler(text);
				}
			}
		}, options.MaxOpenCount, (_cfg.File.File.DefaultLocation == string.Empty) ? null : _cfg.File.File.DefaultLocation, false);
	}

	public void OpenFile<T>(string name, Action<string, T> handler, FileDialogOptions? options = null, DialogType type = DialogType.Other) where T : JsonFile
	{
		OpenFile(name, delegate(string path)
		{
			string text = File.ReadAllText(path);
			if (Path.GetExtension(path).Equals(".cmp"))
			{
				text = LegacyPoseHelpers.ConvertLegacyPose(text);
			}
			T val = _serializer.Deserialize<T>(text);
			if (val != null)
			{
				handler(path, val);
			}
		}, options, type);
	}

	public void SaveFile(string name, string content, FileDialogOptions? options = null)
	{
		if ((object)options == null)
		{
			options = new FileDialogOptions();
		}
		PopulateOptions(options);
		string text = options.DefaultFileName;
		if (options.Extension != null && !text.EndsWith(options.Extension))
		{
			text += options.Extension;
		}
		EnsureFileDialogOptions(options);
		_fileManager.SaveFileDialog(name, options.Filters, text, options.Extension ?? "", (Action<bool, string>)delegate(bool isOk, string path)
		{
			if (isOk && !StringExtensions.IsNullOrEmpty(path))
			{
				File.WriteAllText(path, content);
			}
		}, (string)null, true);
	}

	public void SaveFile<T>(string name, T file, FileDialogOptions? options = null) where T : JsonFile
	{
		string content = _serializer.Serialize(file);
		SaveFile(name, content, options);
	}

	public void OpenImage(string name, Action<string> handler)
	{
		FileDialog dialog = new FileDialog(name, delegate(FileDialog sender, IEnumerable<string> paths)
		{
			foreach (string path in paths)
			{
				handler(path);
			}
		}, ImageOptions);
		_img.BindMetadata(dialog);
		OpenDialog(dialog);
	}

	public void OpenFolder(string name, Action<string> handler)
	{
		_fileManager.OpenFolderDialog(name, (Action<bool, string>)delegate(bool isOk, string path)
		{
			if (isOk && !StringExtensions.IsNullOrEmpty(path))
			{
				handler(path);
			}
		}, (_cfg.File.File.DefaultLocation == string.Empty) ? null : _cfg.File.File.DefaultLocation, false);
	}

	private void PopulateOptions(FileDialogOptions options)
	{
		string filePath = _cfg.File.AutoSave.FilePath;
		if (AutoSaveLoc == null)
		{
			AutoSaveLoc = new FileDialogLocation("AutoSave", filePath, (FontAwesomeIcon)61563, -1);
		}
		else if (!string.Equals(AutoSaveLoc.FullPath, filePath, StringComparison.CurrentCultureIgnoreCase))
		{
			AutoSaveLoc.FullPath = filePath;
		}
		if (!options.Locations.Contains(AutoSaveLoc))
		{
			options.Locations.Add(AutoSaveLoc);
			Ktisis.Log.Debug("Added autosave: " + filePath);
		}
	}

	private void EnsureFileDialogOptions(FileDialogOptions options)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		_fileManager.CustomSideBarItems.Clear();
		foreach (FileDialogLocation location in options.Locations)
		{
			_fileManager.CustomSideBarItems.Add((location.Name, location.FullPath, location.Icon, location.Position));
		}
		foreach (var (item, item2) in _cfg.File.File.CustomLocations)
		{
			_fileManager.CustomSideBarItems.Add((item2, item, (FontAwesomeIcon)61563, -1));
		}
	}
}
