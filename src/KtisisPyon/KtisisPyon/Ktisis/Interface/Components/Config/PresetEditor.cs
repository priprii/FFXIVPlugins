using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Core.Attributes;
using Ktisis.Data.Config;
using Ktisis.Data.Config.Sections;
using Ktisis.Localization;

namespace Ktisis.Interface.Components.Config;

[Transient]
public class PresetEditor
{
	private readonly ConfigManager _cfg;

	private readonly LocaleManager _locale;

	private const uint ColorYellow = 4278255615u;

	private string? Selected;

	private string PresetName;

	private bool IsDefault;

	private PresetConfig Config => _cfg.File.Presets;

	public PresetEditor(ConfigManager cfg, LocaleManager locale)
	{
		_cfg = cfg;
		_locale = locale;
	}

	public void Setup()
	{
		Selected = null;
	}

	public void Draw()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Text(ImU8String.op_Implicit(_locale.Translate("config.presets.description")));
		ImGui.Text(ImU8String.op_Implicit(_locale.Translate("config.presets.defaults")));
		ImGui.Spacing();
		StyleDisposable val = ImRaii.PushStyle((ImGuiStyleVar)16, new Vector2(10f, 10f), true);
		try
		{
			TableDisposable val2 = ImRaii.Table(ImU8String.op_Implicit("##PresetsTable"), 2, (ImGuiTableFlags)1);
			try
			{
				if (val2.Success)
				{
					ImGui.TableSetupColumn(ImU8String.op_Implicit("PresetList"), (ImGuiTableColumnFlags)0, 0f, 0u);
					ImGui.TableSetupColumn(ImU8String.op_Implicit("PresetOptions"), (ImGuiTableColumnFlags)4, 0f, 0u);
					ImGui.TableNextRow();
					DrawPresetList();
					DrawPresetConfig();
				}
			}
			finally
			{
				((TableDisposable)(ref val2)).Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DrawPresetList()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		ImGui.TableNextColumn();
		StyleDisposable val = ImRaii.PushStyle((ImGuiStyleVar)15, UiBuilder.DefaultFontSizePx * ImGuiHelpers.GlobalScale, true);
		try
		{
			foreach (KeyValuePair<string, ImmutableHashSet<string>> preset in Config.Presets)
			{
				preset.Deconstruct(out var key, out var _);
				string text = key;
				ImGuiTreeNodeFlags val2 = (ImGuiTreeNodeFlags)2304;
				bool flag = Config.PresetIsDefault(text);
				if (flag)
				{
					val2 = (ImGuiTreeNodeFlags)(val2 | 0x200);
				}
				if (Selected == text)
				{
					val2 = (ImGuiTreeNodeFlags)(val2 | 1);
				}
				ColorDisposable val3 = ImRaii.PushColor((ImGuiCol)0, flag ? 4278255615u : ImGui.GetColorU32((ImGuiCol)0), true);
				try
				{
					TreeNodeDisposable val4 = ImRaii.TreeNode(ImU8String.op_Implicit(text), val2);
					try
					{
						if (ImGui.IsItemClicked((ImGuiMouseButton)0))
						{
							Selected = ((Selected != text) ? text : null);
							PresetName = text;
							IsDefault = flag;
						}
						else if (ImGui.IsItemClicked((ImGuiMouseButton)1))
						{
							if (flag)
							{
								Config.DefaultPresets.Remove(text);
								IsDefault = false;
							}
							else
							{
								Config.DefaultPresets.Add(text);
								IsDefault = true;
							}
						}
					}
					finally
					{
						((TreeNodeDisposable)(ref val4)).Dispose();
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DrawPresetConfig()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		ImGui.TableNextColumn();
		if (Selected == null)
		{
			return;
		}
		ImGui.InputText(ImU8String.op_Implicit("##Rename"), ref PresetName, 512, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null);
		if (PresetName.Length > 0 && ImGui.IsKeyPressed((ImGuiKey)525) && ImGui.IsItemDeactivated())
		{
			Rename();
		}
		ImGui.SameLine();
		if (ImGui.Button(ImU8String.op_Implicit(_locale.Translate("config.presets.rename")), default(Vector2)))
		{
			Rename();
		}
		DisabledDisposable val = ImRaii.Disabled(!ImGui.IsKeyDown((ImGuiKey)642));
		try
		{
			if (ImGui.Button(ImU8String.op_Implicit(_locale.Translate("config.presets.delete")), default(Vector2)))
			{
				Delete();
			}
			if (ImGui.IsItemHovered((ImGuiHoveredFlags)128))
			{
				TooltipDisposable val2 = ImRaii.Tooltip();
				try
				{
					ImGui.Text(ImU8String.op_Implicit(_locale.Translate("config.presets.delete_tooltip")));
					return;
				}
				finally
				{
					((TooltipDisposable)(ref val2)).Dispose();
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void Rename()
	{
		if (Selected != null && !(Selected == PresetName))
		{
			ImmutableHashSet<string> value = Config.Presets[Selected];
			Config.Presets[PresetName] = value;
			if (IsDefault)
			{
				Config.DefaultPresets.Add(PresetName);
			}
			Config.Presets.Remove(Selected);
			Config.DefaultPresets.Remove(Selected);
			Selected = PresetName;
		}
	}

	private void Delete()
	{
		if (Selected != null)
		{
			PresetConfig.PresetRemovedEvent?.Invoke(Selected);
			Config.Presets.Remove(Selected);
			Config.DefaultPresets.Remove(Selected);
			Selected = null;
		}
	}
}
