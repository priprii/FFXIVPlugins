using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Core.Attributes;
using Ktisis.Scene.Modules;
using Ktisis.Structs.Env;

namespace Ktisis.Interface.Components.Environment.Editors;

[Transient]
public class LightingEditor : EditorBase
{
	public override string Name => Ktisis.Locale.Translate("env_edit.lighting.title");

	public override bool IsActivated(EnvOverride flags)
	{
		return flags.HasFlag(EnvOverride.Lighting);
	}

	public override void Draw(IEnvModule module, ref EnvState state)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		DrawToggleCheckbox(Ktisis.Locale.Translate("env_edit.enable"), EnvOverride.Lighting, module);
		DisabledDisposable val = Disable(module);
		try
		{
			DisabledDisposable val2 = ImRaii.Disabled(!module.Override.HasFlag(EnvOverride.Lighting));
			try
			{
				ImGui.ColorEdit3(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.lighting.sunlight")), ref state.Lighting.SunLightColor, (ImGuiColorEditFlags)0);
				ImGui.ColorEdit3(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.lighting.moonlight")), ref state.Lighting.MoonLightColor, (ImGuiColorEditFlags)0);
				ImGui.ColorEdit3(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.lighting.ambient")), ref state.Lighting.Ambient, (ImGuiColorEditFlags)0);
				ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("common.unknown") + " #1"), ref state.Lighting._unk1, 0f, 10f, default(ImU8String), (ImGuiSliderFlags)0);
				ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.lighting.saturation")), ref state.Lighting.AmbientSaturation, 0f, 5f, default(ImU8String), (ImGuiSliderFlags)0);
				ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.lighting.temperature")), ref state.Lighting.Temperature, -2.5f, 2.5f, default(ImU8String), (ImGuiSliderFlags)0);
				ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("common.unknown") + " #2"), ref state.Lighting._unk2, 0f, 100f, default(ImU8String), (ImGuiSliderFlags)0);
				ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("common.unknown") + " #3"), ref state.Lighting._unk3, 0f, 100f, default(ImU8String), (ImGuiSliderFlags)0);
				ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("common.unknown") + " #4"), ref state.Lighting._unk4, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
