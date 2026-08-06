using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Core.Attributes;
using Ktisis.Scene.Modules;
using Ktisis.Structs.Env;

namespace Ktisis.Interface.Components.Environment.Editors;

[Transient]
public class RainEditor : EditorBase
{
	public override string Name => Ktisis.Locale.Translate("env_edit.rain.title");

	public override bool IsActivated(EnvOverride flags)
	{
		return flags.HasFlag(EnvOverride.Rain);
	}

	public override void Draw(IEnvModule module, ref EnvState state)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		DrawToggleCheckbox(Ktisis.Locale.Translate("env_edit.enable"), EnvOverride.Rain, module);
		DisabledDisposable val = Disable(module);
		try
		{
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.rain.intensity")), ref state.Rain.Intensity, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.rain.thickness")), ref state.Rain.Size, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.ColorEdit4(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.rain.color")), ref state.Rain.Color, (ImGuiColorEditFlags)0);
			ImGui.Spacing();
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.rain.weight")), ref state.Rain.Weight, 0f, 10f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.rain.scattering")), ref state.Rain.Scatter, 0f, 10f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.Spacing();
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.rain.raindrops")), ref state.Rain.Raindrops, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
