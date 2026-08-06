using System;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Selection;
using Ktisis.Interface.Types;
using Ktisis.Localization;
using Ktisis.Scene.Entities.World;
using Ktisis.Structs.Lights;

namespace Ktisis.Interface.Windows.Editors;

public class LightWindow : EntityEditWindow<LightEntity>
{
	private readonly LocaleManager _locale;

	public LightWindow(IEditorContext ctx, LocaleManager locale)
		: base("Light Editor", ctx, (ImGuiWindowFlags)0, "###KtisisLightEditor")
	{
		_locale = locale;
	}

	public override void PreDraw()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		base.PreDraw();
		WindowSizeConstraints value = default(WindowSizeConstraints);
		((WindowSizeConstraints)(ref value))._002Ector();
		((WindowSizeConstraints)(ref value)).MinimumSize = new Vector2(400f, 300f);
		ImGuiIOPtr iO = ImGui.GetIO();
		((WindowSizeConstraints)(ref value)).MaximumSize = ((ImGuiIOPtr)(ref iO)).DisplaySize * 0.9f;
		((Window)this).SizeConstraints = value;
	}

	public override void Draw()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		UpdateTarget();
		ISelectManager selection = Context.Selection;
		if (selection.Count == 1 && selection.GetSelected().First() is LightEntity target)
		{
			SetTarget(target);
		}
		LightEntity target2 = base.Target;
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(1, 1);
		((ImU8String)(ref val)).AppendFormatted<string>(target2.Name);
		((ImU8String)(ref val)).AppendLiteral(":");
		ImGui.Text(val);
		ImGui.Spacing();
		TabBarDisposable val2 = ImRaii.TabBar(ImU8String.op_Implicit("##LightEditTabs"));
		try
		{
			DrawTab("Light", DrawLightTab, target2);
			DrawTab("Shadows", DrawShadowsTab, target2);
		}
		finally
		{
			((TabBarDisposable)(ref val2)).Dispose();
		}
	}

	private void DrawTab(string label, Action<LightEntity> draw, LightEntity entity)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		TabItemDisposable val = ImRaii.TabItem(ImU8String.op_Implicit(label));
		try
		{
			if (val.Success)
			{
				draw(entity);
			}
		}
		finally
		{
			((TabItemDisposable)(ref val)).Dispose();
		}
	}

	private unsafe void DrawLightTab(LightEntity entity)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		SceneLight* ptr = entity.GetObject();
		RenderLight* ptr2 = ((ptr != null) ? ptr->RenderLight : null);
		if (ptr2 == null)
		{
			return;
		}
		ImGui.Spacing();
		DrawLightFlag("Enable reflections", ptr2, LightFlags.Reflection);
		ImGui.Spacing();
		string text = _locale.Translate($"lightType.{ptr2->LightType}");
		if (ImGui.BeginCombo(ImU8String.op_Implicit("Light Type"), ImU8String.op_Implicit(text), (ImGuiComboFlags)0))
		{
			LightType[] values = Enum.GetValues<LightType>();
			foreach (LightType lightType in values)
			{
				if (ImGui.Selectable(ImU8String.op_Implicit(_locale.Translate($"lightType.{lightType}")), ptr2->LightType == lightType, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					ptr2->LightType = lightType;
				}
			}
			ImGui.EndCombo();
		}
		switch (ptr2->LightType)
		{
		case LightType.SpotLight:
			ImGui.SliderFloat(ImU8String.op_Implicit("Cone Angle##LightAngle"), ref ptr2->LightAngle, 0f, 180f, ImU8String.op_Implicit("%0.0f deg"), (ImGuiSliderFlags)0);
			ImGui.SliderFloat(ImU8String.op_Implicit("Falloff Angle##LightAngle"), ref ptr2->FalloffAngle, 0f, 180f, ImU8String.op_Implicit("%0.0f deg"), (ImGuiSliderFlags)0);
			break;
		case LightType.AreaLight:
		{
			ImGuiStylePtr style = ImGui.GetStyle();
			float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
			ItemWidthDisposable val = ImRaii.ItemWidth(ImGui.CalcItemWidth() / 2f - x);
			try
			{
				ImGui.SliderAngle(ImU8String.op_Implicit("##AngleX"), ref ptr2->AreaAngle.X, -90f, 90f, default(ImU8String), (ImGuiSliderFlags)0);
				ImGui.SameLine(0f, x);
				ImGui.SliderAngle(ImU8String.op_Implicit("Light Angle##AngleY"), ref ptr2->AreaAngle.Y, -90f, 90f, default(ImU8String), (ImGuiSliderFlags)0);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			ImGui.SliderFloat(ImU8String.op_Implicit("Falloff Angle##LightAngle"), ref ptr2->FalloffAngle, 0f, 180f, ImU8String.op_Implicit("%0.0f deg"), (ImGuiSliderFlags)0);
			break;
		}
		}
		ImGui.Spacing();
		string text2 = _locale.Translate($"lightFalloff.{ptr2->FalloffType}");
		if (ImGui.BeginCombo(ImU8String.op_Implicit("Falloff Type"), ImU8String.op_Implicit(text2), (ImGuiComboFlags)0))
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
		ImGui.DragFloat(ImU8String.op_Implicit("Falloff Power##FalloffPower"), ref ptr2->Falloff, 0.01f, 0f, 1000f, default(ImU8String), (ImGuiSliderFlags)0);
		ImGui.Spacing();
		Vector3 rGB = ptr2->Color.RGB;
		if (ImGui.ColorEdit3(ImU8String.op_Implicit("Color"), ref rGB, (ImGuiColorEditFlags)8912896))
		{
			ptr2->Color.RGB = rGB;
		}
		ImGui.DragFloat(ImU8String.op_Implicit("Intensity"), ref ptr2->Color.Intensity, 0.01f, 0f, 100f, default(ImU8String), (ImGuiSliderFlags)0);
		if (ImGui.DragFloat(ImU8String.op_Implicit("Range##LightRange"), ref ptr2->Range, 0.1f, 0f, 999f, default(ImU8String), (ImGuiSliderFlags)0))
		{
			entity.Flags |= LightEntityFlags.Update;
		}
	}

	private unsafe void DrawShadowsTab(LightEntity entity)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		SceneLight* ptr = entity.GetObject();
		RenderLight* ptr2 = ((ptr != null) ? ptr->RenderLight : null);
		if (ptr2 != null)
		{
			ImGui.Spacing();
			DrawLightFlag("Dynamic shadows", ptr2, LightFlags.Dynamic);
			ImGui.Spacing();
			DrawLightFlag("Cast character shadows", ptr2, LightFlags.CharaShadow);
			DrawLightFlag("Cast object shadows", ptr2, LightFlags.ObjectShadow);
			ImGui.Spacing();
			ImGui.DragFloat(ImU8String.op_Implicit("Shadow Range"), ref ptr2->CharaShadowRange, 0.1f, 0f, 1000f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.Spacing();
			ImGui.DragFloat(ImU8String.op_Implicit("Shadow Near"), ref ptr2->ShadowNear, 0.01f, 0f, 1000f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.DragFloat(ImU8String.op_Implicit("Shadow Far"), ref ptr2->ShadowFar, 0.01f, 0f, 1000f, default(ImU8String), (ImGuiSliderFlags)0);
		}
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
