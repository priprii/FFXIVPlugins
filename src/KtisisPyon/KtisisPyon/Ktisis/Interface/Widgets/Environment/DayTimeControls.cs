using System;
using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.Timer;
using Ktisis.Structs.Env;

namespace Ktisis.Interface.Widgets.Environment;

public static class DayTimeControls
{
	public const float MaxTime = 86400f;

	public unsafe static bool DrawTime(EnvManagerEx* env, out float time)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		time = 0f;
		if (env == null)
		{
			return false;
		}
		time = ((EnvManager)(&env->_base)).DayTimeSeconds;
		DateTime dateTime = default(DateTime).AddSeconds(time);
		bool num = ImGui.SliderFloat(ImU8String.op_Implicit("##TimeControls_Slider"), ref time, 0f, 86400f, ImU8String.op_Implicit(dateTime.ToShortTimeString()), (ImGuiSliderFlags)128);
		ImGui.SameLine();
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
		bool flag = ImGui.DragFloat(ImU8String.op_Implicit("##TimeControls_Drag"), ref time, 10f, 0f, 86400f, ImU8String.op_Implicit("%.0f"), (ImGuiSliderFlags)0);
		return num || flag;
	}

	public unsafe static bool DrawDay(EnvManagerEx* env, out int day)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		day = CalculateDay(env);
		return ImGui.SliderInt(ImU8String.op_Implicit("##MoonPhase"), ref day, 0, 30, default(ImU8String), (ImGuiSliderFlags)0);
	}

	public unsafe static int CalculateDay(EnvManagerEx* env)
	{
		return (int)Math.Ceiling(((float)((ClientTime)(&((Framework)Framework.Instance()).ClientTime)).EorzeaTime - ((EnvManager)(&env->_base)).DayTimeSeconds) / 86400f) % 32;
	}
}
