using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using GLib.Widgets;
using Ktisis.Core.Attributes;
using Ktisis.Data.Config;
using Ktisis.Data.Config.Sections;
using Ktisis.Editor.Context;
using Ktisis.Editor.Context.Types;
using Ktisis.Localization;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;
using Newtonsoft.Json;

namespace Ktisis.Interface.Components.Config;

[Transient]
public class OffsetEditor
{
	private readonly ConfigManager _cfg;

	private readonly ContextManager _ctx;

	private readonly LocaleManager _locale;

	private IEditorContext? _editorContext;

	private string? SelectedRaceSexId;

	private OffsetConfig Config => _cfg.File.Offsets;

	private bool HasContext => _editorContext != null;

	public OffsetEditor(ConfigManager cfg, ContextManager ctx, LocaleManager locale)
	{
		_cfg = cfg;
		_ctx = ctx;
		_locale = locale;
	}

	public void Setup()
	{
		UpdateContext();
		if (Config.BoneOffsets == null)
		{
			Config.BoneOffsets = new Dictionary<string, Dictionary<string, Vector3>>();
		}
		if (Config.BoneOffsets.Keys.Count > 0)
		{
			SelectedRaceSexId = Config.BoneOffsets.Keys.OrderBy((string k) => k).First();
		}
	}

	private void UpdateContext()
	{
		_editorContext = _ctx.Current;
		if (HasContext && SelectedRaceSexId == null)
		{
			SetRaceSexIdFromSelection();
		}
	}

	private void SetRaceSexIdFromSelection()
	{
		if (HasContext)
		{
			SceneEntity firstSelected = _editorContext.Selection.GetFirstSelected();
			SceneEntity sceneEntity = ((firstSelected is BoneNode boneNode) ? boneNode.Pose.Parent : ((firstSelected is BoneNodeGroup boneNodeGroup) ? boneNodeGroup.Pose.Parent : ((!(firstSelected is EntityPose entityPose)) ? firstSelected : entityPose.Parent)));
			if (sceneEntity is ActorEntity actorEntity && actorEntity.GetRaceSexId() != null)
			{
				SelectedRaceSexId = actorEntity.GetRaceSexId();
			}
		}
	}

	public void Draw()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		UpdateContext();
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		ImGui.Text(ImU8String.op_Implicit(_locale.Translate("config.offsets.description")));
		ImGui.Text(ImU8String.op_Implicit(_locale.Translate("config.offsets.help")));
		ImGui.Spacing();
		if (ImGui.Button(ImU8String.op_Implicit(_locale.Translate("config.offsets.ui.copy_all")), default(Vector2)))
		{
			Config.SaveToClipboard();
		}
		ImGui.SameLine(0f, x);
		DisabledDisposable val = ImRaii.Disabled(!ImGui.IsKeyDown((ImGuiKey)642) || !ImGui.IsKeyDown((ImGuiKey)641));
		try
		{
			if (ImGui.Button(ImU8String.op_Implicit(_locale.Translate("config.offsets.ui.load_all")), default(Vector2)))
			{
				Config.LoadFromClipboard();
			}
			if (ImGui.IsItemHovered((ImGuiHoveredFlags)128))
			{
				TooltipDisposable val2 = ImRaii.Tooltip();
				try
				{
					ImGui.Text(ImU8String.op_Implicit(_locale.Translate("config.offsets.ui.load_all_warning")));
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
		ImGui.Spacing();
		DrawBoneSelection();
		ImGui.Spacing();
		if (Config.BoneOffsets.Keys.Count >= 1 && SelectedRaceSexId != null)
		{
			ImGui.Separator();
			ImGui.Spacing();
			DrawSkeletonCombo();
			ImGui.Spacing();
			DrawBoneOffsets();
		}
	}

	private void DrawBoneSelection()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		string text = "None";
		string text2 = null;
		string text3 = null;
		if (HasContext && _editorContext.Selection.GetFirstSelected() is BoneNode boneNode)
		{
			EntityPose pose = boneNode.Pose;
			if (pose != null && pose.Parent is ActorEntity actorEntity)
			{
				text2 = boneNode.Info.Name;
				text = boneNode.Name + ((boneNode.Name != text2) ? StringExtensions.Format(" ({0})", new object[1] { text2 }) : "");
				text3 = actorEntity.GetRaceSexId();
				text = text + " on " + ((text3 != null) ? _locale.Translate("config.offsets.race_sex." + text3) : "Invalid");
			}
		}
		DisabledDisposable val = ImRaii.Disabled(text3 == null || text2 == null || (Config.BoneOffsets.ContainsKey(text3) && Config.BoneOffsets[text3].ContainsKey(text2)));
		try
		{
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)61543, _locale.Translate("config.offsets.ui.add_bone")))
			{
				Config.UpsertOffset(text3, text2, default(Vector3));
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		ImGui.SameLine(0f, x);
		val = ImRaii.Disabled(text3 == null || text2 == null || SelectedRaceSexId == text3 || !Config.BoneOffsets.ContainsKey(text3));
		try
		{
			if (text3 == null)
			{
				Buttons.IconButton((FontAwesomeIcon)61550);
			}
			else if (Buttons.IconButtonTooltip((FontAwesomeIcon)61550, "Open offsets for " + _locale.Translate("config.offsets.race_sex." + text3)))
			{
				SelectedRaceSexId = text3;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		ImGui.SameLine(0f, x);
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(2, 2);
		((ImU8String)(ref val2)).AppendFormatted<string>(_locale.Translate("config.offsets.selected"));
		((ImU8String)(ref val2)).AppendLiteral(": ");
		((ImU8String)(ref val2)).AppendFormatted<string>(text);
		ImGui.Text(val2);
	}

	private void DrawSkeletonCombo()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		ComboDisposable val = ImRaii.Combo(ImU8String.op_Implicit("##RaceSexChooser"), ImU8String.op_Implicit(SelectedRaceSexId), (ImGuiComboFlags)64);
		try
		{
			if (val.Success)
			{
				foreach (string item in Config.BoneOffsets.Keys.OrderBy((string k) => k).ToList())
				{
					if (ImGui.Selectable(ImU8String.op_Implicit(_locale.Translate("config.offsets.race_sex." + item)), item == SelectedRaceSexId, (ImGuiSelectableFlags)0, default(Vector2)))
					{
						SelectedRaceSexId = item;
					}
				}
			}
		}
		finally
		{
			((ComboDisposable)(ref val)).Dispose();
		}
		ImGui.SameLine(0f, x);
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(10, 1);
		((ImU8String)(ref val2)).AppendLiteral("Skeleton: ");
		((ImU8String)(ref val2)).AppendFormatted<string>(_locale.Translate("config.offsets.race_sex." + SelectedRaceSexId));
		ImGui.Text(val2);
		string text = _locale.Translate("config.offsets.ui.load_legacy");
		style = ImGui.GetStyle();
		float num = ((ImGuiStylePtr)(ref style)).FramePadding.X * 2f;
		float x2 = ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f).X;
		ImGui.SameLine();
		ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - num - x2 - x - Buttons.CalcSize());
		DisabledDisposable val3 = ImRaii.Disabled(!ImGui.IsKeyDown((ImGuiKey)642) || !ImGui.IsKeyDown((ImGuiKey)641));
		try
		{
			if (ImGui.Button(ImU8String.op_Implicit(text), default(Vector2)))
			{
				Config.LoadLegacyFromClipboard(SelectedRaceSexId);
			}
			if (ImGui.IsItemHovered((ImGuiHoveredFlags)128))
			{
				TooltipDisposable val4 = ImRaii.Tooltip();
				try
				{
					ImU8String val5 = new ImU8String(143, 1);
					((ImU8String)(ref val5)).AppendLiteral("Warning: This will replace ALL current offsets for ");
					((ImU8String)(ref val5)).AppendFormatted<string>(_locale.Translate("config.offsets.race_sex." + SelectedRaceSexId));
					((ImU8String)(ref val5)).AppendLiteral(".\nThis function is only usable with a valid set of v0.2 offsets.\nHold CTRL+Shift to confirm.");
					ImGui.Text(val5);
				}
				finally
				{
					((TooltipDisposable)(ref val4)).Dispose();
				}
			}
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
		ImGui.SameLine(0f, x);
		val3 = ImRaii.Disabled(!ImGui.IsKeyDown((ImGuiKey)642));
		try
		{
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)62189, "Shift+click to clear all offsets for " + _locale.Translate("config.offsets.race_sex." + SelectedRaceSexId)))
			{
				Config.RemoveOffsetsForId(SelectedRaceSexId);
			}
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
	}

	private void DrawBoneOffsets()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		Vector2 cellPadding = ((ImGuiStylePtr)(ref style)).CellPadding;
		ChildDisposable val = ImRaii.Child(ImU8String.op_Implicit("##BoneCategoriesFrame"), ImGui.GetContentRegionAvail() - (_cfg.File.Editor.UseToolbar ? new Vector2(0f, 4f) : Vector2.Zero), true);
		try
		{
			StyleDisposable val2 = ImRaii.PushStyle((ImGuiStyleVar)16, new Vector2(2f, 2f), true);
			try
			{
				TableDisposable val3 = ImRaii.Table(ImU8String.op_Implicit("##BoneOffsetTable"), 5, (ImGuiTableFlags)34688);
				try
				{
					if (!val3.Success)
					{
						return;
					}
					ImGui.TableSetupColumn(ImU8String.op_Implicit("##BoneButtons"), (ImGuiTableColumnFlags)8, 0f, 0u);
					ImGui.TableSetupColumn(ImU8String.op_Implicit("X"), (ImGuiTableColumnFlags)0, 0f, 0u);
					ImGui.TableSetupColumn(ImU8String.op_Implicit("Y"), (ImGuiTableColumnFlags)0, 0f, 0u);
					ImGui.TableSetupColumn(ImU8String.op_Implicit("Z"), (ImGuiTableColumnFlags)0, 0f, 0u);
					ImGui.TableSetupColumn(ImU8String.op_Implicit("Bone Name"), (ImGuiTableColumnFlags)0, 0f, 0u);
					ImGui.TableHeadersRow();
					if (!Config.BoneOffsets.TryGetValue(SelectedRaceSexId, out Dictionary<string, Vector3> value))
					{
						return;
					}
					foreach (KeyValuePair<string, Vector3> item in value.OrderBy((KeyValuePair<string, Vector3> k) => k.Key).ToList())
					{
						var (text2, vec) = (KeyValuePair<string, Vector3>)(ref item);
						if (DrawOffsetRow(text2, ref vec, cellPadding))
						{
							Config.UpsertOffset(SelectedRaceSexId, text2, vec);
						}
					}
				}
				finally
				{
					((TableDisposable)(ref val3)).Dispose();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((ChildDisposable)(ref val)).Dispose();
		}
	}

	private bool DrawOffsetRow(string bone, ref Vector3 vec, Vector2 padding)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		ImGui.TableNextRow();
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(11, 1);
		((ImU8String)(ref val)).AppendLiteral("##");
		((ImU8String)(ref val)).AppendFormatted<string>(bone);
		((ImU8String)(ref val)).AppendLiteral("OffsetRow");
		IdDisposable val2 = ImRaii.PushId(val, true);
		try
		{
			StyleDisposable val3 = ImRaii.PushStyle((ImGuiStyleVar)16, padding, true);
			try
			{
				ImGui.TableNextColumn();
				if (Buttons.IconButtonTooltip((FontAwesomeIcon)61637, _locale.Translate("config.offsets.ui.copy_bone")))
				{
					ImGui.SetClipboardText(ImU8String.op_Implicit(Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((object)vec)))));
				}
				ImGui.SameLine(0f, x);
				if (Buttons.IconButtonTooltip((FontAwesomeIcon)61674, _locale.Translate("config.offsets.ui.load_bone")) && LoadClipboardVector(ref vec))
				{
					flag = true;
				}
				ImGui.SameLine(0f, x);
				DisabledDisposable val4 = ImRaii.Disabled(!ImGui.IsKeyDown((ImGuiKey)642));
				try
				{
					if (Buttons.IconButtonTooltip((FontAwesomeIcon)61944, _locale.Translate("config.offsets.ui.clear_bone")))
					{
						Config.RemoveOffset(SelectedRaceSexId, bone);
						return false;
					}
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
			ImGui.TableNextColumn();
			ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
			flag |= ImGui.DragFloat(ImU8String.op_Implicit("##X"), ref vec.X, 0.001f, 0f, 0f, ImU8String.op_Implicit("%.3f"), (ImGuiSliderFlags)64);
			ImGui.TableNextColumn();
			ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
			flag |= ImGui.DragFloat(ImU8String.op_Implicit("##Y"), ref vec.Y, 0.001f, 0f, 0f, ImU8String.op_Implicit("%.3f"), (ImGuiSliderFlags)64);
			ImGui.TableNextColumn();
			ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
			flag |= ImGui.DragFloat(ImU8String.op_Implicit("##Z"), ref vec.Z, 0.001f, 0f, 0f, ImU8String.op_Implicit("%.3f"), (ImGuiSliderFlags)64);
			ImGui.TableNextColumn();
			string text = bone;
			if (_locale.HasTranslationFor("bone." + bone))
			{
				text = text + " (" + _locale.Translate("bone." + bone) + ")";
			}
			ImGui.Text(ImU8String.op_Implicit(text));
			return flag;
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private bool LoadClipboardVector(ref Vector3 vec)
	{
		try
		{
			vec = JsonConvert.DeserializeObject<Vector3>(Encoding.UTF8.GetString(Convert.FromBase64String(ImGui.GetClipboardText())));
			return true;
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Could not deserialize clipboard vector: {value}");
			return false;
		}
	}
}
