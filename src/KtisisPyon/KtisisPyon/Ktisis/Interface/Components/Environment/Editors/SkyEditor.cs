using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Core.Attributes;
using Ktisis.Scene.Modules;
using Ktisis.Structs.Env;

namespace Ktisis.Interface.Components.Environment.Editors;

[Transient]
public class SkyEditor : EditorBase
{
	private readonly SetTextureSelect _texSky;

	private readonly SetTextureSelect _texCloudTop;

	private readonly SetTextureSelect _texCloudSide;

	public override string Name => Ktisis.Locale.Translate("env_edit.sky.title");

	public SkyEditor(SetTextureSelect texSky, SetTextureSelect texCloudTop, SetTextureSelect texCloudSide)
	{
		_texSky = texSky;
		_texCloudTop = texCloudTop;
		_texCloudSide = texCloudSide;
	}

	public override bool IsActivated(EnvOverride flags)
	{
		if (!flags.HasFlag(EnvOverride.SkyId))
		{
			return flags.HasFlag(EnvOverride.Clouds);
		}
		return true;
	}

	public override void Draw(IEnvModule module, ref EnvState state)
	{
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		DrawToggleCheckbox(Ktisis.Locale.Translate("env_edit.sky.edit_sky"), EnvOverride.SkyId, module);
		DisabledDisposable val = ImRaii.Disabled(!module.Override.HasFlag(EnvOverride.SkyId));
		try
		{
			_texSky.Draw(Ktisis.Locale.Translate("env_edit.sky.texture"), ref state.SkyId, (uint id) => $"bgcommon/nature/sky/texture/sky_{id:D3}.tex");
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		ImGui.Spacing();
		ImGui.Spacing();
		DrawToggleCheckbox(Ktisis.Locale.Translate("env_edit.sky.edit_clouds"), EnvOverride.Clouds, module);
		DisabledDisposable val2 = ImRaii.Disabled(!module.Override.HasFlag(EnvOverride.Clouds));
		try
		{
			_texCloudTop.Draw(Ktisis.Locale.Translate("env_edit.sky.top"), ref state.Clouds.CloudTexture, (uint id) => $"bgcommon/nature/cloud/texture/cloud_{id:D3}.tex");
			_texCloudSide.Draw(Ktisis.Locale.Translate("env_edit.sky.side"), ref state.Clouds.CloudSideTexture, (uint id) => $"bgcommon/nature/cloud/texture/cloudside_{id:D3}.tex");
			ImGui.ColorEdit3(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.sky.color")), ref state.Clouds.CloudColor, (ImGuiColorEditFlags)0);
			ImGui.ColorEdit3(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.sky.shadow_color")), ref state.Clouds.Color2, (ImGuiColorEditFlags)0);
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.sky.shadows")), ref state.Clouds.Gradient, 0f, 2f, default(ImU8String), (ImGuiSliderFlags)0);
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.sky.side_height")), ref state.Clouds.SideHeight, 0f, 2f, default(ImU8String), (ImGuiSliderFlags)0);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}
}
