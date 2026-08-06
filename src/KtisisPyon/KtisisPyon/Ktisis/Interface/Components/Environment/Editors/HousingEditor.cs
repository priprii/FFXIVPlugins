using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Core.Attributes;
using Ktisis.Scene.Modules;
using Ktisis.Services.Data;
using Ktisis.Structs.Env;

namespace Ktisis.Interface.Components.Environment.Editors;

[Transient]
public class HousingEditor(HousingDataService housingDataService) : EditorBase
{
	public override string Name => Ktisis.Locale.Translate("env_edit.housing.title");

	public override bool IsActivated(EnvOverride flags)
	{
		if (flags.HasFlag(EnvOverride.Housing))
		{
			return housingDataService.IsInHousing;
		}
		return false;
	}

	public override void Draw(IEnvModule module, ref EnvState state)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		if (DrawToggleCheckbox(Ktisis.Locale.Translate("env_edit.enable"), EnvOverride.Housing, module) && !IsActivated(module.Override))
		{
			housingDataService.ResetLighting();
			housingDataService.ResetSSAO();
		}
		DisabledDisposable val = Disable(module);
		try
		{
			if (float.IsNaN(housingDataService.IndoorLight))
			{
				ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.housing.unavailable")));
				return;
			}
			float indoorLight = housingDataService.IndoorLight;
			if (ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.housing.brightness")), ref indoorLight, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0))
			{
				housingDataService.IndoorLight = indoorLight;
			}
			bool sSAOEnabled = housingDataService.SSAOEnabled;
			if (ImGui.Checkbox(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.housing.ssao")), ref sSAOEnabled))
			{
				housingDataService.SSAOEnabled = sSAOEnabled;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
