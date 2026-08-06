using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace PvPyon;

public class MainWindow : Window
{
	private readonly PvPyon plugin;

	public static Config Config { get; set; }

	public MainWindow(PvPyon plugin)
		: base("PvPyon", (ImGuiWindowFlags)0, false)
	{
		((Window)this).SizeCondition = (ImGuiCond)8;
		((Window)this).Size = new Vector2(400f, 240f) * ImGuiHelpers.GlobalScale;
		this.plugin = plugin;
	}

	public void Initialize()
	{
	}

	public void Dispose()
	{
	}

	public override void OnOpen()
	{
		((Window)this).OnOpen();
	}

	public override void Draw()
	{
		ImGuiEx.Checkbox("Enabled###enabled", Config, "Enabled");
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Enable to show names of enemies in PvP content.\nAlso makes it easier to see their job class.");
		}
		ImGuiEx.Checkbox("Filter Players###filterPlayers", Config, "FilterPlayers");
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("If enabled, only show enemy name if: They are a FC member, friend, or included in the box below.\nThis makes it easy to hunt Seibaaa :3");
		}
		ImGui.SetNextItemWidth(-1f);
		ImGuiEx.InputText("###includedNames", Config, "IncludedNames", 1000u);
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Comma separated list of player names (not case-sensitive) to include when the above Filter Players option is enabled.\nEg: Primu pyon, miyu myon, etc..\nYou do not need to add names of FC members or friends.");
		}
		ImGui.Separator();
		ImGuiHelpers.ScaledDummy(5f);
		if (ImGui.Button("Save"))
		{
			Config.Save();
			((Window)this).IsOpen = false;
		}
		ImGui.SameLine();
		if (ImGui.Button("Close"))
		{
			((Window)this).IsOpen = false;
		}
	}
}
