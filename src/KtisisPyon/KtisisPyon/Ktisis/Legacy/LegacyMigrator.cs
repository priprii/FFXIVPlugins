using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Configuration;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin;
using Ktisis.Common.Utility;
using Ktisis.Core.Attributes;
using Ktisis.Data.Config;
using Ktisis.Data.Config.Actions;
using Ktisis.Data.Config.Bones;
using Ktisis.Data.Config.Sections;
using Ktisis.Interface;
using Ktisis.Legacy.Interface;
using Ktisis.Services.Game;

namespace Ktisis.Legacy;

[Singleton]
public class LegacyMigrator
{
	private readonly GPoseService _gpose;

	private readonly GuiManager _gui;

	private readonly IDalamudPluginInterface _dpi;

	private readonly ConfigManager _cfg;

	internal LegacyConfig.Configuration? _legacyCfg;

	internal Configuration _tempConfig;

	private readonly Dictionary<string, string> LegacyRaceSexMap = new Dictionary<string, string>
	{
		{ "Midlander_Masculine", "101" },
		{ "Midlander_Feminine", "201" },
		{ "Highlander_Masculine", "301" },
		{ "Highlander_Feminine", "401" },
		{ "Elezen_Masculine", "501" },
		{ "Elezen_Feminine", "601" },
		{ "Miqote_Masculine", "701" },
		{ "Miqote_Feminine", "801" },
		{ "Roegadyn_Masculine", "901" },
		{ "Roegadyn_Feminine", "1001" },
		{ "Lalafell_Masculine", "1101" },
		{ "Lalafell_Feminine", "1201" },
		{ "AuRa_Masculine", "1301" },
		{ "AuRa_Feminine", "1401" },
		{ "Hrothgar_Masculine", "1501" },
		{ "Hrothgar_Feminine", "1601" },
		{ "Viera_Masculine", "1701" },
		{ "Viera_Feminine", "1801" }
	};

	private readonly Dictionary<string, string> LegacyCategoryMap = new Dictionary<string, string>
	{
		{ "clothes", "Clothing" },
		{ "body", "Body" },
		{ "eyes", "Eyes" },
		{ "mouth", "Mouth" },
		{ "face", "Face" },
		{ "hair", "Hair" },
		{ "weapons", "Weapons" },
		{ "right hand", "RightHand" },
		{ "left hand", "LeftHand" },
		{ "tail", "Tail" },
		{ "ears", "Ears" },
		{ "ivcs left hand", "LeftHandIvcs" },
		{ "ivcs right hand", "RightHandIvcs" },
		{ "ivcs left foot", "LeftFootIvcs" },
		{ "ivcs right foot", "RightFootIvcs" },
		{ "ivcs penis", "PenisIvcs" },
		{ "ivcs vagina", "VaginaIvcs" },
		{ "ivcs buttocks", "BottomIvcs" }
	};

	public bool v2ConfigExists;

	public bool v3ConfigExists;

	private bool _confirmed;

	public event Action? OnConfirmed;

	public LegacyMigrator(GPoseService gpose, GuiManager gui, IDalamudPluginInterface dpi, ConfigManager cfg)
	{
		_gpose = gpose;
		_gui = gui;
		_dpi = dpi;
		_cfg = cfg;
	}

	public void Setup()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		v2ConfigExists = _dpi.ConfigFile.Exists;
		v3ConfigExists = File.Exists(_dpi.ConfigDirectory?.ToString() + "\\KtisisV3.json");
		_tempConfig = _cfg.GenerateOrLoad();
		Ktisis.Locale.Initialize(_cfg);
		if (v2ConfigExists)
		{
			Ktisis.Log.Warning("User is migrating from Ktisis v0.2, activating legacy mode.");
			PluginConfigurations val = new PluginConfigurations(new DirectoryInfo(_dpi.GetPluginConfigDirectory()).Parent.ToString());
			_legacyCfg = val.LoadForType<LegacyConfig.Configuration>("Ktisis");
		}
		else
		{
			Ktisis.Log.Warning("User is migrating from Ktisis v0.3 beta, activating legacy mode.");
		}
		_gpose.StateChanged += OnGPoseStateChanged;
		_gpose.Subscribe();
	}

	private void OnGPoseStateChanged(object sender, bool state)
	{
		if (state && !_confirmed)
		{
			_gui.GetOrCreate<MigratorWindow>(new object[1] { this }).Open();
		}
	}

	internal void MigrateConfig()
	{
		Configuration configuration = (_tempConfig = _cfg.CreateDefault());
		configuration.Editor.IncognitoPlayerNames = _legacyCfg?.DisplayCharName ?? configuration.Editor.IncognitoPlayerNames;
		CategoryConfig categories = configuration.Categories;
		LegacyConfig.Configuration? legacyCfg = _legacyCfg;
		categories.ShowNsfwBones = ((legacyCfg == null) ? ((bool?)null) : (!legacyCfg.CensorNsfw)) ?? configuration.Categories.ShowNsfwBones;
		configuration.Keybinds.Enabled = _legacyCfg?.EnableKeybinds ?? configuration.Keybinds.Enabled;
		configuration.Editor.UseToolbar = true;
		configuration.Editor.SelectOnTarget = true;
		configuration.Overlay.DimOverlayForInactiveActors = true;
		LegacyConfig.Configuration? legacyCfg2 = _legacyCfg;
		if (legacyCfg2 != null && legacyCfg2.SavedDirPaths?.Count > 0)
		{
			foreach (KeyValuePair<string, string> item in _legacyCfg?.SavedDirPaths)
			{
				configuration.File.CustomLocations.Add((item.Key, item.Value));
			}
		}
		configuration.Keybinds.BlockTargetLeftClick = _legacyCfg?.DisableChangeTargetOnLeftClick ?? configuration.Keybinds.BlockTargetLeftClick;
		configuration.Keybinds.BlockTargetRightClick = _legacyCfg?.DisableChangeTargetOnRightClick ?? configuration.Keybinds.BlockTargetRightClick;
		configuration.Overlay.DrawLines = _legacyCfg?.DrawLinesOnSkeleton ?? configuration.Overlay.DrawLines;
		configuration.Overlay.DrawLinesGizmo = _legacyCfg?.DrawLinesWithGizmo ?? configuration.Overlay.DrawLinesGizmo;
		configuration.Overlay.DrawDotsGizmo = _legacyCfg?.DrawDotsWithGizmo ?? configuration.Overlay.DrawDotsGizmo;
		configuration.Overlay.LineThickness = _legacyCfg?.SkeletonLineThickness ?? configuration.Overlay.LineThickness;
		configuration.Overlay.LineOpacity = _legacyCfg?.SkeletonLineOpacity ?? configuration.Overlay.LineOpacity;
		configuration.Overlay.LineOpacityUsing = _legacyCfg?.SkeletonLineOpacityWhileUsing ?? configuration.Overlay.LineOpacityUsing;
		configuration.Overlay.DotRadius = _legacyCfg?.SkeletonDotRadius ?? configuration.Overlay.DotRadius;
		configuration.Gizmo.AllowAxisFlip = _legacyCfg?.AllowAxisFlip ?? configuration.Gizmo.AllowAxisFlip;
		configuration.AutoSave.Enabled = _legacyCfg?.EnableAutoSave ?? configuration.AutoSave.Enabled;
		configuration.AutoSave.Interval = _legacyCfg?.AutoSaveInterval ?? configuration.AutoSave.Interval;
		configuration.AutoSave.Count = _legacyCfg?.AutoSaveCount ?? configuration.AutoSave.Count;
		configuration.AutoSave.FilePath = _legacyCfg?.AutoSavePath ?? configuration.AutoSave.FilePath;
		configuration.AutoSave.FolderFormat = _legacyCfg?.AutoSaveFormat ?? configuration.AutoSave.FolderFormat;
		configuration.AutoSave.ClearOnExit = _legacyCfg?.ClearAutoSavesOnExit ?? configuration.AutoSave.ClearOnExit;
		configuration.Editor.WorkcamMoveSpeed = _legacyCfg?.FreecamMoveSpeed ?? configuration.Editor.WorkcamMoveSpeed;
		configuration.Editor.WorkcamSens = _legacyCfg?.FreecamSensitivity ?? configuration.Editor.WorkcamSens;
		configuration.Editor.WorkcamFastMulti = _legacyCfg?.FreecamShiftMuli ?? configuration.Editor.WorkcamFastMulti;
		configuration.Editor.WorkcamSlowMulti = _legacyCfg?.FreecamCtrlMuli ?? configuration.Editor.WorkcamSlowMulti;
		configuration.Editor.WorkcamVertMulti = _legacyCfg?.FreecamUpDownMuli ?? configuration.Editor.WorkcamVertMulti;
		if (_legacyCfg?.FreecamForward != null)
		{
			configuration.Keybinds.SetDefault("Camera_Work_Forward", MigrateKeybind(_legacyCfg?.FreecamForward));
		}
		if (_legacyCfg?.FreecamBack != null)
		{
			configuration.Keybinds.SetDefault("Camera_Work_Back", MigrateKeybind(_legacyCfg?.FreecamBack));
		}
		if (_legacyCfg?.FreecamRight != null)
		{
			configuration.Keybinds.SetDefault("Camera_Work_Right", MigrateKeybind(_legacyCfg?.FreecamRight));
		}
		if (_legacyCfg?.FreecamLeft != null)
		{
			configuration.Keybinds.SetDefault("Camera_Work_Left", MigrateKeybind(_legacyCfg?.FreecamLeft));
		}
		if (_legacyCfg?.FreecamUp != null)
		{
			configuration.Keybinds.SetDefault("Camera_Work_Up", MigrateKeybind(_legacyCfg?.FreecamUp));
		}
		if (_legacyCfg?.FreecamDown != null)
		{
			configuration.Keybinds.SetDefault("Camera_Work_Down", MigrateKeybind(_legacyCfg?.FreecamDown));
		}
		if (_legacyCfg?.FreecamFast != null)
		{
			configuration.Keybinds.SetDefault("Camera_Work_Fast", MigrateKeybind(_legacyCfg?.FreecamFast));
		}
		if (_legacyCfg?.FreecamSlow != null)
		{
			configuration.Keybinds.SetDefault("Camera_Work_Slow", MigrateKeybind(_legacyCfg?.FreecamSlow));
		}
		LegacyConfig.Configuration? legacyCfg3 = _legacyCfg;
		if (legacyCfg3 != null && legacyCfg3.KeyBinds.ContainsKey(LegacyConfig.Input.Purpose.SwitchToTranslate))
		{
			configuration.Keybinds.SetDefault("Gizmo_SetTranslateMode", MigrateKeys(_legacyCfg?.KeyBinds[LegacyConfig.Input.Purpose.SwitchToTranslate]));
		}
		LegacyConfig.Configuration? legacyCfg4 = _legacyCfg;
		if (legacyCfg4 != null && legacyCfg4.KeyBinds.ContainsKey(LegacyConfig.Input.Purpose.SwitchToRotate))
		{
			configuration.Keybinds.SetDefault("Gizmo_SetRotateMode", MigrateKeys(_legacyCfg?.KeyBinds[LegacyConfig.Input.Purpose.SwitchToRotate]));
		}
		LegacyConfig.Configuration? legacyCfg5 = _legacyCfg;
		if (legacyCfg5 != null && legacyCfg5.KeyBinds.ContainsKey(LegacyConfig.Input.Purpose.SwitchToScale))
		{
			configuration.Keybinds.SetDefault("Gizmo_SetScaleMode", MigrateKeys(_legacyCfg?.KeyBinds[LegacyConfig.Input.Purpose.SwitchToScale]));
		}
		LegacyConfig.Configuration? legacyCfg6 = _legacyCfg;
		if (legacyCfg6 != null && legacyCfg6.KeyBinds.ContainsKey(LegacyConfig.Input.Purpose.SwitchToUniversal))
		{
			configuration.Keybinds.SetDefault("Gizmo_SetUniversalMode", MigrateKeys(_legacyCfg?.KeyBinds[LegacyConfig.Input.Purpose.SwitchToUniversal]));
		}
		LegacyConfig.Configuration? legacyCfg7 = _legacyCfg;
		if (legacyCfg7 != null && legacyCfg7.KeyBinds.ContainsKey(LegacyConfig.Input.Purpose.ToggleLocalWorld))
		{
			configuration.Keybinds.SetDefault("Gizmo_ToggleMode", MigrateKeys(_legacyCfg?.KeyBinds[LegacyConfig.Input.Purpose.ToggleLocalWorld]));
		}
		LegacyConfig.Configuration? legacyCfg8 = _legacyCfg;
		if (legacyCfg8 != null && legacyCfg8.KeyBinds.ContainsKey(LegacyConfig.Input.Purpose.CircleThroughSiblingLinkModes))
		{
			configuration.Keybinds.SetDefault("Gizmo_MirrorRotation", MigrateKeys(_legacyCfg?.KeyBinds[LegacyConfig.Input.Purpose.CircleThroughSiblingLinkModes]));
		}
		LegacyConfig.Configuration? legacyCfg9 = _legacyCfg;
		if (legacyCfg9 != null && legacyCfg9.KeyBinds.ContainsKey(LegacyConfig.Input.Purpose.DeselectGizmo))
		{
			configuration.Keybinds.SetDefault("Select_None", MigrateKeys(_legacyCfg?.KeyBinds[LegacyConfig.Input.Purpose.DeselectGizmo]));
		}
		LegacyConfig.Configuration? legacyCfg10 = _legacyCfg;
		if (legacyCfg10 != null && legacyCfg10.KeyBinds.ContainsKey(LegacyConfig.Input.Purpose.NextCamera))
		{
			configuration.Keybinds.SetDefault("Camera_SetNext", MigrateKeys(_legacyCfg?.KeyBinds[LegacyConfig.Input.Purpose.NextCamera]));
		}
		LegacyConfig.Configuration? legacyCfg11 = _legacyCfg;
		if (legacyCfg11 != null && legacyCfg11.KeyBinds.ContainsKey(LegacyConfig.Input.Purpose.PreviousCamera))
		{
			configuration.Keybinds.SetDefault("Camera_SetPrevious", MigrateKeys(_legacyCfg?.KeyBinds[LegacyConfig.Input.Purpose.PreviousCamera]));
		}
		LegacyConfig.Configuration? legacyCfg12 = _legacyCfg;
		if (legacyCfg12 != null && legacyCfg12.KeyBinds.ContainsKey(LegacyConfig.Input.Purpose.ToggleFreeCam))
		{
			configuration.Keybinds.SetDefault("Camera_Work_Toggle", MigrateKeys(_legacyCfg?.KeyBinds[LegacyConfig.Input.Purpose.ToggleFreeCam]));
		}
		if (_legacyCfg?.BoneCategoryColors != null)
		{
			foreach (KeyValuePair<string, Vector4> item2 in _legacyCfg?.BoneCategoryColors)
			{
				if (LegacyCategoryMap.ContainsKey(item2.Key))
				{
					uint boneColor = ImGui.ColorConvertFloat4ToU32(item2.Value);
					BoneCategory? byName = configuration.Categories.GetByName(LegacyCategoryMap[item2.Key]);
					if (byName != null)
					{
						byName.BoneColor = boneColor;
					}
				}
			}
		}
		_cfg.GenerateDefaultPresets(_tempConfig);
		if (_legacyCfg?.CustomBoneOffset != null)
		{
			foreach (KeyValuePair<string, Dictionary<string, Vector3>> item3 in _legacyCfg.CustomBoneOffset)
			{
				try
				{
					string raceSexId = LegacyRaceSexMap[item3.Key];
					configuration.Offsets.LoadLegacy(raceSexId, item3.Value);
				}
				catch (Exception value)
				{
					Ktisis.Log.Warning($"Could not deserialize legacy offsets from clipboard: {value}");
				}
			}
		}
		_tempConfig = configuration;
	}

	private static ActionKeybind MigrateKeybind(LegacyConfig.Keybind keybind)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		ActionKeybind actionKeybind = new ActionKeybind();
		VirtualKey[] keys = keybind.Keys;
		foreach (VirtualKey key in keys)
		{
			if (KeyHelpers.IsModifierKey(key))
			{
				actionKeybind.Combo.AddModifier(key);
			}
			else
			{
				actionKeybind.Combo.Key = key;
			}
		}
		return actionKeybind;
	}

	private static ActionKeybind MigrateKeys(List<VirtualKey> keys)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		ActionKeybind actionKeybind = new ActionKeybind();
		foreach (VirtualKey key in keys)
		{
			if (KeyHelpers.IsModifierKey(key))
			{
				actionKeybind.Combo.AddModifier(key);
			}
			else
			{
				actionKeybind.Combo.Key = key;
			}
		}
		return actionKeybind;
	}

	internal void V3Skip()
	{
		Configuration tempConfig = _tempConfig;
		tempConfig.Keybinds.Enabled = true;
		tempConfig.Editor.ToggleOpenWindows = true;
	}

	public void Begin()
	{
		if (!_confirmed)
		{
			_confirmed = true;
			_gpose.StateChanged -= OnGPoseStateChanged;
			_tempConfig.Version = 12;
			_cfg.File = _tempConfig;
			_cfg._isLoaded = true;
			_cfg.Save();
			_gpose.Reset();
			this.OnConfirmed?.Invoke();
		}
	}
}
