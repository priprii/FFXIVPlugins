using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace SoundPyon;

public static class ImGuiEx
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetItemTooltip(string s, ImGuiHoveredFlags flags = (ImGuiHoveredFlags)0)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (ImGui.IsItemHovered(flags))
		{
			ImGui.SetTooltip(ImU8String.op_Implicit(s));
		}
	}

	public static bool IconButton(FontAwesomeIcon icon, string id)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(2, 2);
		((ImU8String)(ref val)).AppendFormatted<string>(FontAwesomeExtensions.ToIconString(icon));
		((ImU8String)(ref val)).AppendLiteral("##");
		((ImU8String)(ref val)).AppendFormatted<string>(id);
		bool result = ImGui.Button(val, default(Vector2));
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
}
