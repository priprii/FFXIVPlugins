using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace PartyRangePyon;

public class MainWindow : Window
{
	public enum Tab
	{
		Config
	}

	private Plugin plugin;

	private Tab CurrentTab;

	private string[] FontNames = new string[7] { "Default", "Axis", "Jupiter", "Jupiter Numeric", "Meidinger", "Meidinger Mid", "Trump Gothic" };

	public MainWindow(Plugin plugin)
		: base("PartyRangePyon", (ImGuiWindowFlags)0, false)
	{
		this.plugin = plugin;
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(400f, 500f) * ImGuiHelpers.GlobalScale;
	}

	public override void OnOpen()
	{
		((Window)this).OnOpen();
	}

	public override void Draw()
	{
		if (ImGui.BeginTabBar("PartyRangePyonTabBar", (ImGuiTabBarFlags)32))
		{
			if (ImGui.BeginTabItem("Config"))
			{
				CurrentTab = Tab.Config;
				ImGui.EndTabItem();
			}
			ImGui.EndTabBar();
			ImGui.Spacing();
		}
		if (CurrentTab == Tab.Config)
		{
			DrawConfig();
		}
		else
		{
			DrawConfig();
		}
	}

	private void DrawConfig()
	{
		ImGui.Columns(1);
		ImGuiEx.Checkbox("Enable", Plugin.Config, "Enabled");
		ImGui.SameLine();
		ImGui.Checkbox("Preview", ref plugin.DebugMode);
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("You can enable this to preview changes if you're not in a party.\nThe distance value will use target distance instead for previewing.");
		}
		ImGui.Separator();
		ImGuiEx.DragFloat("CloseRangeMax", Plugin.Config, "CloseRangeMax", 0.1f, 0f, 100f);
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("The max distance from you to consider a player to be in close range.\nDistance above this will be considered mid/far range.");
		}
		ImGuiEx.DragFloat("MidRangeMax", Plugin.Config, "MidRangeMax", 0.1f, 0f, 100f);
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("The max distance from you to consider a player to be in mid range.\nDistance above this will be considered far range.");
		}
		ImGui.Separator();
		ImGuiEx.InputText("TextFormat", Plugin.Config, "TextFormat");
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("How the text should be formatted, examples for 7.5 yalms:\n0.0y = 7.5y\n0 = 7\n00 = 07\nDistance: 00.00 Yalms = Distance: 07.50 Yalms");
		}
		if (ImGuiEx.Combo("FontName", Plugin.Config, "Font", FontNames, FontNames.Length))
		{
			plugin.UpdateFont();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("The font to display text in.");
		}
		if (ImGuiEx.DragInt("FontSize", Plugin.Config, "FontSize", 1f, 1, 124))
		{
			plugin.UpdateFont(delayUpdate: true);
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("The size of the font.");
		}
		ImGuiEx.DragFloat("FontScale", Plugin.Config, "FontScale", 0.01f, 0.01f, 2f);
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Scale of the font relative to the size.");
		}
		ImGuiEx.DragInt("TextOutline", Plugin.Config, "TextOutline", 1f, 0, 10);
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Thickness of outline around text.\n0 to disable outline.");
		}
		ImGuiEx.DragFloat("TextPosX", Plugin.Config, "TextPosX", 0.1f, -300f, 300f);
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("X position of the text offset in the party list item.");
		}
		ImGuiEx.DragFloat("TextPosY", Plugin.Config, "TextPosY", 0.1f, -300f, 300f);
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Y position of the text offset in the party list item.");
		}
		ImGui.Separator();
		ImGuiEx.ColorEdit4("OutlineColour", Plugin.Config, "OutlineColour");
		ImGuiEx.ColorEdit4("CloseRangeColour", Plugin.Config, "CloseRangeColour");
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Colour of the text when the player is in close range.");
		}
		ImGuiEx.ColorEdit4("MidRangeColour", Plugin.Config, "MidRangeColour");
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Colour of the text when the player is in mid range.");
		}
		ImGuiEx.ColorEdit4("FarRangeColour", Plugin.Config, "FarRangeColour");
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Colour of the text when the player is in far range.");
		}
		ImGui.Separator();
		ImGuiEx.DragInt("UpdateMs", Plugin.Config, "UpdateMs", 1f, 10, 500);
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Frequency of drawing to the party list, should be a low value to prevent flickering.");
		}
		ImGui.Separator();
		if (ImGui.Button("Save"))
		{
			Plugin.Config.Save();
		}
		ImGui.SameLine();
		if (ImGui.Button("Close"))
		{
			((Window)this).IsOpen = false;
			plugin.DebugMode = false;
		}
	}
}
