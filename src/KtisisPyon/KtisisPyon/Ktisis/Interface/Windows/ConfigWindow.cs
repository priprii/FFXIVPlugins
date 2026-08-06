using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using GLib.Widgets;
using Ktisis.Common.Utility;
using Ktisis.Data.Config;
using Ktisis.Data.Config.Sections;
using Ktisis.Editor.Context;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Components.Config;
using Ktisis.Interface.Types;
using Ktisis.Interface.Windows.ToolbarModules;
using Ktisis.Localization;
using Ktisis.Services.Data;
using Ktisis.Structs.Objects;

namespace Ktisis.Interface.Windows;

public class ConfigWindow : KtisisWindow
{
	private delegate void DrawContentDelegate();

	private readonly ConfigManager _cfg;

	private readonly GuiManager _gui;

	private readonly ContextManager _context;

	private readonly FormatService _format;

	private readonly ActionKeybindEditor _keybinds;

	private readonly BoneCategoryEditor _boneCategories;

	private readonly GizmoStyleEditor _gizmoStyle;

	private readonly PresetEditor _presetEditor;

	private readonly OffsetEditor _offsetEditor;

	public readonly LocaleManager Locale;

	private const ImGuiInputTextFlags inputFlags = (ImGuiInputTextFlags)16400;

	private int _tabIndex;

	private int resW;

	private int resH;

	private Configuration Config => _cfg.File;

	private IReadOnlyList<(string, DrawContentDelegate?)> Tabs { get; }

	public ConfigWindow(ConfigManager cfg, ContextManager context, FormatService format, ActionKeybindEditor keybinds, BoneCategoryEditor boneCategories, GizmoStyleEditor gizmoStyle, PresetEditor presetEditor, OffsetEditor offsetEditor, LocaleManager locale, GuiManager gui)
		: base("config.title", (ImGuiWindowFlags)24, "###KtisisConfig")
	{
		_cfg = cfg;
		_context = context;
		_format = format;
		_keybinds = keybinds;
		_boneCategories = boneCategories;
		_gizmoStyle = gizmoStyle;
		_presetEditor = presetEditor;
		_offsetEditor = offsetEditor;
		Locale = locale;
		_gui = gui;
		Tabs = new _003C_003Ez__ReadOnlyArray<(string, DrawContentDelegate)>(new(string, DrawContentDelegate)[15]
		{
			("config.workspace.title", DrawWorkspaceTab),
			("config.autosave.title", DrawAutoSaveTab),
			("config.categories.title", DrawCategoriesTab),
			("config.gizmo.title", DrawGizmoTab),
			("config.overlay.title", DrawOverlayTab),
			("config.offsets.title", DrawOffsetsTab),
			("config.poseview.title", DrawPoseViewTab),
			("config.input.title", DrawInputTab),
			("config.input.cameras.title", DrawCamerasInputTab),
			("config.input.gizmo.title", DrawGizmoInputTab),
			("config.input.toolbar.title", DrawToolbarInputTab),
			("Output", DrawOutputTab),
			("config.misc.title", null),
			("config.language.title", DrawLanguageTab),
			("config.about.title", DrawAboutTab)
		});
	}

	public override void OnOpen()
	{
		if (_cfg.GetConfigFileExists())
		{
			_keybinds.Setup();
			_boneCategories.Setup();
			_presetEditor.Setup();
			_offsetEditor.Setup();
		}
	}

	private void DrawTabNode(int index, List<int>? children = null, int? parentIndex = null)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		bool num = children != null;
		if (num)
		{
			Separators.SeparatorText(ImU8String.op_Implicit(Locale.Translate(Tabs[index].Item1)), 0u, 0.3f, 5f, Separators.LineHeight.Bottom, ImGui.GetColorU32((ImGuiCol)1));
		}
		string text = "##";
		text = ((!parentIndex.HasValue) ? (text + Tabs[index].Item1) : (text + Tabs[parentIndex.Value].Item1));
		if (Tabs[index].Item2 != null && ImGui.Selectable(ImU8String.op_Implicit(Locale.Translate(Tabs[index].Item1) + text), _tabIndex == index, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			_tabIndex = index;
		}
		if (!num)
		{
			return;
		}
		foreach (int child in children)
		{
			DrawTabNode(child, null, index);
		}
	}

	public override void Draw()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		ChildDisposable val = ImRaii.Child(ImU8String.op_Implicit("##nav"), new Vector2(150f * ImGuiHelpers.GlobalScale, Math.Clamp(ImGui.GetContentRegionAvail().Y - 0.1f, 400f * ImGuiHelpers.GlobalScale, float.MaxValue)));
		try
		{
			int num = 1;
			List<int> list = new List<int>(num);
			CollectionsMarshal.SetCount(list, num);
			CollectionsMarshal.AsSpan(list)[0] = 1;
			DrawTabNode(0, list);
			num = 4;
			List<int> list2 = new List<int>(num);
			CollectionsMarshal.SetCount(list2, num);
			Span<int> span = CollectionsMarshal.AsSpan(list2);
			span[0] = 3;
			span[1] = 4;
			span[2] = 5;
			span[3] = 6;
			DrawTabNode(2, list2);
			num = 3;
			List<int> list3 = new List<int>(num);
			CollectionsMarshal.SetCount(list3, num);
			Span<int> span2 = CollectionsMarshal.AsSpan(list3);
			span2[0] = 8;
			span2[1] = 9;
			span2[2] = 10;
			DrawTabNode(7, list3);
			num = 2;
			List<int> list4 = new List<int>(num);
			CollectionsMarshal.SetCount(list4, num);
			Span<int> span3 = CollectionsMarshal.AsSpan(list4);
			span3[0] = 12;
			span3[1] = 13;
			DrawTabNode(11, list4);
		}
		finally
		{
			((ChildDisposable)(ref val)).Dispose();
		}
		ImGui.SameLine();
		GroupDisposable val2 = ImRaii.Group();
		try
		{
			Tabs[_tabIndex].Item2();
		}
		finally
		{
			((GroupDisposable)(ref val2)).Dispose();
		}
	}

	private void DrawHint(string localeHandle)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		ImGui.SameLine();
		Icons.DrawIcon((FontAwesomeIcon)61529);
		if (ImGui.IsItemHovered())
		{
			TooltipDisposable val = ImRaii.Tooltip();
			try
			{
				ImGui.Text(ImU8String.op_Implicit(Locale.Translate(localeHandle)));
			}
			finally
			{
				((TooltipDisposable)(ref val)).Dispose();
			}
		}
	}

	private void DrawCategoriesTab()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("config.categories.header")));
		ImGui.Spacing();
		int num = 0 | (ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.categories.show_all_viera_ears")), ref Config.Categories.ShowAllVieraEars) ? 1 : 0);
		DrawHint("config.categories.hint_viera_ears");
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.categories.show_friendly_bone_names")), ref Config.Categories.ShowFriendlyBoneNames);
		DrawHint("config.categories.hint_friendly_bones");
		if (num != 0)
		{
			RefreshScene();
		}
		ImGui.Spacing();
		_boneCategories.Draw();
	}

	private void DrawGizmoTab()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("config.gizmo.header")));
		ImGui.Spacing();
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.gizmo.flip")), ref Config.Gizmo.AllowAxisFlip);
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.gizmo.raySnap")), ref Config.Gizmo.AllowRaySnap);
		DrawHint("config.gizmo.rayHint");
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.gizmo.holdSnap")), ref Config.Gizmo.AllowHoldSnap);
		DrawHint("config.gizmo.hintHoldSnap");
		ImGui.SliderFloat(ImU8String.op_Implicit(Locale.Translate("config.gizmo.2d_scale")), ref Config.Gizmo.Gizmo2DScaleFactor, 0.4f, 0.75f, ImU8String.op_Implicit("%.2f"), (ImGuiSliderFlags)16);
		ImGui.Spacing();
		_gizmoStyle.Draw();
	}

	private void DrawOverlayTab()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0484: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0552: Unknown result type (might be due to invalid IL or missing references)
		//IL_056c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0571: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0578: Unknown result type (might be due to invalid IL or missing references)
		//IL_0599: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("config.overlay.header")));
		ImGui.Spacing();
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.overlay.lines.draw")), ref Config.Overlay.DrawLines);
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.overlay.lines.draw_gizmo")), ref Config.Overlay.DrawLinesGizmo);
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.overlay.dots.draw_gizmo")), ref Config.Overlay.DrawDotsGizmo);
		ImGui.Spacing();
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.references.draw_title")), ref Config.Overlay.DrawReferenceTitle);
		ImGui.Spacing();
		ImGui.DragFloat(ImU8String.op_Implicit(Locale.Translate("config.overlay.dots.radius")), ref Config.Overlay.DotRadius, 0.1f, 0f, 0f, default(ImU8String), (ImGuiSliderFlags)0);
		ImGui.DragFloat(ImU8String.op_Implicit(Locale.Translate("config.overlay.lines.thick")), ref Config.Overlay.LineThickness, 0.1f, 0f, 0f, default(ImU8String), (ImGuiSliderFlags)0);
		ImGui.Spacing();
		ImGui.SliderFloat(ImU8String.op_Implicit(Locale.Translate("config.overlay.lines.opacity")), ref Config.Overlay.LineOpacity, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0);
		ImGui.SliderFloat(ImU8String.op_Implicit(Locale.Translate("config.overlay.lines.opacity_gizmo")), ref Config.Overlay.LineOpacityUsing, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0);
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		ComboDisposable val = ImRaii.Combo(ImU8String.op_Implicit(Locale.Translate("config.overlay.active_state_chooser")), ImU8String.op_Implicit(Config.Overlay.ActiveStateType.ToString()));
		try
		{
			if (val.Success)
			{
				ActiveState[] values = Enum.GetValues<ActiveState>();
				for (int i = 0; i < values.Length; i++)
				{
					ActiveState activeState = values[i];
					if (ImGui.Selectable(ImU8String.op_Implicit(activeState.ToString()), activeState == Config.Overlay.ActiveStateType, (ImGuiSelectableFlags)0, default(Vector2)))
					{
						Config.Overlay.ActiveStateType = activeState;
					}
				}
			}
		}
		finally
		{
			((ComboDisposable)(ref val)).Dispose();
		}
		ImGui.Spacing();
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.overlay.dim_inactive")), ref Config.Overlay.DimOverlayForInactiveActors);
		if (Config.Overlay.DimOverlayForInactiveActors)
		{
			ImGui.SliderFloat(ImU8String.op_Implicit(Locale.Translate("config.overlay.inactive_opacity")), ref Config.Overlay.InactiveOpacity, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0);
		}
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		ColorPicker4("Default line color", "colDefaultLine", ref Config.Overlay.DefaultLineColor);
		DrawHint("Group/Bone line colors on the Categories tab will override this.");
		ImGui.Checkbox(ImU8String.op_Implicit("Color selected bone parent lines"), ref Config.Overlay.ColorSelectedBoneParentLine);
		ImGui.SameLine();
		ColorPicker4("", "colBoneParentLine", ref Config.Overlay.SelectedBoneParentLineColor);
		ImGui.Checkbox(ImU8String.op_Implicit("Color selected bone descendant lines"), ref Config.Overlay.ColorSelectedBoneDescendantLine);
		ImGui.SameLine();
		ColorPicker4("", "colBoneDescLine", ref Config.Overlay.SelectedBoneDescendantLineColor);
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		ImGui.DragFloat(ImU8String.op_Implicit(Locale.Translate("config.overlay.world.dot_radius")), ref Config.Overlay.WorldNodeRadius, 0.1f, 0f, 0f, default(ImU8String), (ImGuiSliderFlags)0);
		ImGui.DragFloat(ImU8String.op_Implicit(Locale.Translate("config.overlay.world.dot_thickness")), ref Config.Overlay.WorldNodeOutlineWidth, 0.1f, 0f, 0f, default(ImU8String), (ImGuiSliderFlags)0);
		ImGui.SliderFloat(ImU8String.op_Implicit(Locale.Translate("config.overlay.world.scale_factor")), ref Config.Overlay.WorldNodeScaleFactor, 0.1f, 1f, default(ImU8String), (ImGuiSliderFlags)0);
		DrawColorEdit(Locale.Translate("config.overlay.world.color"), ref Config.Overlay.WorldNodeColor);
		DrawColorEdit(Locale.Translate("config.overlay.world.color_actor"), ref Config.Overlay.ActorNodeColor);
		DrawColorEdit(Locale.Translate("config.overlay.world.color_light"), ref Config.Overlay.LightNodeColor);
		ComboDisposable val2 = ImRaii.Combo(ImU8String.op_Implicit(Locale.Translate("config.overlay.world.highlight_color")), ImU8String.op_Implicit(Enum.GetName(Config.Overlay.WorldOutlineColor)));
		try
		{
			if (!val2.Success)
			{
				return;
			}
			OutlineChoice[] values2 = Enum.GetValues<OutlineChoice>();
			foreach (OutlineChoice outlineChoice in values2)
			{
				if (ImGui.Selectable(ImU8String.op_Implicit(Enum.GetName(outlineChoice)), outlineChoice == Config.Overlay.WorldOutlineColor, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					Config.Overlay.WorldOutlineColor = outlineChoice;
				}
			}
		}
		finally
		{
			((ComboDisposable)(ref val2)).Dispose();
		}
	}

	private bool ColorPicker4(string label, string id, ref uint value)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		Vector4 vector = ImGui.ColorConvertU32ToFloat4(value);
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(8, 1);
		((ImU8String)(ref val)).AppendLiteral("##");
		((ImU8String)(ref val)).AppendFormatted<string>(id);
		((ImU8String)(ref val)).AppendLiteral("Button");
		if (ImGui.ColorButton(val, ref vector, (ImGuiColorEditFlags)32, default(Vector2)))
		{
			ImGui.OpenPopup(ImU8String.op_Implicit(id), (ImGuiPopupFlags)0);
		}
		if (!string.IsNullOrWhiteSpace(label))
		{
			ImGui.SameLine();
			ImGui.Text(ImU8String.op_Implicit(label));
		}
		bool result = false;
		if (ImGui.BeginPopup(ImU8String.op_Implicit(id), (ImGuiWindowFlags)0))
		{
			ImGui.SetColorEditOptions((ImGuiColorEditFlags)177209344);
			ImU8String val2 = default(ImU8String);
			((ImU8String)(ref val2))._002Ector(2, 2);
			((ImU8String)(ref val2)).AppendFormatted<string>(label);
			((ImU8String)(ref val2)).AppendLiteral("##");
			((ImU8String)(ref val2)).AppendFormatted<string>(id);
			result = ImGui.ColorPicker4(val2, ref vector, (ImGuiColorEditFlags)181404034);
			ImGui.EndPopup();
		}
		value = ImGui.ColorConvertFloat4ToU32(vector);
		return result;
	}

	private void DrawWorkspaceTab()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("config.workspace.header")));
		ImGui.Spacing();
		if (ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.workspace.toolbar")), ref Config.Editor.UseToolbar))
		{
			IEditorContext current = _context.Current;
			if (current != null && current.IsValid && current.IsGPosing)
			{
				_gui.CreatePopup<ChangeStatePopup>(new object[1] { _context.Current }).Open();
			}
		}
		DrawHint("config.workspace.hintToolbar");
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.workspace.init")), ref Config.Editor.OpenOnEnterGPose);
		DrawHint("config.workspace.hintInit");
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.workspace.incognitoPlayerNames")), ref Config.Editor.IncognitoPlayerNames);
		DrawHint("config.workspace.hintIncognito");
		bool flag = ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.categories.allow_nsfw")), ref Config.Categories.ShowNsfwBones);
		DrawHint("config.categories.hint_nsfw");
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.workspace.confirmExit")), ref Config.Editor.ConfirmExit);
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.workspace.openTray")), ref Config.Editor.OpenTrayOnWorkspaceClose);
		DrawHint("config.workspace.hintTrayIcon");
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.workspace.selectTarget")), ref Config.Editor.SelectOnTarget);
		DrawHint("config.workspace.hintSelectTarget");
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.workspace.showHints")), ref Config.Editor.ShowHints);
		DrawHint("config.workspace.hintHint");
		ImGui.Spacing();
		if (ImGui.CollapsingHeader(ImU8String.op_Implicit(Locale.Translate("config.workspace.windowHeader")), (ImGuiTreeNodeFlags)0))
		{
			ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.workspace.toggleOpenWindows")), ref Config.Editor.ToggleOpenWindows);
			ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.workspace.legacyPoseTabs")), ref Config.Editor.UseLegacyPoseViewTabs);
			ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.workspace.editOnSelect")), ref Config.Editor.ToggleEditorOnSelect);
			ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.workspace.AutoResizeObjectEditor")), ref Config.Editor.AutoResizeObjectEditor);
			DrawHint("config.workspace.hint_AutoResizeObj");
			DisabledDisposable val = ImRaii.Disabled(!Config.Editor.ToggleEditorOnSelect);
			try
			{
				ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.workspace.closeOnDeselect")), ref Config.Editor.CloseEditorOnDeselect);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		ImGui.Spacing();
		if (ImGui.CollapsingHeader(ImU8String.op_Implicit(Locale.Translate("config.workspace.customLocations.header")), (ImGuiTreeNodeFlags)0))
		{
			DrawCustomLocations();
		}
		ImGui.SetCursorPosX(600f);
		ImGui.Dummy(Vector2.Zero);
		if (flag)
		{
			RefreshScene();
		}
	}

	private void DrawInputTab()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.input.enable")), ref Config.Keybinds.Enabled);
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.input.scrollAllow")), ref Config.Keybinds.ScrollAllow);
		DisabledDisposable val = ImRaii.Disabled(!Config.Keybinds.ScrollAllow);
		try
		{
			ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.input.scrollMod")), ref Config.Keybinds.ScrollModifier);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.input.blockLeft")), ref Config.Keybinds.BlockTargetLeftClick);
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.input.blockRight")), ref Config.Keybinds.BlockTargetRightClick);
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("config.input.help")));
		val = ImRaii.Disabled(!Config.Keybinds.Enabled);
		try
		{
			_keybinds.Draw("history|select|overlay|pose|scene|output");
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("config.input.reset")), default(Vector2)))
		{
			_keybinds.ResetBinds("history|select|overlay|pose|scene|output");
		}
	}

	private void DrawCamerasInputTab()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("config.input.cameras.header")));
		ImGui.Spacing();
		ImGui.DragFloat(ImU8String.op_Implicit(Locale.Translate("config.workspace.workcam.speed")), ref Config.Editor.WorkcamMoveSpeed, 0.001f, 0f, 100f, default(ImU8String), (ImGuiSliderFlags)0);
		ImGui.DragFloat(ImU8String.op_Implicit(Locale.Translate("config.workspace.workcam.fastMulti")), ref Config.Editor.WorkcamFastMulti, 0.001f, 0f, 100f, default(ImU8String), (ImGuiSliderFlags)0);
		ImGui.DragFloat(ImU8String.op_Implicit(Locale.Translate("config.workspace.workcam.slowMulti")), ref Config.Editor.WorkcamSlowMulti, 0.001f, 0f, 100f, default(ImU8String), (ImGuiSliderFlags)0);
		ImGui.DragFloat(ImU8String.op_Implicit(Locale.Translate("config.workspace.workcam.vertMulti")), ref Config.Editor.WorkcamVertMulti, 0.001f, 0f, 100f, default(ImU8String), (ImGuiSliderFlags)0);
		ImGui.DragFloat(ImU8String.op_Implicit(Locale.Translate("config.workspace.workcam.sens")), ref Config.Editor.WorkcamSens, 0.001f, 0f, 100f, default(ImU8String), (ImGuiSliderFlags)0);
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("config.input.help")));
		DisabledDisposable val = ImRaii.Disabled(!Config.Keybinds.Enabled);
		try
		{
			_keybinds.Draw("camera");
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("config.input.reset")), default(Vector2)))
		{
			_keybinds.ResetBinds("camera");
		}
	}

	private void DrawGizmoInputTab()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("config.input.gizmo.header")));
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("config.input.help")));
		DisabledDisposable val = ImRaii.Disabled(!Config.Keybinds.Enabled);
		try
		{
			_keybinds.Draw("gizmo");
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("config.input.reset")), default(Vector2)))
		{
			_keybinds.ResetBinds("gizmo");
		}
	}

	private void DrawToolbarInputTab()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("config.input.toolbar.header")));
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("config.input.help")));
		DisabledDisposable val = ImRaii.Disabled(!Config.Keybinds.Enabled);
		try
		{
			_keybinds.Draw("toolbar", allowToolbar: true);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("config.input.reset")), default(Vector2)))
		{
			_keybinds.ResetBinds("toolbar", allowToolbar: true);
		}
	}

	private void DrawAutoSaveTab()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		AutoSaveConfig autoSave = Config.AutoSave;
		ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.autosave.enable")), ref autoSave.Enabled);
		DisabledDisposable val = ImRaii.Disabled(!autoSave.Enabled);
		try
		{
			ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.autosave.disconnect")), ref autoSave.OnDisconnect);
			ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.autosave.ondisable")), ref autoSave.OnDisable);
			ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.autosave.clear")), ref autoSave.ClearOnExit);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		ImGui.Spacing();
		ImGui.SliderInt(ImU8String.op_Implicit(Locale.Translate("config.autosave.interval")), ref autoSave.Interval, 10, 600, ImU8String.op_Implicit("%d s"), (ImGuiSliderFlags)0);
		ImU8String val2 = ImU8String.op_Implicit(Locale.Translate("config.autosave.count"));
		ref int count = ref autoSave.Count;
		ImU8String val3 = default(ImU8String);
		ImGui.SliderInt(val2, ref count, 1, 20, val3, (ImGuiSliderFlags)0);
		ImGui.Spacing();
		ImGui.InputText(ImU8String.op_Implicit(Locale.Translate("config.autosave.path")), ref autoSave.FilePath, 256, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null);
		ImGui.InputText(ImU8String.op_Implicit(Locale.Translate("config.autosave.dir")), ref autoSave.FolderFormat, 256, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null);
		ColorDisposable val4 = ImRaii.PushColor((ImGuiCol)0, ImGui.GetColorU32((ImGuiCol)1), true);
		try
		{
			val3 = new ImU8String(21, 1);
			((ImU8String)(ref val3)).AppendLiteral("Example folder name: ");
			((ImU8String)(ref val3)).AppendFormatted<string>(_format.Replace(autoSave.FolderFormat));
			ImGui.TextUnformatted(val3);
		}
		finally
		{
			((IDisposable)val4)?.Dispose();
		}
		ImGui.Spacing();
		DrawAutoSaveFormatting();
	}

	private void DrawAutoSaveFormatting()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(20, 0);
		((ImU8String)(ref val)).AppendLiteral("##AutoSaveFormatters");
		TableDisposable val2 = ImRaii.Table(val, 2, (ImGuiTableFlags)2107264);
		try
		{
			if (!val2.Success)
			{
				return;
			}
			ImGui.TableSetupScrollFreeze(0, 1);
			ImGui.TableSetupColumn(ImU8String.op_Implicit("Formatter"), (ImGuiTableColumnFlags)0, 0f, 0u);
			ImGui.TableSetupColumn(ImU8String.op_Implicit("Example Value"), (ImGuiTableColumnFlags)0, 0f, 0u);
			ImGui.TableHeadersRow();
			foreach (var (text3, text4) in _format.GetReplacements())
			{
				ImGui.TableNextRow();
				ImGui.TableNextColumn();
				ImGui.TextUnformatted(ImU8String.op_Implicit(text3));
				ImGui.TableNextColumn();
				ImGui.TextUnformatted(ImU8String.op_Implicit(text4));
			}
		}
		finally
		{
			((TableDisposable)(ref val2)).Dispose();
		}
	}

	public void DrawPresetsTab()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		_ = Config.Presets;
		_presetEditor.Draw();
		ImGuiStylePtr style = ImGui.GetStyle();
		Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
		contentRegionAvail.X = 0f;
		Vector2 vector = contentRegionAvail;
		vector.Y -= ((ImGuiStylePtr)(ref style)).ItemSpacing.Y + ((ImGuiStylePtr)(ref style)).CellPadding.Y;
		ImGui.Dummy(vector);
	}

	public void DrawOffsetsTab()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		_ = Config.Offsets;
		_offsetEditor.Draw();
		ImGuiStylePtr style = ImGui.GetStyle();
		Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
		contentRegionAvail.X = 0f;
		Vector2 vector = contentRegionAvail;
		vector.Y -= ((ImGuiStylePtr)(ref style)).ItemSpacing.Y + ((ImGuiStylePtr)(ref style)).CellPadding.Y;
		ImGui.Dummy(vector);
	}

	public void DrawPoseViewTab()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		PoseViewConfig cfg = Config.PoseView;
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("config.poseview.description")));
		ImGui.AlignTextToFramePadding();
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("config.poseview.linkout.description")));
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		if (ImGui.Button(ImU8String.op_Implicit(Locale.Translate("config.poseview.linkout.button")), default(Vector2)))
		{
			GuiHelpers.OpenBrowser(Locale.Translate("config.poseview.linkout.link"));
		}
		ImGui.Spacing();
		string text = Locale.Translate("config.poseview.body");
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(9, 1);
		((ImU8String)(ref val)).AppendLiteral("poseview_");
		((ImU8String)(ref val)).AppendFormatted<string>(text);
		IdDisposable val2 = ImRaii.PushId(val, true);
		try
		{
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)62831, "Load " + text + " Image"))
			{
				SetPoseViewImage(delegate(string path)
				{
					cfg.BodyPath = path;
				});
			}
			DrawPoseViewPath(ref cfg.BodyPath, text);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		text = Locale.Translate("config.poseview.armor");
		ImU8String val3 = default(ImU8String);
		((ImU8String)(ref val3))._002Ector(9, 1);
		((ImU8String)(ref val3)).AppendLiteral("poseview_");
		((ImU8String)(ref val3)).AppendFormatted<string>(text);
		val2 = ImRaii.PushId(val3, true);
		try
		{
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)62831, "Load " + text + " Image"))
			{
				SetPoseViewImage(delegate(string path)
				{
					cfg.ArmorPath = path;
				});
			}
			DrawPoseViewPath(ref cfg.ArmorPath, text);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		text = Locale.Translate("config.poseview.face");
		ImU8String val4 = default(ImU8String);
		((ImU8String)(ref val4))._002Ector(9, 1);
		((ImU8String)(ref val4)).AppendLiteral("poseview_");
		((ImU8String)(ref val4)).AppendFormatted<string>(text);
		val2 = ImRaii.PushId(val4, true);
		try
		{
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)62831, "Load " + text + " Image"))
			{
				SetPoseViewImage(delegate(string path)
				{
					cfg.FacePath = path;
				});
			}
			DrawPoseViewPath(ref cfg.FacePath, text);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		text = Locale.Translate("config.poseview.lips");
		ImU8String val5 = default(ImU8String);
		((ImU8String)(ref val5))._002Ector(9, 1);
		((ImU8String)(ref val5)).AppendLiteral("poseview_");
		((ImU8String)(ref val5)).AppendFormatted<string>(text);
		val2 = ImRaii.PushId(val5, true);
		try
		{
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)62831, "Load " + text + " Image"))
			{
				SetPoseViewImage(delegate(string path)
				{
					cfg.LipsPath = path;
				});
			}
			DrawPoseViewPath(ref cfg.LipsPath, text);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		text = Locale.Translate("config.poseview.mouth");
		ImU8String val6 = default(ImU8String);
		((ImU8String)(ref val6))._002Ector(9, 1);
		((ImU8String)(ref val6)).AppendLiteral("poseview_");
		((ImU8String)(ref val6)).AppendFormatted<string>(text);
		val2 = ImRaii.PushId(val6, true);
		try
		{
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)62831, "Load " + text + " Image"))
			{
				SetPoseViewImage(delegate(string path)
				{
					cfg.MouthPath = path;
				});
			}
			DrawPoseViewPath(ref cfg.MouthPath, text);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		text = Locale.Translate("config.poseview.hands");
		ImU8String val7 = default(ImU8String);
		((ImU8String)(ref val7))._002Ector(9, 1);
		((ImU8String)(ref val7)).AppendLiteral("poseview_");
		((ImU8String)(ref val7)).AppendFormatted<string>(text);
		val2 = ImRaii.PushId(val7, true);
		try
		{
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)62831, "Load " + text + " Image"))
			{
				SetPoseViewImage(delegate(string path)
				{
					cfg.HandsPath = path;
				});
			}
			DrawPoseViewPath(ref cfg.HandsPath, text);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		text = Locale.Translate("config.poseview.tail");
		ImU8String val8 = default(ImU8String);
		((ImU8String)(ref val8))._002Ector(9, 1);
		((ImU8String)(ref val8)).AppendLiteral("poseview_");
		((ImU8String)(ref val8)).AppendFormatted<string>(text);
		val2 = ImRaii.PushId(val8, true);
		try
		{
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)62831, "Load " + text + " Image"))
			{
				SetPoseViewImage(delegate(string path)
				{
					cfg.TailPath = path;
				});
			}
			DrawPoseViewPath(ref cfg.TailPath, text);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		text = Locale.Translate("config.poseview.ears");
		ImU8String val9 = default(ImU8String);
		((ImU8String)(ref val9))._002Ector(9, 1);
		((ImU8String)(ref val9)).AppendLiteral("poseview_");
		((ImU8String)(ref val9)).AppendFormatted<string>(text);
		val2 = ImRaii.PushId(val9, true);
		try
		{
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)62831, "Load " + text + " Image"))
			{
				SetPoseViewImage(delegate(string path)
				{
					cfg.EarsPath = path;
				});
			}
			DrawPoseViewPath(ref cfg.EarsPath, text);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private void DrawLanguageTab()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		if (ImGui.Checkbox(ImU8String.op_Implicit(Locale.Translate("config.language.autoselect")), ref _cfg.File.Locale.AutoDetect))
		{
			Locale.HandleLanguageChangeDelegate();
		}
		DisabledDisposable val = ImRaii.Disabled(_cfg.File.Locale.AutoDetect);
		try
		{
			string text = Locale.Data?.MetaData.SelfName;
			if (Locale.Data?.MetaData.DisplayName != Locale.Data?.MetaData.SelfName)
			{
				text = text + " (" + Locale.Data?.MetaData.DisplayName + ")";
			}
			ComboDisposable val2 = ImRaii.Combo(ImU8String.op_Implicit(Locale.Translate("config.language.selector")), ImU8String.op_Implicit(text));
			try
			{
				if (!val2.Success)
				{
					return;
				}
				foreach (LocaleMetaData availableLocale in Locale.AvailableLocales)
				{
					string text2 = availableLocale.SelfName;
					if (availableLocale.DisplayName != availableLocale.SelfName)
					{
						text2 = text2 + " (" + availableLocale.DisplayName + ")";
					}
					if (ImGui.Selectable(ImU8String.op_Implicit(text2), availableLocale.TechnicalName == Locale.Data?.MetaData.TechnicalName, (ImGuiSelectableFlags)0, default(Vector2)) && availableLocale.TechnicalName != Locale.Data?.MetaData.TechnicalName)
					{
						_cfg.File.Locale.LocaleId = availableLocale.TechnicalName;
						Locale.LoadLocale(availableLocale.TechnicalName);
					}
				}
			}
			finally
			{
				((ComboDisposable)(ref val2)).Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DrawOutputTab()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e8: Unknown result type (might be due to invalid IL or missing references)
		PyonConfig pyon = Config.Pyon;
		ImGui.Text(ImU8String.op_Implicit("Hi-Res Output"));
		ImGui.SameLine();
		Icons.DrawIcon((FontAwesomeIcon)61529);
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("The below selected resolution will be toggled when pressing\nthe 'Toggle hi-res mode' keybind set on the Input tab (Default F9).\n\nWhen hi-res is toggled, press your Reshade/Gshade screenshot key to save the image at this resolution.\n\nThen press the toggle key again to revert back to your original resolution."));
		}
		if (pyon.Resolutions.Count == 0)
		{
			pyon.Resolutions.Add(new Size(1280, 768));
			pyon.Resolutions.Add(new Size(1600, 900));
			pyon.Resolutions.Add(new Size(1920, 1080));
			pyon.Resolutions.Add(new Size(1920, 1200));
			pyon.Resolutions.Add(new Size(2560, 1440));
			pyon.Resolutions.Add(new Size(2560, 1600));
			pyon.Resolutions.Add(new Size(3200, 1800));
			pyon.Resolutions.Add(new Size(3440, 1440));
			pyon.Resolutions.Add(new Size(3840, 2160));
			pyon.Resolutions.Add(new Size(3840, 2400));
			pyon.Resolutions.Add(new Size(4096, 2160));
			pyon.Resolutions.Add(new Size(5120, 2880));
			pyon.Resolutions.Add(new Size(7680, 4320));
			pyon.HiResSize = new Size(3840, 2160);
			resW = 3840;
			resH = 2160;
		}
		int index = 0;
		for (int i = 0; i < pyon.Resolutions.Count; i++)
		{
			if (pyon.Resolutions[i].Width == pyon.HiResSize.Width && pyon.Resolutions[i].Height == pyon.HiResSize.Height)
			{
				index = i;
				if (resW == 0)
				{
					resW = pyon.Resolutions[i].Width;
					resH = pyon.Resolutions[i].Height;
				}
				break;
			}
		}
		string[] array = pyon.Resolutions.Select((Size r) => $"{r.Width} x {r.Height}").ToArray();
		ImGui.SetNextItemWidth(210f);
		if (ImGui.Combo(ImU8String.op_Implicit("Resolution"), ref index, (ReadOnlySpan<string>)array, array.Length))
		{
			pyon.HiResSize = pyon.Resolutions[index];
			resW = pyon.HiResSize.Width;
			resH = pyon.HiResSize.Height;
		}
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit("Add/Remove Resolution"));
		ImGui.SetNextItemWidth(70f);
		if (ImGui.DragInt(ImU8String.op_Implicit("##w"), ref resW, 1f, 1040, 9999, default(ImU8String), (ImGuiSliderFlags)0))
		{
			if (resW < 1040)
			{
				resW = 1040;
			}
			if (resW > 9999)
			{
				resW = 9999;
			}
		}
		ImGui.SameLine(0f, 2f);
		ImGui.Text(ImU8String.op_Implicit("x"));
		ImGui.SameLine(0f, 2f);
		ImGui.SetNextItemWidth(70f);
		if (ImGui.DragInt(ImU8String.op_Implicit("##h"), ref resH, 1f, 768, 9999, default(ImU8String), (ImGuiSliderFlags)0))
		{
			if (resH < 768)
			{
				resH = 768;
			}
			if (resH > 9999)
			{
				resH = 9999;
			}
		}
		ImGui.SameLine();
		if (Buttons.IconButton((FontAwesomeIcon)61543, new Vector2(24f, 24f)) && pyon.Resolutions.Find((Size x) => x.Width == resW && x.Height == resH) == Size.Empty)
		{
			pyon.Resolutions.Add(new Size(resW, resH));
			pyon.Resolutions.Sort((Size a, Size b) => (a.Width + a.Height).CompareTo(b.Width + b.Height));
			pyon.HiResSize.Width = resW;
			pyon.HiResSize.Height = resH;
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Add the specified width/height to the Resolution list."));
		}
		ImGui.SameLine(0f, 2f);
		ImGui.BeginDisabled(pyon.Resolutions.Count <= 1);
		if (Buttons.IconButton((FontAwesomeIcon)61544, new Vector2(24f, 24f)))
		{
			Size? size = pyon.Resolutions.Find((Size x) => x.Width == resW && x.Height == resH);
			if (size.HasValue)
			{
				pyon.Resolutions.Remove(size ?? Size.Empty);
				resW = pyon.Resolutions[0].Width;
				resH = pyon.Resolutions[0].Height;
			}
		}
		ImGui.EndDisabled();
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Remove the specified width/height from the Resolution list."));
		}
	}

	private void DrawAboutTab()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("config.about.header")));
		ImGui.Spacing();
		ImGui.TextWrapped(ImU8String.op_Implicit(Locale.Translate("config.about.readme")));
		ImGui.Spacing();
		ImGui.AlignTextToFramePadding();
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("config.about.discordLinkout.description")));
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		if (ImGui.Button(ImU8String.op_Implicit(Locale.Translate("config.about.discordLinkout.button")), default(Vector2)))
		{
			GuiHelpers.OpenBrowser(Locale.Translate("config.about.discordLinkout.link"));
		}
		ImGui.Spacing();
		ImGui.AlignTextToFramePadding();
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("config.about.gitLinkout.description")));
		style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		if (ImGui.Button(ImU8String.op_Implicit(Locale.Translate("config.about.gitLinkout.button")), default(Vector2)))
		{
			GuiHelpers.OpenBrowser(Locale.Translate("config.about.gitLinkout.link"));
		}
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit("About KtisisPyon"));
		ImGui.Spacing();
		ImGui.TextWrapped(ImU8String.op_Implicit("KtisisPyon is an extended version of Ktisis with additional features. Recently I have started working on the main Ktisis branch so most KtisisPyon features will be moved to Ktisis."));
		ImGui.AlignTextToFramePadding();
		ImGui.Text(ImU8String.op_Implicit("Check the PyonPlugins Github for a list of additional features:"));
		style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		if (ImGui.Button(ImU8String.op_Implicit("PyonPlugins GitHub"), default(Vector2)))
		{
			GuiHelpers.OpenBrowser("https://github.com/priprii/FFXIVPlugins");
		}
		ImGui.Spacing();
		ImGui.AlignTextToFramePadding();
		ImGui.Text(ImU8String.op_Implicit("Join this Discord if you need any help with KtisisPyon specifically:"));
		style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		if (ImGui.Button(ImU8String.op_Implicit("Pyon Discord"), default(Vector2)))
		{
			GuiHelpers.OpenBrowser("https://discord.com/invite/3wBtUrVDJh");
		}
	}

	private void DrawCustomLocations()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0563: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		List<(string Path, string Name)> locations = Config.File.CustomLocations;
		ImGui.TextWrapped(ImU8String.op_Implicit(Locale.Translate("config.workspace.customLocations.description")));
		ImGui.Spacing();
		float num = Buttons.CalcSize();
		float num2 = num * 2f;
		ImGuiStylePtr style = ImGui.GetStyle();
		float num3 = num2 + ((ImGuiStylePtr)(ref style)).ItemSpacing.X;
		style = ImGui.GetStyle();
		float num4 = num3 + ((ImGuiStylePtr)(ref style)).CellPadding.X * 2f;
		TableDisposable val = ImRaii.Table(ImU8String.op_Implicit("##CustomLocationsTable"), 4, (ImGuiTableFlags)513);
		try
		{
			if (TableDisposable.op_Implicit(val))
			{
				ImGui.TableSetupColumn(ImU8String.op_Implicit("##Move"), (ImGuiTableColumnFlags)8, num4, 0u);
				ImGui.TableSetupColumn(ImU8String.op_Implicit(Locale.Translate("config.workspace.customLocations.columnName")), (ImGuiTableColumnFlags)4, 0.3f, 0u);
				ImGui.TableSetupColumn(ImU8String.op_Implicit(Locale.Translate("config.workspace.customLocations.columnPath")), (ImGuiTableColumnFlags)4, 0.7f, 0u);
				ImU8String val2 = ImU8String.op_Implicit("##Remove");
				float num5 = num * 2f;
				style = ImGui.GetStyle();
				ImGui.TableSetupColumn(val2, (ImGuiTableColumnFlags)8, num5 + ((ImGuiStylePtr)(ref style)).CellPadding.X * 2f, 0u);
				ImGui.TableHeadersRow();
				for (int i = 0; i < locations.Count; i++)
				{
					IdDisposable val3 = ImRaii.PushId(i, true);
					try
					{
						(string Path, string Name) tuple = locations[i];
						string item = tuple.Path;
						string item2 = tuple.Name;
						bool flag = locations[i].Path.Equals(Config.File.DefaultLocation);
						ImGui.TableNextRow();
						ImGui.TableNextColumn();
						DisabledDisposable val4 = ImRaii.Disabled(i == 0);
						try
						{
							if (Buttons.IconButton((FontAwesomeIcon)61538))
							{
								List<(string, string)> list = locations;
								int index = i;
								List<(string Path, string Name)> list2 = locations;
								int index2 = i - 1;
								(string, string) value = locations[i - 1];
								(string, string) value2 = locations[i];
								list[index] = value;
								list2[index2] = value2;
								_cfg.Save();
							}
						}
						finally
						{
							((IDisposable)val4)?.Dispose();
						}
						ImGui.SameLine();
						DisabledDisposable val5 = ImRaii.Disabled(i == locations.Count - 1);
						try
						{
							if (Buttons.IconButton((FontAwesomeIcon)61539))
							{
								List<(string, string)> list = locations;
								int index2 = i;
								List<(string Path, string Name)> list3 = locations;
								int index = i + 1;
								(string, string) value2 = locations[i + 1];
								(string, string) value = locations[i];
								list[index2] = value2;
								list3[index] = value;
								_cfg.Save();
							}
						}
						finally
						{
							((IDisposable)val5)?.Dispose();
						}
						ImGui.TableNextColumn();
						ImGui.SetNextItemWidth(-1f);
						ImU8String val6 = new ImU8String(6, 0);
						((ImU8String)(ref val6)).AppendLiteral("##name");
						if (ImGui.InputText(val6, ref item2, 256, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null))
						{
							locations[i] = (item, item2);
							_cfg.Save();
						}
						ImGui.TableNextColumn();
						float x = ImGui.GetContentRegionAvail().X;
						style = ImGui.GetStyle();
						ImGui.SetNextItemWidth(x - (num + ((ImGuiStylePtr)(ref style)).ItemSpacing.X));
						ImU8String val7 = new ImU8String(6, 0);
						((ImU8String)(ref val7)).AppendLiteral("##path");
						if (ImGui.InputText(val7, ref item, 512, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null))
						{
							locations[i] = (item, item2);
							_cfg.Save();
						}
						ImGui.SameLine();
						if (Buttons.IconButtonTooltip((FontAwesomeIcon)61564, Locale.Translate("config.workspace.customLocations.browse")))
						{
							int index3 = i;
							_gui.FileDialogs.OpenFolder(Locale.Translate("config.workspace.customLocations.selectFolder"), delegate(string p)
							{
								locations[index3] = (p, locations[index3].Name);
								_cfg.Save();
							});
						}
						ImGui.TableNextColumn();
						if (flag)
						{
							string tooltip = Locale.Translate("config.workspace.defaultLocation.remove");
							Vector4? iconColor = ImGuiColors.ParsedGold;
							if (Buttons.IconButtonTooltip((FontAwesomeIcon)61445, tooltip, null, iconColor))
							{
								Config.File.DefaultLocation = string.Empty;
							}
						}
						else if (Buttons.IconButtonTooltip((FontAwesomeIcon)61445, Locale.Translate("config.workspace.defaultLocation.add")))
						{
							Config.File.DefaultLocation = locations[i].Path;
						}
						ImGui.SameLine();
						if (Buttons.IconButtonTooltip((FontAwesomeIcon)61944, Locale.Translate("config.workspace.customLocations.remove")))
						{
							if (flag)
							{
								Config.File.DefaultLocation = string.Empty;
							}
							locations.RemoveAt(i);
							_cfg.Save();
							i--;
						}
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
				}
			}
		}
		finally
		{
			((TableDisposable)(ref val)).Dispose();
		}
		ImGui.Spacing();
		if (ImGui.Button(ImU8String.op_Implicit(Locale.Translate("config.workspace.customLocations.add")), default(Vector2)))
		{
			_gui.FileDialogs.OpenFolder(Locale.Translate("config.workspace.customLocations.selectFolder"), delegate(string p)
			{
				locations.Add((p, new DirectoryInfo(p).Name));
				_cfg.Save();
			});
		}
	}

	private void RefreshScene()
	{
		_context.Current?.Scene.Refresh();
	}

	private void DrawPoseViewPath(ref string configPath, string locale)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		ImGui.SameLine(0f, x);
		DisabledDisposable val = ImRaii.Disabled(string.IsNullOrEmpty(configPath));
		try
		{
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)61666, "Reset"))
			{
				configPath = null;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		ImGui.SameLine(0f, x);
		ImGui.InputText(ImU8String.op_Implicit(locale), ref configPath, 512, (ImGuiInputTextFlags)16400, (ImGuiInputTextCallbackDelegate)null);
	}

	private void SetPoseViewImage(Action<string> handler)
	{
		_gui.FileDialogs.OpenImage("image", handler);
	}

	private static bool DrawColorEdit(string label, ref uint color)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Vector4 vector = ImGui.ColorConvertU32ToFloat4(color);
		bool num = ImGui.ColorEdit4(ImU8String.op_Implicit(label), ref vector, (ImGuiColorEditFlags)0);
		if (num)
		{
			color = ImGui.ColorConvertFloat4ToU32(vector);
		}
		return num;
	}

	public override void OnClose()
	{
		base.OnClose();
		_cfg.Save();
	}
}
