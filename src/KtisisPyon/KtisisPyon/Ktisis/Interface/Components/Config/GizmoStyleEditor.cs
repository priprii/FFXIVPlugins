using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using GLib.Widgets;
using Ktisis.Core.Attributes;
using Ktisis.Data.Config;
using Ktisis.Data.Config.Sections;
using Ktisis.Localization;

namespace Ktisis.Interface.Components.Config;

[Transient]
public class GizmoStyleEditor
{
	private readonly ConfigManager _cfg;

	private readonly LocaleManager _locale;

	private Configuration Config => _cfg.File;

	public GizmoStyleEditor(ConfigManager cfg, LocaleManager locale)
	{
		_cfg = cfg;
		_locale = locale;
	}

	public void Draw()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		Style defaultStyle = GizmoConfig.DefaultStyle;
		Style style = Config.Gizmo.Style;
		ChildDisposable val = ImRaii.Child(ImU8String.op_Implicit("##CfgStyleFrame"), ImGui.GetContentRegionAvail() - (_cfg.File.Editor.UseToolbar ? new Vector2(0f, 2f) : Vector2.Zero), true);
		try
		{
			DisabledDisposable val2 = ImRaii.Disabled();
			try
			{
				ImGui.TextWrapped(ImU8String.op_Implicit(Ktisis.Locale.Translate("config.gizmo.disabled")));
				if (ImGui.CollapsingHeader(ImU8String.op_Implicit(_locale.Translate("config.gizmo.editor.general.title")), (ImGuiTreeNodeFlags)0))
				{
					DrawStyleColor(_locale.Translate("config.gizmo.editor.general.dir_x"), ref style.ColorDirectionX, defaultStyle.ColorDirectionX);
					DrawStyleColor(_locale.Translate("config.gizmo.editor.general.dir_y"), ref style.ColorDirectionY, defaultStyle.ColorDirectionY);
					DrawStyleColor(_locale.Translate("config.gizmo.editor.general.dir_z"), ref style.ColorDirectionZ, defaultStyle.ColorDirectionZ);
					DrawStyleColor(_locale.Translate("config.gizmo.editor.general.active"), ref style.ColorSelection, defaultStyle.ColorSelection);
					DrawStyleColor(_locale.Translate("config.gizmo.editor.general.inactive"), ref style.ColorInactive, defaultStyle.ColorInactive);
				}
				if (ImGui.CollapsingHeader(ImU8String.op_Implicit(_locale.Translate("config.gizmo.editor.position.title")), (ImGuiTreeNodeFlags)0))
				{
					DrawStyleFloat(_locale.Translate("config.gizmo.editor.position.line_thick") + "##PosThickness", ref style.TranslationLineThickness, defaultStyle.TranslationLineThickness);
					DrawStyleFloat(_locale.Translate("config.gizmo.editor.position.arrow_size") + "##PosArrowSize", ref style.TranslationLineArrowSize, defaultStyle.TranslationLineArrowSize);
					DrawStyleFloat(_locale.Translate("config.gizmo.editor.position.axis_thick"), ref style.HatchedAxisLineThickness, defaultStyle.HatchedAxisLineThickness);
					DrawStyleFloat(_locale.Translate("config.gizmo.editor.position.circle_size") + "##PosCircleSize", ref style.CenterCircleSize, defaultStyle.CenterCircleSize);
					DrawStyleColor(_locale.Translate("config.gizmo.editor.position.plane_x") + "##PosPlaneColorX", ref style.ColorPlaneX, defaultStyle.ColorPlaneX);
					DrawStyleColor(_locale.Translate("config.gizmo.editor.position.plane_y") + "##PosPlaneColorY", ref style.ColorPlaneY, defaultStyle.ColorPlaneY);
					DrawStyleColor(_locale.Translate("config.gizmo.editor.position.plane_z") + "##PosPlaneColorZ", ref style.ColorPlaneZ, defaultStyle.ColorPlaneZ);
					DrawStyleColor(_locale.Translate("config.gizmo.editor.position.line_color") + "##PosLineColor", ref style.ColorTranslationLine, defaultStyle.ColorTranslationLine);
					DrawStyleColor(_locale.Translate("config.gizmo.editor.position.axis_color"), ref style.ColorHatchedAxisLines, defaultStyle.ColorHatchedAxisLines);
				}
				if (ImGui.CollapsingHeader(ImU8String.op_Implicit(_locale.Translate("config.gizmo.editor.rotation.title")), (ImGuiTreeNodeFlags)0))
				{
					DrawStyleFloat(_locale.Translate("config.gizmo.editor.rotation.inner_thick") + "##RotateThickness", ref style.RotationLineThickness, defaultStyle.RotationLineThickness);
					DrawStyleFloat(_locale.Translate("config.gizmo.editor.rotation.outer_thick") + "##RotateThicknessOuter", ref style.RotationOuterLineThickness, defaultStyle.RotationOuterLineThickness);
					DrawStyleColor(_locale.Translate("config.gizmo.editor.rotation.border_color") + "##RotateUsingBorder", ref style.ColorRotationUsingBorder, defaultStyle.ColorRotationUsingBorder);
					DrawStyleColor(_locale.Translate("config.gizmo.editor.rotation.fill_color") + "##RotateUsingFill", ref style.ColorRotationUsingFill, defaultStyle.ColorRotationUsingFill);
				}
				if (ImGui.CollapsingHeader(ImU8String.op_Implicit(_locale.Translate("config.gizmo.editor.scale.title")), (ImGuiTreeNodeFlags)0))
				{
					DrawStyleFloat(_locale.Translate("config.gizmo.editor.scale.line_thick") + "##ScaleThickness", ref style.ScaleLineThickness, defaultStyle.ScaleLineThickness);
					DrawStyleFloat(_locale.Translate("config.gizmo.editor.scale.circle_size") + "##ScaleSize", ref style.ScaleLineCircleSize, defaultStyle.ScaleLineCircleSize);
					DrawStyleColor(_locale.Translate("config.gizmo.editor.scale.line_color") + "##ScaleColor", ref style.ColorScaleLine, defaultStyle.ColorScaleLine);
				}
				if (ImGui.CollapsingHeader(ImU8String.op_Implicit(_locale.Translate("config.gizmo.editor.text.title")), (ImGuiTreeNodeFlags)0))
				{
					DrawStyleColor(_locale.Translate("config.gizmo.editor.text.color"), ref style.ColorText, defaultStyle.ColorText);
					DrawStyleColor(_locale.Translate("config.gizmo.editor.text.shadow_color"), ref style.ColorTextShadow, defaultStyle.ColorTextShadow);
				}
				Config.Gizmo.Style = style;
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

	private static void DrawStyleColor(string label, ref Vector4 value, Vector4 defaultValue)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		float cursorPosX = ImGui.GetCursorPosX();
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(13, 1);
		((ImU8String)(ref val)).AppendLiteral("##StyleFloat_");
		((ImU8String)(ref val)).AppendFormatted<string>(label);
		IdDisposable val2 = ImRaii.PushId(val, true);
		try
		{
			DisabledDisposable val3 = ImRaii.Disabled(value.Equals(defaultValue));
			try
			{
				if (Buttons.IconButtonTooltip((FontAwesomeIcon)61666, Ktisis.Locale.Translate("config.gizmo.editor.undo")))
				{
					value = defaultValue;
				}
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
			ImGuiStylePtr style = ImGui.GetStyle();
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			ImGui.SetNextItemWidth(ImGui.CalcItemWidth() - (ImGui.GetCursorPosX() - cursorPosX));
			ImGui.ColorEdit4(ImU8String.op_Implicit(label), ref value, (ImGuiColorEditFlags)0);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private static void DrawStyleFloat(string label, ref float value, float defaultValue)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		float cursorPosX = ImGui.GetCursorPosX();
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(13, 1);
		((ImU8String)(ref val)).AppendLiteral("##StyleFloat_");
		((ImU8String)(ref val)).AppendFormatted<string>(label);
		IdDisposable val2 = ImRaii.PushId(val, true);
		try
		{
			DisabledDisposable val3 = ImRaii.Disabled(value.Equals(defaultValue));
			try
			{
				if (Buttons.IconButtonTooltip((FontAwesomeIcon)61666, Ktisis.Locale.Translate("config.gizmo.editor.undo")))
				{
					value = defaultValue;
				}
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
			ImGuiStylePtr style = ImGui.GetStyle();
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			ImGui.SetNextItemWidth(ImGui.CalcItemWidth() - (ImGui.GetCursorPosX() - cursorPosX));
			ImGui.DragFloat(ImU8String.op_Implicit(label), ref value, 0.01f, 0f, 0f, default(ImU8String), (ImGuiSliderFlags)0);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}
}
