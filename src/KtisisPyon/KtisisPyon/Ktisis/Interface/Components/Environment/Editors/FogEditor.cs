using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Core.Attributes;
using Ktisis.Scene.Modules;
using Ktisis.Structs.Env;

namespace Ktisis.Interface.Components.Environment.Editors;

[Transient]
public class FogEditor : EditorBase
{
	public override string Name => Ktisis.Locale.Translate("env_edit.fog.title");

	public override bool IsActivated(EnvOverride flags)
	{
		return flags.HasFlag(EnvOverride.Fog);
	}

	public override void Draw(IEnvModule module, ref EnvState state)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		DrawToggleCheckbox(Ktisis.Locale.Translate("env_edit.enable"), EnvOverride.Fog, module);
		DisabledDisposable val = Disable(module);
		try
		{
			ImGui.ColorEdit4(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.fog.color")), ref state.Fog.Color, (ImGuiColorEditFlags)0);
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.fog.distance")), ref state.Fog.Distance, 0f, 1000f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.fog.thickness")), ref state.Fog.Thickness, 0f, 100f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.Spacing();
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.fog.opacity")), ref state.Fog.Opacity, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.fog.sky_vis")), ref state.Fog.SkyVisibility, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
