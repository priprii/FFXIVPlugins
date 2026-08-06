using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

namespace PartyPyon;

public class MainWindow : Window
{
	private readonly Plugin plugin;

	private PFManager PFManager;

	public MainWindow(Plugin plugin, PFManager pFManager)
		: base("PartyPyon")
	{
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(550f, 230f) * ImGuiHelpers.GlobalScale;
		this.plugin = plugin;
		PFManager = pFManager;
	}

	public override void Draw()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e2: Unknown result type (might be due to invalid IL or missing references)
		if (ImGui.Button(ImU8String.op_Implicit("Open PF"), default(Vector2)))
		{
			PFManager.PFOpen();
		}
		if (PFManager.Enabled)
		{
			ImGui.SameLine();
			if (ImGui.Button(ImU8String.op_Implicit("Stop"), default(Vector2)))
			{
				PFManager.Disable();
			}
		}
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		if (!PFManager.IsPFRecruitingOrUpdating)
		{
			if (PFManager.PFRecruitChangeWait != DateTime.MinValue)
			{
				ImGui.PushFont(UiBuilder.IconFont);
				Vector4 dalamudYellow = ImGuiColors.DalamudYellow;
				ImGui.TextColored(ref dalamudYellow, ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61553)));
				ImGui.PopFont();
				ImGui.SameLine();
				dalamudYellow = ImGuiColors.DalamudYellow;
				ImU8String val = default(ImU8String);
				((ImU8String)(ref val))._002Ector(30, 1);
				((ImU8String)(ref val)).AppendLiteral("PF Listing Lost, ending in ");
				((ImU8String)(ref val)).AppendFormatted<double>(Math.Ceiling((PFManager.PFRecruitChangeWait - DateTime.Now).TotalSeconds));
				((ImU8String)(ref val)).AppendLiteral("s..");
				ImGui.TextColored(ref dalamudYellow, val);
			}
			else
			{
				ImGui.Text(ImU8String.op_Implicit("Create/Select a PF template below to start auto relisting with comment from template."));
			}
		}
		else if (!PFManager.Enabled)
		{
			ImGui.Text(ImU8String.op_Implicit("Create/Select a PF template below to start auto relisting with comment from template."));
		}
		else if (PFManager.Enabled)
		{
			if (PFManager.PFRecruitChangeWait != DateTime.MinValue)
			{
				ImGui.PushFont(UiBuilder.IconFont);
				Vector4 dalamudYellow = ImGuiColors.DalamudYellow;
				ImGui.TextColored(ref dalamudYellow, ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61553)));
				ImGui.PopFont();
				ImGui.SameLine();
				dalamudYellow = ImGuiColors.DalamudYellow;
				ImU8String val2 = default(ImU8String);
				((ImU8String)(ref val2))._002Ector(30, 1);
				((ImU8String)(ref val2)).AppendLiteral("PF Listing Lost, ending in ");
				((ImU8String)(ref val2)).AppendFormatted<double>(Math.Ceiling((PFManager.PFRecruitChangeWait - DateTime.Now).TotalSeconds));
				((ImU8String)(ref val2)).AppendLiteral("s..");
				ImGui.TextColored(ref dalamudYellow, val2);
			}
			else if (PFManager.PFExpirationTime > DateTime.Now)
			{
				ImU8String val3 = default(ImU8String);
				((ImU8String)(ref val3))._002Ector(14, 1);
				((ImU8String)(ref val3)).AppendLiteral("Relisting In: ");
				((ImU8String)(ref val3)).AppendFormatted<TimeSpan>(PFManager.PFExpirationTime - DateTime.Now, "hh\\:mm\\:ss");
				ImGui.Text(val3);
			}
			else if (PFManager.PFExpirationTime == DateTime.MinValue)
			{
				ImU8String val4 = default(ImU8String);
				((ImU8String)(ref val4))._002Ector(11, 0);
				((ImU8String)(ref val4)).AppendLiteral("Relisting..");
				ImGui.Text(val4);
			}
		}
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit("Templates"));
		ImGui.SameLine();
		if (ImGuiEx.IconButton((FontAwesomeIcon)61543))
		{
			plugin.Config.Templates.Add(Guid.NewGuid(), "");
			plugin.Config.Save();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Add New Template"));
		}
		ImGui.Separator();
		float num = ImGui.GetTextLineHeightWithSpacing() * 2f;
		float fontSize = ImGui.GetFontSize();
		ImGuiStylePtr style = ImGui.GetStyle();
		float num2 = fontSize + ((ImGuiStylePtr)(ref style)).FramePadding.X * 2f;
		float fontSize2 = ImGui.GetFontSize();
		style = ImGui.GetStyle();
		float num3 = fontSize2 + ((ImGuiStylePtr)(ref style)).FramePadding.Y * 2f;
		float num4 = (num - num3) * 0.5f;
		style = ImGui.GetStyle();
		float num5 = ((ImGuiStylePtr)(ref style)).FramePadding.X * ImGuiHelpers.GlobalScale;
		foreach (KeyValuePair<Guid, string> item in plugin.Config.Templates.ToList())
		{
			ImGui.PushID(ImU8String.op_Implicit(item.Key.ToString()));
			Vector2 cursorPos = ImGui.GetCursorPos();
			float num6 = cursorPos.X + num5;
			bool flag = plugin.Config.SelectedTemplate == item.Key;
			ImGui.SetCursorPos(new Vector2(num6, cursorPos.Y + num4));
			ImGui.BeginDisabled(PFManager.IsUpdating || PFManager.IsProcessingActions);
			if (ImGui.Checkbox(ImU8String.op_Implicit("##selected"), ref flag))
			{
				plugin.Config.SelectedTemplate = ((!flag) ? ((Guid?)null) : new Guid?(item.Key));
				plugin.Config.Save();
				plugin.PFManager.SelectionChanged();
			}
			ImGui.EndDisabled();
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Select Template"));
			}
			float num7 = num6 + num2 + num5;
			float num8 = ImGui.GetContentRegionAvail().X - num7 * 2f;
			string text = item.Value;
			ImGui.SetCursorPos(new Vector2(num7, cursorPos.Y));
			if (ImGuiEx.InputTextMultilineWithHint(plugin, "##comment", ref text, 192, new Vector2(num8, num), "Party Finder Comment", 2, (ImGuiInputTextFlags)0))
			{
				plugin.Config.Templates[item.Key] = text;
				plugin.Config.Save();
			}
			ImGui.SetCursorPos(new Vector2(num7 + num8 + num5 * 2f, cursorPos.Y + num4));
			if (ImGuiEx.IconButton((FontAwesomeIcon)61944))
			{
				if (flag)
				{
					plugin.Config.SelectedTemplate = null;
				}
				plugin.Config.Templates.Remove(item.Key);
				plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Delete Template"));
			}
			ImGui.SetCursorPosY(cursorPos.Y + num);
			ImGui.Separator();
			ImGui.PopID();
		}
	}
}
