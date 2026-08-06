using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Core.Attributes;
using Ktisis.Scene.Modules;
using Ktisis.Structs.Env;

namespace Ktisis.Interface.Components.Environment.Editors;

[Transient]
public class StarsEditor : EditorBase
{
	public override string Name => Ktisis.Locale.Translate("env_edit.stars.title");

	public override bool IsActivated(EnvOverride flags)
	{
		return flags.HasFlag(EnvOverride.Stars);
	}

	public override void Draw(IEnvModule module, ref EnvState state)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		DrawToggleCheckbox(Ktisis.Locale.Translate("env_edit.enable"), EnvOverride.Stars, module);
		DisabledDisposable val = Disable(module);
		try
		{
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.stars.title")), ref state.Stars.Stars, 0f, 20f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.stars.intensity") + "##1"), ref state.Stars.StarIntensity, 0f, 2.5f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.Spacing();
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.stars.constellations")), ref state.Stars.Constellations, 0f, 10f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.stars.intensity") + "##2"), ref state.Stars.ConstellationIntensity, 0f, 2.5f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.Spacing();
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.stars.galaxy_intensity")), ref state.Stars.GalaxyIntensity, 0f, 10f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.Spacing();
			ImGui.ColorEdit4(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.stars.moon_color")), ref state.Stars.MoonColor, (ImGuiColorEditFlags)0);
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.stars.moon_brightness")), ref state.Stars.MoonBrightness, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
