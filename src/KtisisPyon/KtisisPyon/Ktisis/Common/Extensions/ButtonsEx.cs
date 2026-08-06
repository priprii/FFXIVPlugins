using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace Ktisis.Common.Extensions;

public static class ButtonsEx
{
	internal static bool IsClicked()
	{
		Vector2 itemRectMin = ImGui.GetItemRectMin();
		Vector2 itemRectMax = ImGui.GetItemRectMax();
		if (ImGui.IsMouseHoveringRect(itemRectMin, itemRectMax))
		{
			return ImGui.IsMouseClicked((ImGuiMouseButton)0);
		}
		return false;
	}

	internal static bool IsClicked(Vector2 margin)
	{
		Vector2 vector = ImGui.GetItemRectMin() - margin;
		Vector2 vector2 = ImGui.GetItemRectMax() + margin;
		if (ImGui.IsMouseHoveringRect(vector, vector2))
		{
			return ImGui.IsMouseClicked((ImGuiMouseButton)0);
		}
		return false;
	}
}
