using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace Ktisis.Interface.Widgets;

public static class ToggleButton
{
	private static readonly Vector4 ToggleBg = new Vector4(0.35f, 0.35f, 0.35f, 1f);

	private static readonly Vector4 ToggleBgHover = new Vector4(0.78f, 0.78f, 0.78f, 1f);

	private const float ToggleWidthRatio = 1.55f;

	public static bool Draw(string id, ref bool v, uint circleColor = uint.MaxValue)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		Span<Vector4> colors = ((ImGuiStylePtr)(ref style)).Colors;
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		float frameHeight = ImGui.GetFrameHeight();
		float num = frameHeight * 1.55f;
		float num2 = frameHeight * 0.5f;
		bool result = false;
		ImGui.InvisibleButton(ImU8String.op_Implicit(id), new Vector2(num, frameHeight), (ImGuiButtonFlags)0);
		if (ImGui.IsItemClicked())
		{
			result = true;
			v = !v;
		}
		Vector4 vector = (ImGui.IsItemHovered() ? ((!v) ? colors[23] : ToggleBgHover) : ((!v) ? (colors[21] * 0.6f) : ToggleBg));
		Vector4 vector2 = vector;
		Vector2 vector3 = new Vector2(cursorScreenPos.X + num, cursorScreenPos.Y + frameHeight);
		((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(cursorScreenPos, vector3, ImGui.GetColorU32(vector2), frameHeight * 0.5f);
		((ImDrawListPtr)(ref windowDrawList)).AddCircleFilled(new Vector2(cursorScreenPos.X + num2 + (float)(v ? 1 : 0) * (num - num2 * 2f), cursorScreenPos.Y + num2), num2 - 1.5f, circleColor);
		return result;
	}
}
