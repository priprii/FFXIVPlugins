using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using GLib.Popups;
using GLib.Widgets;
using Ktisis.Common.Extensions;
using Ktisis.Data.Config.Gobos;
using Ktisis.Data.Files;
using Ktisis.Data.Serialization;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Editor.Properties.Types;
using Ktisis.Localization;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.World;
using Ktisis.Structs.Lights;

namespace Ktisis.Interface.Editor.Properties;

public class LightPropertyList : ObjectPropertyList
{
	private readonly IEditorContext _ctx;

	private readonly ITextureProvider _tex;

	private readonly LocaleManager _locale;

	private readonly GoboSchema _goboSchema;

	private readonly PopupList<GoboEntry> _goboPopup;

	private GoboEntry? Gobo;

	public LightPropertyList(IEditorContext ctx, ITextureProvider tex, LocaleManager locale)
	{
		_ctx = ctx;
		_tex = tex;
		_locale = locale;
		_goboSchema = SchemaReader.ReadGobos();
		_goboPopup = new PopupList<GoboEntry>("##GoboPopup", DrawGoboRow).WithSearch(GoboSearchPredicate);
	}

	public override void Invoke(IPropertyListBuilder builder, SceneEntity entity)
	{
		LightEntity light = entity as LightEntity;
		if (light != null)
		{
			builder.AddHeader(Ktisis.Locale.Translate("object_edit.light.headers.light"), delegate
			{
				DrawLightTab(light);
			});
			builder.AddHeader(Ktisis.Locale.Translate("object_edit.light.headers.shadow"), delegate
			{
				DrawShadowsTab(light);
			});
		}
	}

	private unsafe void DrawLightTab(LightEntity entity)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0515: Unknown result type (might be due to invalid IL or missing references)
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_057b: Unknown result type (might be due to invalid IL or missing references)
		//IL_058d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0649: Unknown result type (might be due to invalid IL or missing references)
		//IL_064e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0663: Unknown result type (might be due to invalid IL or missing references)
		//IL_0668: Unknown result type (might be due to invalid IL or missing references)
		//IL_06be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0729: Unknown result type (might be due to invalid IL or missing references)
		//IL_072e: Unknown result type (might be due to invalid IL or missing references)
		SceneLight* ptr = entity.GetObject();
		RenderLight* ptr2 = ((ptr != null) ? ptr->RenderLight : null);
		if (ptr2 == null)
		{
			return;
		}
		DrawLightFlag(Ktisis.Locale.Translate("object_edit.light.light.reflection"), ptr2, LightFlags.Reflection);
		ImGui.Spacing();
		string text = _locale.Translate($"lightType.{ptr2->LightType}");
		if (ImGui.BeginCombo(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.light.light.type")), ImU8String.op_Implicit(text), (ImGuiComboFlags)0))
		{
			LightType[] values = Enum.GetValues<LightType>();
			foreach (LightType lightType in values)
			{
				if (ImGui.Selectable(ImU8String.op_Implicit(_locale.Translate($"lightType.{lightType}")), ptr2->LightType == lightType, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					if (lightType - 3 > LightType.Directional)
					{
						entity.RemoveGobo();
					}
					ptr2->LightType = lightType;
				}
			}
			ImGui.EndCombo();
		}
		ImGuiStylePtr style;
		switch (ptr2->LightType)
		{
		case LightType.SpotLight:
		{
			ImU8String val6 = default(ImU8String);
			((ImU8String)(ref val6))._002Ector(12, 1);
			((ImU8String)(ref val6)).AppendFormatted<string>(Ktisis.Locale.Translate("object_edit.light.light.spot.angle"));
			((ImU8String)(ref val6)).AppendLiteral("##LightAngle");
			ImGui.SliderFloat(val6, ref ptr2->LightAngle, 0f, 180f, ImU8String.op_Implicit("%0.0f deg"), (ImGuiSliderFlags)0);
			ImU8String val7 = default(ImU8String);
			((ImU8String)(ref val7))._002Ector(12, 1);
			((ImU8String)(ref val7)).AppendFormatted<string>(Ktisis.Locale.Translate("object_edit.light.light.spot.falloff"));
			((ImU8String)(ref val7)).AppendLiteral("##LightAngle");
			ImGui.SliderFloat(val7, ref ptr2->FalloffAngle, 0f, 180f, ImU8String.op_Implicit("%0.0f deg"), (ImGuiSliderFlags)0);
			break;
		}
		case LightType.AreaLight:
		{
			style = ImGui.GetStyle();
			float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
			ItemWidthDisposable val = ImRaii.ItemWidth(ImGui.CalcItemWidth() / 2f - x);
			ImU8String val5;
			try
			{
				ImU8String val2 = ImU8String.op_Implicit("##AngleX");
				float* x2 = &ptr2->AreaAngle.X;
				ImU8String val3 = default(ImU8String);
				ImGui.SliderAngle(val2, ref *x2, -90f, 90f, val3, (ImGuiSliderFlags)0);
				ImGui.SameLine(0f, x);
				val3 = new ImU8String(8, 1);
				((ImU8String)(ref val3)).AppendFormatted<string>(Ktisis.Locale.Translate("object_edit.light.light.area.angle"));
				((ImU8String)(ref val3)).AppendLiteral("##AngleY");
				ImU8String val4 = val3;
				float* y = &ptr2->AreaAngle.Y;
				val5 = default(ImU8String);
				ImGui.SliderAngle(val4, ref *y, -90f, 90f, val5, (ImGuiSliderFlags)0);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			((ImU8String)(ref val5))._002Ector(12, 1);
			((ImU8String)(ref val5)).AppendFormatted<string>(Ktisis.Locale.Translate("object_edit.light.light.area.falloff"));
			((ImU8String)(ref val5)).AppendLiteral("##LightAngle");
			ImGui.SliderFloat(val5, ref ptr2->FalloffAngle, 0f, 180f, ImU8String.op_Implicit("%0.0f deg"), (ImGuiSliderFlags)0);
			break;
		}
		}
		ImGui.Spacing();
		string text2 = _locale.Translate($"lightFalloff.{ptr2->FalloffType}");
		if (ImGui.BeginCombo(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.light.light.falloff.type")), ImU8String.op_Implicit(text2), (ImGuiComboFlags)0))
		{
			FalloffType[] values2 = Enum.GetValues<FalloffType>();
			foreach (FalloffType falloffType in values2)
			{
				if (ImGui.Selectable(ImU8String.op_Implicit(_locale.Translate($"lightFalloff.{falloffType}")), ptr2->FalloffType == falloffType, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					ptr2->FalloffType = falloffType;
				}
			}
			ImGui.EndCombo();
		}
		ImU8String val8 = default(ImU8String);
		((ImU8String)(ref val8))._002Ector(14, 1);
		((ImU8String)(ref val8)).AppendFormatted<string>(Ktisis.Locale.Translate("object_edit.light.light.falloff.power"));
		((ImU8String)(ref val8)).AppendLiteral("##FalloffPower");
		ImU8String val9 = val8;
		float* falloff = &ptr2->Falloff;
		ImU8String val10 = default(ImU8String);
		ImGui.DragFloat(val9, ref *falloff, 0.01f, 0f, 1000f, val10, (ImGuiSliderFlags)0);
		ImGui.Spacing();
		Vector3 rGB = ptr2->Color.RGB;
		if (ImGui.ColorEdit3(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.light.light.color")), ref rGB, (ImGuiColorEditFlags)8912896))
		{
			ptr2->Color.RGB = rGB;
		}
		ImU8String val11 = ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.light.light.intensity"));
		float* intensity = &ptr2->Color.Intensity;
		val10 = default(ImU8String);
		ImGui.DragFloat(val11, ref *intensity, 0.01f, 0f, 100f, val10, (ImGuiSliderFlags)0);
		((ImU8String)(ref val10))._002Ector(12, 1);
		((ImU8String)(ref val10)).AppendFormatted<string>(Ktisis.Locale.Translate("object_edit.light.light.range"));
		((ImU8String)(ref val10)).AppendLiteral("##LightRange");
		ImU8String val12 = val10;
		float* range = &ptr2->Range;
		ImU8String val13 = default(ImU8String);
		if (ImGui.DragFloat(val12, ref *range, 0.1f, 0f, 999f, val13, (ImGuiSliderFlags)0))
		{
			entity.Flags |= LightEntityFlags.Update;
		}
		ImGui.Spacing();
		ImGui.AlignTextToFramePadding();
		Icons.DrawIcon((FontAwesomeIcon)61529);
		if (ImGui.IsItemHovered())
		{
			TooltipDisposable val14 = ImRaii.Tooltip();
			try
			{
				ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.light.light.gobos.info")));
			}
			finally
			{
				((TooltipDisposable)(ref val14)).Dispose();
			}
		}
		ImGui.SameLine();
		LightType lightType2 = ptr2->LightType;
		bool flag = lightType2 - 1 <= LightType.Directional;
		DisabledDisposable val15 = ImRaii.Disabled(flag);
		try
		{
			string text3 = Ktisis.Locale.Translate("object_edit.light.light.gobos.choose");
			if (entity.Gobo != null)
			{
				text3 += Ktisis.Locale.Translate("object_edit.light.light.gobos.remove");
			}
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)61502, text3))
			{
				_goboPopup.Open();
			}
			if (ImGui.IsItemClicked((ImGuiMouseButton)1))
			{
				entity.RemoveGobo();
			}
			style = ImGui.GetStyle();
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			val13 = new ImU8String(1, 2);
			((ImU8String)(ref val13)).AppendFormatted<string>(Ktisis.Locale.Translate("object_edit.light.light.gobos.current"));
			((ImU8String)(ref val13)).AppendLiteral(" ");
			((ImU8String)(ref val13)).AppendFormatted<string>((entity.Gobo == null) ? "N/A" : entity.Gobo.Name);
			ImGui.Text(val13);
		}
		finally
		{
			((IDisposable)val15)?.Dispose();
		}
		ImGui.Spacing();
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)62831, Ktisis.Locale.Translate("object_edit.light.light.import")))
		{
			_ctx.Interface.OpenLightFile(delegate(string path, LightFile file)
			{
				_ctx.Scene.ApplyLightFile(entity, file);
			});
		}
		style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)61639, Ktisis.Locale.Translate("object_edit.light.light.export")))
		{
			_ctx.Interface.OpenLightExport(entity);
		}
		DrawGoboPopup(entity);
	}

	private unsafe void DrawShadowsTab(LightEntity entity)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		SceneLight* ptr = entity.GetObject();
		RenderLight* ptr2 = ((ptr != null) ? ptr->RenderLight : null);
		if (ptr2 != null)
		{
			DrawLightFlag(Ktisis.Locale.Translate("object_edit.light.shadow.dynamic"), ptr2, LightFlags.Dynamic);
			ImGui.Spacing();
			DrawLightFlag(Ktisis.Locale.Translate("object_edit.light.shadow.chara"), ptr2, LightFlags.CharaShadow);
			DrawLightFlag(Ktisis.Locale.Translate("object_edit.light.shadow.object"), ptr2, LightFlags.ObjectShadow);
			ImGui.Spacing();
			ImGui.DragFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.light.shadow.range")), ref ptr2->CharaShadowRange, 0.1f, 0f, 1000f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.Spacing();
			ImGui.DragFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.light.shadow.near")), ref ptr2->ShadowNear, 0.01f, 0f, 1000f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.DragFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.light.shadow.far")), ref ptr2->ShadowFar, 0.01f, 0f, 1000f, default(ImU8String), (ImGuiSliderFlags)0);
		}
	}

	private void DrawGoboPopup(LightEntity entity)
	{
		if (_goboPopup.IsOpen && _goboPopup.Draw(_goboSchema.Gobos, _goboSchema.Gobos.Count, out GoboEntry selected, CalcItemHeight()))
		{
			entity.SetGobo(selected);
		}
	}

	private static float CalcItemHeight()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		float textLineHeight = ImGui.GetTextLineHeight();
		ImGuiStylePtr style = ImGui.GetStyle();
		return (textLineHeight + ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.Y) * 2f;
	}

	private bool DrawGoboRow(GoboEntry gobo, bool isFocus)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		float num = CalcItemHeight();
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		float cursorPosX = ImGui.GetCursorPosX();
		bool result = ImGui.Button(ImU8String.op_Implicit(string.Empty), new Vector2(ImGui.GetContentRegionAvail().X, num));
		ImGui.SameLine(cursorPosX, num + x);
		ImGui.Text(ImU8String.op_Implicit(gobo.Name));
		ImGui.SameLine(cursorPosX, num + x);
		ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetTextLineHeight());
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)0, ImGui.GetColorU32((ImGuiCol)0).SetAlpha(175), true);
		try
		{
			ImGui.Text(ImU8String.op_Implicit(gobo.Path));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		ImGui.SameLine(cursorPosX);
		Vector2 vector = new Vector2(num, num);
		ISharedImmediateTexture val2 = null;
		try
		{
			val2 = _tex.GetFromGame(gobo.Path);
		}
		catch
		{
			Ktisis.Log.Error("[LightPropertyList] Couldn't resolve ITextureProvider path for gobo!\n" + gobo.Name + " @ " + gobo.Path);
		}
		if (val2 != null)
		{
			ImGui.Image(val2.GetWrapOrEmpty().Handle, vector);
		}
		else
		{
			ImGui.Dummy(vector);
		}
		return result;
	}

	private static bool GoboSearchPredicate(GoboEntry gobo, string query)
	{
		if (!gobo.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
		{
			return gobo.Path.Contains(query, StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private unsafe void DrawLightFlag(string label, RenderLight* light, LightFlags flag)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		bool flag2 = light->Flags.HasFlag(flag);
		if (ImGui.Checkbox(ImU8String.op_Implicit(label), ref flag2))
		{
			light->Flags ^= flag;
		}
	}
}
