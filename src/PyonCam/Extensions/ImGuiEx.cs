using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace PyonCam.Extensions;

public static class ImGuiEx
{
	public static void SetItemTooltip(string s, ImGuiHoveredFlags flags = (ImGuiHoveredFlags)0)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (ImGui.IsItemHovered(flags))
		{
			ImGui.SetTooltip(ImU8String.op_Implicit(s));
		}
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

	public static bool IconSelectable(FontAwesomeIcon icon)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		bool result = ImGui.Selectable(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(icon)), false, (ImGuiSelectableFlags)0, default(Vector2));
		ImGui.PopFont();
		return result;
	}

	public static void IconTextUnformatted(FontAwesomeIcon icon)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		ImGui.TextUnformatted(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(icon)));
		ImGui.PopFont();
	}

	public static bool ColorIconButton(FontAwesomeIcon icon, string id, uint iconColor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		ImGui.PushStyleColor((ImGuiCol)21, 0u);
		ImGui.PushStyleColor((ImGuiCol)0, iconColor);
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(2, 2);
		((ImU8String)(ref val)).AppendFormatted<string>(FontAwesomeExtensions.ToIconString(icon));
		((ImU8String)(ref val)).AppendLiteral("##");
		((ImU8String)(ref val)).AppendFormatted<string>(id);
		bool result = ImGui.Button(val, new Vector2(24f, 20f));
		ImGui.PopStyleColor(2);
		ImGui.PopFont();
		return result;
	}

	public static bool TreeNode(string text, string id, Vector4 col = default(Vector4), Action? contextMenu = null, ImGuiTreeNodeFlags flags = (ImGuiTreeNodeFlags)0)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(0, 1);
		((ImU8String)(ref val)).AppendFormatted<string>(id);
		ImGui.PushID(val);
		Vector4 vector = ((col == default(Vector4)) ? ImGuiColors.DalamudViolet : col);
		ImGui.PushStyleColor((ImGuiCol)0, vector);
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(0, 1);
		((ImU8String)(ref val2)).AppendFormatted<string>(id);
		bool result = ImGui.TreeNodeEx(val2, flags, ImU8String.op_Implicit(text));
		ImGui.PopStyleColor();
		if (contextMenu != null && ImGui.BeginPopupContextItem(ImU8String.op_Implicit("##treeContext"), (ImGuiPopupFlags)1))
		{
			contextMenu();
			ImGui.EndPopup();
		}
		ImGui.PopID();
		return result;
	}
}
