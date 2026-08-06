using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using GLib.Widgets;

namespace Ktisis.Legacy.Interface;

public static class DialogHelpers
{
	public static void BuildDialog(ref bool newSet, bool newDefault, string tooltipString, string newSettingName, string secondaryText)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		ImGui.AlignTextToFramePadding();
		ImGui.Text(ImU8String.op_Implicit(newSettingName));
		if (tooltipString != string.Empty)
		{
			DrawHint(tooltipString);
		}
		string text = "Default: " + (newDefault ? "On" : "Off");
		float num = ImGui.GetContentRegionAvail().X - ImGui.GetFrameHeight() - ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f).X;
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SameLine(num - ((ImGuiStylePtr)(ref style)).FramePadding.X);
		ImGui.TextDisabled(ImU8String.op_Implicit(text));
		ImGui.SameLine();
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(2, 1);
		((ImU8String)(ref val)).AppendLiteral("##");
		((ImU8String)(ref val)).AppendFormatted<string>(newSettingName);
		ImGui.Checkbox(val, ref newSet);
		if (!(secondaryText != string.Empty))
		{
			return;
		}
		IndentDisposable val2 = ImRaii.PushIndent(1, true);
		try
		{
			TextWrapDisposable val3 = ImRaii.TextWrapPos(ImGui.GetContentRegionMax().X * 0.65f);
			try
			{
				ImGui.TextWrapped(ImU8String.op_Implicit(secondaryText));
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	public static void BuildDialog(ref float newSet, float newDefault, string tooltipString, string newSettingName, string secondaryText)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		ImGui.AlignTextToFramePadding();
		ImGui.Text(ImU8String.op_Implicit(newSettingName));
		if (tooltipString != string.Empty)
		{
			DrawHint(tooltipString);
		}
		string text = $"Default: {newDefault}";
		float num = ImGui.GetContentRegionAvail().X - 80f - ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f).X;
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SameLine(num - ((ImGuiStylePtr)(ref style)).FramePadding.X);
		ImGui.TextDisabled(ImU8String.op_Implicit(text));
		ImGui.SameLine();
		ItemWidthDisposable val = ImRaii.ItemWidth(80f);
		try
		{
			ImU8String val2 = new ImU8String(2, 1);
			((ImU8String)(ref val2)).AppendLiteral("##");
			((ImU8String)(ref val2)).AppendFormatted<string>(newSettingName);
			ImGui.InputFloat(val2, ref newSet, 0f, 0f, default(ImU8String), (ImGuiInputTextFlags)0);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		if (!(secondaryText != string.Empty))
		{
			return;
		}
		IndentDisposable val3 = ImRaii.PushIndent(1, true);
		try
		{
			TextWrapDisposable val4 = ImRaii.TextWrapPos(ImGui.GetContentRegionMax().X * 0.65f);
			try
			{
				ImGui.TextWrapped(ImU8String.op_Implicit(secondaryText));
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
	}

	public static void BuildDialog(ref int newSet, int newDefault, string tooltipString, string newSettingName, string secondaryText)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		ImGui.AlignTextToFramePadding();
		ImGui.Text(ImU8String.op_Implicit(newSettingName));
		if (tooltipString != string.Empty)
		{
			DrawHint(tooltipString);
		}
		string text = $"Default: {newDefault}";
		float num = ImGui.GetContentRegionAvail().X - 80f - ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f).X;
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SameLine(num - ((ImGuiStylePtr)(ref style)).FramePadding.X);
		ImGui.TextDisabled(ImU8String.op_Implicit(text));
		ImGui.SameLine();
		ItemWidthDisposable val = ImRaii.ItemWidth(80f);
		try
		{
			ImU8String val2 = new ImU8String(2, 1);
			((ImU8String)(ref val2)).AppendLiteral("##");
			((ImU8String)(ref val2)).AppendFormatted<string>(newSettingName);
			ImGui.InputInt(val2, ref newSet, 0, 0, default(ImU8String), (ImGuiInputTextFlags)0);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		if (!(secondaryText != string.Empty))
		{
			return;
		}
		IndentDisposable val3 = ImRaii.PushIndent(1, true);
		try
		{
			TextWrapDisposable val4 = ImRaii.TextWrapPos(ImGui.GetContentRegionMax().X * 0.65f);
			try
			{
				ImGui.TextWrapped(ImU8String.op_Implicit(secondaryText));
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
	}

	private static void DrawHint(string tooltipString)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		ImGui.SameLine();
		Icons.DrawIcon((FontAwesomeIcon)61529);
		if (ImGui.IsItemHovered())
		{
			TooltipDisposable val = ImRaii.Tooltip();
			try
			{
				ImGui.Text(ImU8String.op_Implicit(tooltipString));
			}
			finally
			{
				((TooltipDisposable)(ref val)).Dispose();
			}
		}
	}
}
