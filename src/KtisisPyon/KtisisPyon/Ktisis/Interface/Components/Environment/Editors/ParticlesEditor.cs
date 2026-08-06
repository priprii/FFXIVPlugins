using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Core.Attributes;
using Ktisis.Scene.Modules;
using Ktisis.Structs.Env;

namespace Ktisis.Interface.Components.Environment.Editors;

[Transient]
public class ParticlesEditor : EditorBase
{
	private readonly SetTextureSelect _texDust;

	public override string Name => Ktisis.Locale.Translate("env_edit.particles.title");

	public ParticlesEditor(SetTextureSelect texDust)
	{
		_texDust = texDust;
	}

	public override bool IsActivated(EnvOverride flags)
	{
		return flags.HasFlag(EnvOverride.Dust);
	}

	public override void Draw(IEnvModule module, ref EnvState state)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		DrawToggleCheckbox(Ktisis.Locale.Translate("env_edit.enable"), EnvOverride.Dust, module);
		DisabledDisposable val = Disable(module);
		try
		{
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.particles.intensity")), ref state.Dust.Intensity, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.particles.size")), ref state.Dust.Size, 0f, 20f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.particles.glow")), ref state.Dust.Glow, 0f, 10f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.ColorEdit4(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.particles.color")), ref state.Dust.Color, (ImGuiColorEditFlags)0);
			ImGui.Spacing();
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.particles.weight")), ref state.Dust.Weight, 0f, 10f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.Spacing();
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.particles.spread")), ref state.Dust.Spread, 0f, 10f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.particles.speed")), ref state.Dust.Speed, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.particles.spin")), ref state.Dust.Spin, 0.05f, 5f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.Spacing();
			_texDust.Draw("Texture", ref state.Dust.TextureId, ResolvePath);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private string ResolvePath(uint id)
	{
		if (id == 1)
		{
			return "bgcommon/nature/snow/texture/snow.tex";
		}
		return $"bgcommon/nature/dust/texture/dust_{Math.Max(0u, id - 2):D3}.tex";
	}
}
