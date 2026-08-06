using System.Linq;
using System.Numerics;
using System.Reflection;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace PartyPyon;

public static class ImGuiEx
{
	public static bool InputText(string label, object obj, string nameofProp, int length = 255)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		string value = (string)(property?.GetValue(obj) ?? "");
		bool result = ImGui.InputText(ImU8String.op_Implicit(label), ref value, length, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null);
		property?.SetValue(obj, value);
		return result;
	}

	public static bool IconButton(FontAwesomeIcon icon)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		bool result = ImGui.Button(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(icon)), default(Vector2));
		ImGui.PopFont();
		return result;
	}

	public static bool InputTextMultilineWithHint(Plugin plugin, string label, ref string text, int bufSize, Vector2 size, string hint, int maxLines = 2, ImGuiInputTextFlags flags = (ImGuiInputTextFlags)0)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		bool result = ImGui.InputTextMultiline(ImU8String.op_Implicit(label), ref text, bufSize, size, flags, (ImGuiInputTextCallbackDelegate)null);
		if (string.IsNullOrEmpty(text) && !ImGui.IsItemActive())
		{
			ImGuiStylePtr style = ImGui.GetStyle();
			Vector4 vector = ((ImGuiStylePtr)(ref style)).Colors[1];
			ImGui.SetCursorScreenPos(ImGui.GetItemRectMin() + new Vector2(3f, 2f));
			ImGui.PushStyleColor((ImGuiCol)0, vector);
			ImGui.TextUnformatted(ImU8String.op_Implicit(hint));
			ImGui.PopStyleColor();
		}
		string text2 = "";
		int num = text.Count((char c) => c == '\n') + 1;
		if ((string.IsNullOrWhiteSpace(text2) && num > maxLines) || text.Length > 128)
		{
			text2 = "Comment is limited to 2 lines & up to 192 characters.\nYou should confirm that this comment fits in the PF Listing Comment box.";
		}
		if (!string.IsNullOrWhiteSpace(text2))
		{
			Vector2 itemRectMin = ImGui.GetItemRectMin();
			Vector2 itemRectMax = ImGui.GetItemRectMax();
			float fontSize = ImGui.GetFontSize();
			float num2 = 16f;
			Vector2 vector2 = new Vector2(itemRectMax.X - fontSize - num2, itemRectMin.Y + (itemRectMax.Y - itemRectMin.Y - fontSize) * 0.5f);
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			ImGui.PushFont(UiBuilder.IconFont);
			((ImDrawListPtr)(ref windowDrawList)).AddText(vector2, ImGui.GetColorU32(new Vector4(1f, 1f, 0f, 1f)), ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61553)));
			ImGui.PopFont();
			ImGuiIOPtr iO = ImGui.GetIO();
			Vector2 mousePos = ((ImGuiIOPtr)(ref iO)).MousePos;
			Vector4 vector3 = new Vector4(vector2.X, vector2.Y, vector2.X + fontSize, vector2.Y + fontSize);
			if (mousePos.X >= vector3.X && mousePos.X <= vector3.Z && mousePos.Y >= vector3.Y && mousePos.Y <= vector3.W)
			{
				ImGui.SetTooltip(ImU8String.op_Implicit(text2));
				ImGui.SetMouseCursor((ImGuiMouseCursor)0);
			}
		}
		return result;
	}
}
