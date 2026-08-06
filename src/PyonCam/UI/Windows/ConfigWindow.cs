using System;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using System.Text;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using Newtonsoft.Json;
using PyonCam.Config;
using PyonCam.Config.Cam;
using PyonCam.Extensions;
using PyonCam.Services;

namespace PyonCam.UI.Windows;

public class ConfigWindow : Window
{
	private readonly Configuration _config;

	private readonly IServiceContext _services;

	private readonly IWindowContext _windows;

	private int SelectedPresetIndex = -1;

	private PresetService PresetService => _services.Get<PresetService>();

	private CameraService CameraService => _services.Get<CameraService>();

	private KeybindsWindow KeybindsWindow => _windows.Get<KeybindsWindow>();

	private CameraConfigPreset? SelectedPreset
	{
		get
		{
			if (SelectedPresetIndex < 0 || SelectedPresetIndex >= _config.Presets.Count)
			{
				return null;
			}
			return _config.Presets[SelectedPresetIndex];
		}
	}

	private unsafe uint GameWidth => ((RenderTargetManager)RenderTargetManager.Instance()).Resolution_Width;

	private unsafe uint GameHeight => ((RenderTargetManager)RenderTargetManager.Instance()).Resolution_Height;

	private Vector2 GameResolution => new Vector2(GameWidth, GameHeight);

	public ConfigWindow(Configuration config, IServiceContext services, IWindowContext windows)
		: base($"{"PyonCam"} v{Plugin.Version}")
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		_config = config;
		_services = services;
		_windows = windows;
		WindowSizeConstraints value = default(WindowSizeConstraints);
		((WindowSizeConstraints)(ref value))._002Ector();
		((WindowSizeConstraints)(ref value)).MinimumSize = new Vector2(500f, 300f);
		((WindowSizeConstraints)(ref value)).MaximumSize = GameResolution;
		((Window)this).SizeConstraints = value;
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(600f, 480f) * ImGuiHelpers.GlobalScale;
	}

	public override void OnOpen()
	{
		((Window)this).OnOpen();
		_config.UI.Config.IsOpen = true;
		_config.Save();
	}

	public override void OnClose()
	{
		_config.UI.Config.IsOpen = false;
		_config.Save();
		((Window)this).OnClose();
	}

	public override void Draw()
	{
		if (!((Window)this).IsOpen)
		{
			return;
		}
		try
		{
			if (_config.Presets.Count > 0 && SelectedPresetIndex == -1)
			{
				SelectedPresetIndex = ((PresetService.CurrentPreset != PresetService.DefaultPreset) ? _config.Presets.IndexOf(PresetService.CurrentPreset) : 0);
			}
			DrawHeader();
			DrawPresetList();
		}
		catch (Exception value)
		{
			_services.Log.Warning($"{value}", Array.Empty<object>());
		}
	}

	private void DrawHeader()
	{
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0580: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0624: Unknown result type (might be due to invalid IL or missing references)
		//IL_065c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0694: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cc: Unknown result type (might be due to invalid IL or missing references)
		bool flag = SelectedPreset != null;
		if (ImGuiEx.IconButton((FontAwesomeIcon)61525))
		{
			CameraConfigPreset cameraConfigPreset = new CameraConfigPreset();
			_config.Presets.Add(cameraConfigPreset);
			if (_config.Presets.Count == 1)
			{
				_config.SelectedPresetID = cameraConfigPreset.ID;
				PresetService.CurrentPreset = cameraConfigPreset;
			}
			_config.Save();
		}
		ImGuiEx.SetItemTooltip("Create new preset.", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		if (ImGuiEx.IconButton((FontAwesomeIcon)61610) && flag)
		{
			CameraConfigPreset selectedPreset = SelectedPreset;
			_config.Presets.RemoveAt(SelectedPresetIndex);
			SelectedPresetIndex = Math.Max(SelectedPresetIndex - 1, 0);
			_config.Presets.Insert(SelectedPresetIndex, selectedPreset);
			_config.Save();
		}
		ImGuiEx.SetItemTooltip("Move selected preset up.", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		if (ImGuiEx.IconButton((FontAwesomeIcon)61611) && flag)
		{
			CameraConfigPreset selectedPreset2 = SelectedPreset;
			_config.Presets.RemoveAt(SelectedPresetIndex);
			SelectedPresetIndex = Math.Min(SelectedPresetIndex + 1, _config.Presets.Count);
			_config.Presets.Insert(SelectedPresetIndex, selectedPreset2);
			_config.Save();
		}
		ImGuiEx.SetItemTooltip("Move selected preset down.", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		if (ImGuiEx.IconButton((FontAwesomeIcon)61637) && flag)
		{
			ImGui.SetClipboardText(ImU8String.op_Implicit(CompressToBase64(JsonConvert.SerializeObject((object)SelectedPreset))));
		}
		ImGuiEx.SetItemTooltip("Copy the selected preset to clipboard.", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		if (ImGuiEx.IconButton((FontAwesomeIcon)61674) && TryImportObject<CameraConfigPreset>(ImGui.GetClipboardText().Trim(), out CameraConfigPreset result) && result != null)
		{
			result.ID = Guid.NewGuid();
			_config.Presets.Add(result);
			_config.Save();
		}
		ImGuiEx.SetItemTooltip("Paste preset from clipboard.", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		ImGuiEx.IconButton((FontAwesomeIcon)62189);
		ImGuiEx.SetItemTooltip("Remove selected preset.", (ImGuiHoveredFlags)0);
		if (flag && ImGui.BeginPopupContextItem(ImU8String.op_Implicit("##removePreset"), (ImGuiPopupFlags)0))
		{
			if (ImGuiEx.IconSelectable((FontAwesomeIcon)62189))
			{
				bool num = _config.Presets[SelectedPresetIndex] == PresetService.ActivePreset;
				_config.Presets.RemoveAt(SelectedPresetIndex);
				SelectedPresetIndex = Math.Min(SelectedPresetIndex, _config.Presets.Count - 1);
				_config.Save();
				if (num)
				{
					_config.SelectedPresetID = Guid.Empty;
					PresetService.CurrentPreset = PresetService.DefaultPreset;
					_config.Save();
				}
			}
			ImGui.EndPopup();
		}
		ImGui.SameLine();
		ImGuiEx.IconTextUnformatted((FontAwesomeIcon)61530);
		ImGuiEx.SetItemTooltip("Ctrl+LClick sliders to input values manually.", (ImGuiHoveredFlags)0);
		ImGui.SameLine(0f, 4f);
		if (ImGuiEx.ColorIconButton((FontAwesomeIcon)((_config.DeathCamMode == DeathCamSetting.Disabled) ? 61534 : ((_config.DeathCamMode == DeathCamSetting.Spectate) ? 58675 : 61571)), "deathCam", (_config.DeathCamMode == DeathCamSetting.Disabled) ? 4282664157u : 4282703172u))
		{
			if (_config.DeathCamMode == DeathCamSetting.Disabled)
			{
				_config.DeathCamMode = DeathCamSetting.Spectate;
			}
			else if (_config.DeathCamMode == DeathCamSetting.Spectate)
			{
				_config.DeathCamMode = DeathCamSetting.FreeCam;
			}
			else
			{
				_config.DeathCamMode = DeathCamSetting.Disabled;
			}
			_config.Save();
		}
		if (_config.DeathCamMode == DeathCamSetting.Disabled)
		{
			ImGuiEx.SetItemTooltip("DeathCam: Disabled", (ImGuiHoveredFlags)0);
		}
		else if (_config.DeathCamMode == DeathCamSetting.Spectate)
		{
			ImGuiEx.SetItemTooltip("DeathCam: Spectate", (ImGuiHoveredFlags)0);
		}
		else
		{
			ImGuiEx.SetItemTooltip("DeathCam: FreeCam", (ImGuiHoveredFlags)0);
		}
		ImGui.SameLine();
		if (ImGuiEx.ColorIconButton((FontAwesomeIcon)61550, "camSpectate", CameraService.SpectatingEnabled ? 4282703172u : 4282664157u))
		{
			CameraService.SpectatingEnabled = !CameraService.SpectatingEnabled;
		}
		ImGuiEx.SetItemTooltip("Spectate Focus/Soft Target: " + (CameraService.SpectatingEnabled ? "Enabled" : "Disabled"), (ImGuiHoveredFlags)0);
		if (CameraService.NoClipValid)
		{
			ImGui.SameLine();
			if (ImGuiEx.ColorIconButton((FontAwesomeIcon)62480, "camCollide", _config.EnableCameraNoClippy ? 4282664157u : 4282703172u))
			{
				_config.EnableCameraNoClippy = !_config.EnableCameraNoClippy;
				if (!CameraService.FreeCam.Enabled)
				{
					CameraService.ToggleNoClip();
				}
				_config.Save();
			}
			ImGuiEx.SetItemTooltip("Camera Collision: " + (_config.EnableCameraNoClippy ? "Disabled" : "Enabled"), (ImGuiHoveredFlags)0);
		}
		ImGui.SameLine();
		if (ImGuiEx.ColorIconButton((FontAwesomeIcon)61488, "freeCam", CameraService.FreeCam.Enabled ? 4282703172u : 4282664157u))
		{
			CameraService.FreeCam.Toggle();
		}
		string text = "Keybinds" + $"\nMove: {_config.Keybinds[KeybindInput.Forward]}/{_config.Keybinds[KeybindInput.Left]}/{_config.Keybinds[KeybindInput.Back]}/{_config.Keybinds[KeybindInput.Right]}" + $"\nAscend: {_config.Keybinds[KeybindInput.Ascend]}" + $"\nDescend: {_config.Keybinds[KeybindInput.Descend]}" + $"\nSlow_Speed: {_config.Keybinds[KeybindInput.Slow_Speed]}" + $"\nFast_Speed: {_config.Keybinds[KeybindInput.Fast_Speed]}" + "\nAdjust_Speed: SCROLLWHEEL\nPan_Camera: LCLICK/RCLICK\nExit_FreeCam: ESCAPE";
		ImGuiEx.SetItemTooltip("Toggle FreeCam\n(Can also toggle with /freecam command)\n\n" + text, (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		if (ImGuiEx.ColorIconButton((FontAwesomeIcon)61459, "freecamKeybinds", ((Window)KeybindsWindow).IsOpen ? 4282703172u : 4282664157u))
		{
			KeybindsWindow keybindsWindow = KeybindsWindow;
			((Window)keybindsWindow).IsOpen = !((Window)keybindsWindow).IsOpen;
		}
		ImGuiEx.SetItemTooltip("Configure Freecam Keybinds", (ImGuiHoveredFlags)0);
	}

	private static string CompressToBase64(string input)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(input);
		using MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
		{
			gZipStream.Write(bytes, 0, bytes.Length);
		}
		return Convert.ToBase64String(memoryStream.ToArray());
	}

	private static string DecompressFromBase64(string base64)
	{
		using MemoryStream stream = new MemoryStream(Convert.FromBase64String(base64));
		using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
		using MemoryStream memoryStream = new MemoryStream();
		gZipStream.CopyTo(memoryStream);
		return Encoding.UTF8.GetString(memoryStream.ToArray());
	}

	private static bool TryImportObject<CameraConfigPreset>(string base64, out CameraConfigPreset? result)
	{
		result = default(CameraConfigPreset);
		if (string.IsNullOrWhiteSpace(base64))
		{
			return false;
		}
		string text;
		try
		{
			text = DecompressFromBase64(base64);
		}
		catch
		{
			return false;
		}
		try
		{
			CameraConfigPreset val = JsonConvert.DeserializeObject<CameraConfigPreset>(text);
			if (val == null)
			{
				return false;
			}
			if (val != null)
			{
				result = val;
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void DrawPresetList()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		bool flag = SelectedPreset != null;
		ImGui.BeginChild(ImU8String.op_Implicit("PyonCamPresetList"), new Vector2(140f * ImGuiHelpers.GlobalScale, 0f), true, (ImGuiWindowFlags)0);
		Vector4 dalamudViolet = ImGuiColors.DalamudViolet;
		ImGui.TextColored(ref dalamudViolet, ImU8String.op_Implicit("Presets"));
		ImGui.SameLine();
		ImGuiEx.IconTextUnformatted((FontAwesomeIcon)61529);
		ImGuiEx.SetItemTooltip("Toggle activating a preset by double-clicking it.\nThe active preset will be applied on login.\nIf no preset is active, the default camera properties will be restored.", (ImGuiHoveredFlags)0);
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		if (_config.Presets.Count == 0)
		{
			dalamudViolet = ImGuiColors.DalamudRed;
			ImU8String val = default(ImU8String);
			((ImU8String)(ref val))._002Ector(20, 0);
			((ImU8String)(ref val)).AppendLiteral("No Presets Available");
			ImGui.TextColored(ref dalamudViolet, val);
			ImGuiEx.SetItemTooltip("Click the + button above to create a new preset.", (ImGuiHoveredFlags)0);
		}
		else
		{
			for (int i = 0; i < _config.Presets.Count; i++)
			{
				ImGui.PushID((IntPtr)i);
				CameraConfigPreset cameraConfigPreset = _config.Presets[i];
				bool flag2 = cameraConfigPreset == PresetService.ActivePreset;
				ImGui.PushStyleColor((ImGuiCol)0, flag2 ? 4282711876u : 4289374890u);
				if (ImGui.Selectable(ImU8String.op_Implicit(cameraConfigPreset.Name), SelectedPresetIndex == i, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					SelectedPresetIndex = i;
				}
				ImGui.PopStyleColor();
				if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked((ImGuiMouseButton)0))
				{
					if (!flag2)
					{
						_config.SelectedPresetID = cameraConfigPreset.ID;
						PresetService.CurrentPreset = cameraConfigPreset;
					}
					else
					{
						_config.SelectedPresetID = Guid.Empty;
						PresetService.CurrentPreset = PresetService.DefaultPreset;
					}
					_config.Save();
				}
				ImGui.PopID();
			}
		}
		ImGui.EndChild();
		if (flag)
		{
			ImGui.SameLine();
			ImGui.BeginChild(ImU8String.op_Implicit("PyonCamPresetEditor"), Vector2.Zero, true, (ImGuiWindowFlags)0);
			DrawPresetEditor(SelectedPreset);
			ImGui.EndChild();
		}
	}

	private void DrawPresetEditor(CameraConfigPreset preset)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_0455: Unknown result type (might be due to invalid IL or missing references)
		bool flag = preset == PresetService.CurrentPreset;
		if (ImGui.Checkbox(ImU8String.op_Implicit("##enablePreset"), ref flag))
		{
			if (flag)
			{
				_config.SelectedPresetID = preset.ID;
				PresetService.CurrentPreset = preset;
			}
			else
			{
				_config.SelectedPresetID = Guid.Empty;
				PresetService.CurrentPreset = PresetService.DefaultPreset;
			}
			_config.Save();
		}
		ImGuiEx.SetItemTooltip("Enable this preset", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		ImGuiIOPtr iO = ImGui.GetIO();
		ImGui.SetNextItemWidth(120f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
		if (ImGui.InputTextWithHint(ImU8String.op_Implicit("##presetName"), ImU8String.op_Implicit("Preset Name"), ref preset.Name, 64, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null))
		{
			_config.Save();
		}
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		if (ImGuiEx.TreeNode("Zoom Properties", "##zoomProperties", default(Vector4), null, (ImGuiTreeNodeFlags)32))
		{
			ResetDragFloat("Minimum##Zoom", ref preset.MinZoom, 0.1f, 0.7f, preset.MaxZoom, PresetService.DefaultPreset.MinZoom, "%.2f");
			ResetDragFloat("Maximum##Zoom", ref preset.MaxZoom, 0.1f, preset.MinZoom, 999f, PresetService.DefaultPreset.MaxZoom, "%.2f");
			ResetDragFloat("Delta##Zoom", ref preset.ZoomDelta, 0.01f, 0f, 10f, PresetService.DefaultPreset.ZoomDelta, "%.2f");
			ImGui.TreePop();
		}
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		if (ImGuiEx.TreeNode("FoV Properties", "##fovProperties", default(Vector4), null, (ImGuiTreeNodeFlags)32))
		{
			ResetDragFloat("Minimum##FoV", ref preset.MinFoV, 0.001f, 0.01f, preset.MaxFoV, PresetService.DefaultPreset.MinFoV, "%.2f");
			ResetDragFloat("Maximum##FoV", ref preset.MaxFoV, 0.001f, preset.MinFoV, 3f, PresetService.DefaultPreset.MaxFoV, "%.2f");
			ResetDragFloat("Delta##FoV", ref preset.FoVDelta, 0.001f, 0f, 0.5f, PresetService.DefaultPreset.FoVDelta, "%.2f");
			ImGui.TreePop();
		}
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		if (ImGuiEx.TreeNode("Offset Properties", "##offsetProperties", default(Vector4), null, (ImGuiTreeNodeFlags)32))
		{
			ResetDragFloat("Min VRotation", ref preset.MinVRotation, 0.01f, -1.569f, preset.MaxVRotation, PresetService.DefaultPreset.MinVRotation, "%.2f");
			ResetDragFloat("Max VRotation", ref preset.MaxVRotation, 0.01f, preset.MinVRotation, 1.569f, PresetService.DefaultPreset.MaxVRotation, "%.2f");
			ResetDragFloat("Height Offset", ref preset.HeightOffset, 0.001f, -1f, 1f, PresetService.DefaultPreset.HeightOffset, "%.3f");
			ResetDragFloat("Side Offset", ref preset.SideOffset, 0.001f, -1f, 1f, PresetService.DefaultPreset.SideOffset, "%.3f");
			ResetDragFloat("Tilt", ref preset.Tilt, 0.01f, -(float)Math.PI, (float)Math.PI, PresetService.DefaultPreset.Tilt, "%.2f");
			ResetDragFloat("LookAt Offset", ref preset.LookAtHeightOffset, 0.001f, -10f, 10f, CameraService.GetDefaultLookAtHeightOffset().GetValueOrDefault(), "%.3f");
			ImGui.TreePop();
		}
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		if (ImGuiEx.TreeNode("PoV Properties", "##povProperties", default(Vector4), null, (ImGuiTreeNodeFlags)32))
		{
			if (ImGui.Checkbox(ImU8String.op_Implicit("Enable PoV##enablePoV"), ref preset.EnablePoV))
			{
				_config.Save();
			}
			ImGuiEx.SetItemTooltip("When in 1st person view:\n- Enables rendering of your character.\n- Camera follows character's head position.", (ImGuiHoveredFlags)0);
			ImGui.SameLine();
			if (ImGui.Checkbox(ImU8String.op_Implicit("Track Rotation##trackRotation"), ref preset.PoVRotation))
			{
				_config.Save();
			}
			ImGuiEx.SetItemTooltip("Camera follows head rotation.", (ImGuiHoveredFlags)0);
			ImGui.SameLine();
			ImGuiEx.IconTextUnformatted((FontAwesomeIcon)61530);
			ImGuiEx.SetItemTooltip("Character Configuration > Control Settings > General\nEnable 'Switch to 1st person view when fully zoomed in.'\nSet '1st Person Camera Auto-adjustment' to 'Only When Moving'/'Always' to have camera follow when turning.", (ImGuiHoveredFlags)0);
			ResetDragFloat("FoV", ref preset.PoVFoV, 0.01f, 0.01f, 3f, PresetService.DefaultPreset.PoVFoV, "%.2f");
			ResetDragFloat("Min VRotation##povMinV", ref preset.PoVMinVRotation, 0.01f, -1.569f, preset.PoVMaxVRotation, PresetService.DefaultPreset.PoVMinVRotation, "%.2f");
			ResetDragFloat("Max VRotation##povMaxV", ref preset.PoVMaxVRotation, 0.01f, preset.PoVMinVRotation, 1.569f, PresetService.DefaultPreset.PoVMaxVRotation, "%.2f");
			ResetDragFloat("Height Offset##povHeightOffset", ref preset.PoVHeightOffset, 0.001f, -1f, 1f, PresetService.DefaultPreset.PoVHeightOffset, "%.3f");
			ResetDragFloat("Forward Offset", ref preset.PoVForwardOffset, 0.001f, -1f, 1f, PresetService.DefaultPreset.PoVForwardOffset, "%.3f");
			ResetDragFloat("Side Offset##povSideOffset", ref preset.PoVSideOffset, 0.001f, -1f, 1f, PresetService.DefaultPreset.PoVSideOffset, "%.3f");
			ImGui.TreePop();
		}
	}

	private void ResetDragFloat(string id, ref float val, float speed, float min, float max, float reset, string format)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		ImGui.PushFont(UiBuilder.IconFont);
		ImGui.BeginDisabled(val == reset);
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(2, 2);
		((ImU8String)(ref val2)).AppendFormatted<string>(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)62186));
		((ImU8String)(ref val2)).AppendLiteral("##");
		((ImU8String)(ref val2)).AppendFormatted<string>(id);
		if (ImGui.Button(val2, default(Vector2)))
		{
			val = reset;
			flag = true;
		}
		ImGui.EndDisabled();
		ImGui.PopFont();
		ImGuiEx.SetItemTooltip("Restore Default", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 150f * ImGuiHelpers.GlobalScale);
		flag |= ImGui.DragFloat(ImU8String.op_Implicit(id), ref val, speed, min, max, ImU8String.op_Implicit(format), (ImGuiSliderFlags)0);
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 itemRectMin = ImGui.GetItemRectMin();
		Vector2 itemRectMax = ImGui.GetItemRectMax();
		float num = 0f;
		if (!string.IsNullOrEmpty(id))
		{
			float x = ImGui.CalcTextSize(ImU8String.op_Implicit(id.Contains("##") ? id.Split("##")[0] : id), false, -1f).X;
			ImGuiStylePtr style = ImGui.GetStyle();
			num = x + ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		}
		Vector2 vector = new Vector2(itemRectMin.X, itemRectMin.Y);
		Vector2 vector2 = new Vector2(itemRectMax.X - num, itemRectMax.Y);
		float value = (val - min) / (max - min);
		value = Math.Clamp(value, 0f, 1f);
		float num2 = 2f * ImGuiHelpers.GlobalScale;
		float num3 = 1f * ImGuiHelpers.GlobalScale;
		Vector2 vector3 = new Vector2(vector.X + num3, vector2.Y - num2 - num3);
		Vector2 vector4 = new Vector2(vector2.X - num3, vector2.Y - num3);
		Vector2 vector5 = new Vector2(vector3.X + (vector4.X - vector3.X) * value, vector4.Y);
		uint colorU = ImGui.GetColorU32((ImGuiCol)7);
		uint colorU2 = ImGui.GetColorU32((ImGuiCol)20);
		((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(vector3, vector4, colorU, 2f);
		((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(vector3, vector5, colorU2, 2f);
		if (flag)
		{
			_config.Save();
			if (SelectedPreset == PresetService.CurrentPreset)
			{
				CameraService.ApplyPreset(SelectedPreset);
			}
		}
	}

	private void ResetDragFloat(string id, ref float val, float speed, float min, float max, Func<float> reset, string format)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		ImGui.PushFont(UiBuilder.IconFont);
		float num = reset();
		ImGui.BeginDisabled(float.Round(val, 6) - float.Round(num, 6) == 0f);
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(2, 2);
		((ImU8String)(ref val2)).AppendFormatted<string>(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)62186));
		((ImU8String)(ref val2)).AppendLiteral("##");
		((ImU8String)(ref val2)).AppendFormatted<string>(id);
		if (ImGui.Button(val2, default(Vector2)))
		{
			val = num;
			flag = true;
		}
		ImGui.EndDisabled();
		ImGui.PopFont();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 150f * ImGuiHelpers.GlobalScale);
		flag |= ImGui.DragFloat(ImU8String.op_Implicit(id), ref val, speed, min, max, ImU8String.op_Implicit(format), (ImGuiSliderFlags)0);
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 itemRectMin = ImGui.GetItemRectMin();
		Vector2 itemRectMax = ImGui.GetItemRectMax();
		float num2 = 0f;
		if (!string.IsNullOrEmpty(id))
		{
			float x = ImGui.CalcTextSize(ImU8String.op_Implicit(id.Contains("##") ? id.Split("##")[0] : id), false, -1f).X;
			ImGuiStylePtr style = ImGui.GetStyle();
			num2 = x + ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		}
		Vector2 vector = new Vector2(itemRectMin.X, itemRectMin.Y);
		Vector2 vector2 = new Vector2(itemRectMax.X - num2, itemRectMax.Y);
		float value = (val - min) / (max - min);
		value = Math.Clamp(value, 0f, 1f);
		float num3 = 2f * ImGuiHelpers.GlobalScale;
		float num4 = 1f * ImGuiHelpers.GlobalScale;
		Vector2 vector3 = new Vector2(vector.X + num4, vector2.Y - num3 - num4);
		Vector2 vector4 = new Vector2(vector2.X - num4, vector2.Y - num4);
		Vector2 vector5 = new Vector2(vector3.X + (vector4.X - vector3.X) * value, vector4.Y);
		uint colorU = ImGui.GetColorU32((ImGuiCol)7);
		uint colorU2 = ImGui.GetColorU32((ImGuiCol)20);
		((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(vector3, vector4, colorU, 2f);
		((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(vector3, vector5, colorU2, 2f);
		if (flag)
		{
			_config.Save();
			if (SelectedPreset == PresetService.CurrentPreset)
			{
				CameraService.ApplyPreset(SelectedPreset);
			}
		}
	}
}
