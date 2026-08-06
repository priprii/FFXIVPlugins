using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Common.Utility;
using Ktisis.Core.Attributes;
using Ktisis.Scene.Modules;
using Ktisis.Structs.Env;

namespace Ktisis.Interface.Components.Environment.Editors;

[Transient]
public class WindEditor : EditorBase
{
	public override string Name => Ktisis.Locale.Translate("env_edit.wind.title");

	public override bool IsActivated(EnvOverride flags)
	{
		return flags.HasFlag(EnvOverride.Wind);
	}

	public override void Draw(IEnvModule module, ref EnvState state)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		DrawToggleCheckbox(Ktisis.Locale.Translate("env_edit.enable"), EnvOverride.Wind, module);
		DisabledDisposable val = Disable(module);
		try
		{
			DrawAngle(Ktisis.Locale.Translate("env_edit.wind.direction"), ref state.Wind.Direction, 0f, 360f);
			DrawAngle(Ktisis.Locale.Translate("env_edit.wind.angle"), ref state.Wind.Angle, 0f, 180f);
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.wind.speed")), ref state.Wind.Speed, 0f, 1.5f, default(ImU8String), (ImGuiSliderFlags)0);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DrawAngle(string label, ref float angle, float min, float max)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		float num = angle * MathHelpers.Deg2Rad;
		if (ImGui.SliderAngle(ImU8String.op_Implicit(label), ref num, min, max, default(ImU8String), (ImGuiSliderFlags)0))
		{
			angle = num * MathHelpers.Rad2Deg;
		}
	}
}
