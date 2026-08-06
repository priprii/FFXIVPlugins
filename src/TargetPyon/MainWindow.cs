using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Common.Math;
using TargetPyon.Extensions;

namespace TargetPyon;

public class MainWindow : Window
{
	public enum Tab
	{
		Config,
		Players,
		Objects
	}

	private readonly Plugin plugin;

	private readonly FileDialogManager FileDialogManager = new FileDialogManager();

	private Tab CurrentTab;

	private string[] FontNames = new string[7] { "Default", "Axis", "Jupiter", "Jupiter Numeric", "Meidinger", "Meidinger Mid", "Trump Gothic" };

	private TitleBarButton ResetCamTitleButton = new TitleBarButton
	{
		Icon = (FontAwesomeIcon)58557,
		ShowTooltip = delegate
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Reset Camera"));
			}
		},
		Click = delegate
		{
			IPC.ResetCamTarget();
		}
	};

	private TitleBarButton ToggleVisibilityButton = new TitleBarButton
	{
		Icon = (FontAwesomeIcon)(Plugin.Config.PlayerVisibilityFilter ? 58675 : 61447),
		ShowTooltip = delegate
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit(Plugin.Config.PlayerVisibilityFilter ? "Show players who are not in party/friend list" : "Hide players who are not in party/friend list"));
			}
		},
		Click = delegate
		{
			Plugin.ToggleAllVisibility();
		}
	};

	private TitleBarButton ToggleObjectVisibilityButton = new TitleBarButton
	{
		Icon = (FontAwesomeIcon)(Plugin.Config.ObjectVisibilityFilter ? 61875 : 61874),
		ShowTooltip = delegate
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit(Plugin.Config.ObjectVisibilityFilter ? "Show all non-player objects" : "Hide all non-player objects"));
			}
		},
		Click = delegate
		{
			Plugin.ToggleAllObjectVisibility();
		}
	};

	private string SearchText = "";

	private string ObjectSearchText = "";

	private int PlayerCount { get; set; }

	private int ObjectCount { get; set; }

	public MainWindow(Plugin plugin)
		: base("Target")
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Expected O, but got Unknown
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Expected O, but got Unknown
		this.plugin = plugin;
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(600f, 600f) * ImGuiHelpers.GlobalScale;
		((Window)this).TitleBarButtons.Add(ToggleVisibilityButton);
		((Window)this).TitleBarButtons.Add(ToggleObjectVisibilityButton);
	}

	public override void OnClose()
	{
		((Window)this).OnClose();
		if (Plugin.Config.CustomizationMode)
		{
			Plugin.Config.CustomizationMode = false;
			Plugin.Config.Save();
		}
	}

	public void AddResetCamTitleButton()
	{
		if (!((Window)this).TitleBarButtons.Contains(ResetCamTitleButton))
		{
			((Window)this).TitleBarButtons.Add(ResetCamTitleButton);
		}
	}

	public void RemoveResetCamTitleButton()
	{
		if (((Window)this).TitleBarButtons.Contains(ResetCamTitleButton))
		{
			((Window)this).TitleBarButtons.Remove(ResetCamTitleButton);
		}
	}

	public override void OnOpen()
	{
		((Window)this).OnOpen();
	}

	public override void Draw()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		ToggleVisibilityButton.Icon = (FontAwesomeIcon)(Plugin.Config.PlayerVisibilityFilter ? 58675 : 61447);
		ToggleObjectVisibilityButton.Icon = (FontAwesomeIcon)(Plugin.Config.ObjectVisibilityFilter ? 61875 : 61874);
		if (IPC.PyonCamEnabled)
		{
			if (IPC.GetCamTarget() != 0L)
			{
				AddResetCamTitleButton();
			}
			else
			{
				RemoveResetCamTitleButton();
			}
		}
		else
		{
			RemoveResetCamTitleButton();
		}
		if (ImGui.BeginTabBar(ImU8String.op_Implicit("TargetTabBar"), (ImGuiTabBarFlags)32))
		{
			if (ImGui.BeginTabItem(ImU8String.op_Implicit("Config###Target_ConfigTab"), (ImGuiTabItemFlags)0))
			{
				CurrentTab = Tab.Config;
				ImGui.EndTabItem();
			}
			if (ImGui.BeginTabItem(ImU8String.op_Implicit("Players###Target_PlayersTab"), (ImGuiTabItemFlags)0))
			{
				CurrentTab = Tab.Players;
				ImGui.EndTabItem();
			}
			if (ImGui.BeginTabItem(ImU8String.op_Implicit("Objects###Target_ObjectsTab"), (ImGuiTabItemFlags)0))
			{
				CurrentTab = Tab.Objects;
				ImGui.EndTabItem();
			}
			ImGui.EndTabBar();
			ImGui.Spacing();
		}
		switch (CurrentTab)
		{
		case Tab.Config:
			DrawConfig();
			break;
		case Tab.Players:
			DrawPlayers();
			break;
		case Tab.Objects:
			DrawObjects();
			break;
		default:
			DrawConfig();
			break;
		}
	}

	private void DrawConfig()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Columns(1, ImU8String.op_Implicit(""), false);
		Vector4 dalamudViolet = ImGuiColors.DalamudViolet;
		ImGui.TextColored(ref dalamudViolet, ImU8String.op_Implicit("Configure properties of the overlay listing players targeting you."));
		dalamudViolet = ImGuiColors.DalamudViolet;
		ImGui.TextColored(ref dalamudViolet, ImU8String.op_Implicit("Note: Increase BG Opacity to see where the overlay list is on screen."));
		DrawConfigInteraction();
		ImGui.Separator();
		DrawConfigNotification();
		ImGui.Separator();
		DrawConfigOverlay();
		ImGui.Separator();
		DrawConfigMarker();
		ImGui.Separator();
		DrawConfigFormatting();
	}

	private void DrawConfigInteraction()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0506: Unknown result type (might be due to invalid IL or missing references)
		//IL_051c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0633: Unknown result type (might be due to invalid IL or missing references)
		//IL_0649: Unknown result type (might be due to invalid IL or missing references)
		//IL_0760: Unknown result type (might be due to invalid IL or missing references)
		//IL_0776: Unknown result type (might be due to invalid IL or missing references)
		//IL_08da: Unknown result type (might be due to invalid IL or missing references)
		//IL_089e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0900: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0916: Unknown result type (might be due to invalid IL or missing references)
		if (!ImGui.CollapsingHeader(ImU8String.op_Implicit("Interaction"), (ImGuiTreeNodeFlags)32))
		{
			return;
		}
		ImGui.Columns(3, ImU8String.op_Implicit(""), false);
		if (ImGuiEx.Checkbox("Enable##toggleEnabled", Plugin.Config, "Enabled"))
		{
			((Window)Plugin.OverlayWindow).IsOpen = Plugin.Config.Enabled;
			Plugin.Config.Save();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Display list of players targeting you in the overlay."));
		}
		ImGui.NextColumn();
		if (ImGuiEx.Checkbox("Soft Target##softTarget", Plugin.Config, "IncludeSoftTarget"))
		{
			Plugin.Config.Save();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Include players who are 'soft targeting' you.\n'Soft targeting' is the act of cycling through targets without confirming selection of target."));
		}
		ImGui.NextColumn();
		if (ImGuiEx.Checkbox("Customization Mode##custMode", Plugin.Config, "CustomizationMode"))
		{
			Plugin.Config.Save();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Toggle 'Customization Mode' which will display names in the overlay list to preview customization.\nYou can disable this when you are done customizing.\nWill be automatically disabled when closing this window."));
		}
		ImGui.NextColumn();
		if (ImGuiEx.Checkbox("ClickThrough##clickThrough", Plugin.Config, "ClickThrough"))
		{
			Plugin.Config.Save();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Prevent click events on the overlay."));
		}
		ImGui.NextColumn();
		if (ImGuiEx.Checkbox("Locked##lockPos", Plugin.Config, "LockPosition"))
		{
			Plugin.Config.Save();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Prevent moving the overlay when dragged."));
		}
		ImGui.NextColumn();
		ImGui.Dummy(Vector2.Zero);
		ImGui.NextColumn();
		ImGui.Separator();
		ImGui.Text(ImU8String.op_Implicit("ClickThrough Bypass"));
		ImGui.NextColumn();
		ImGui.Dummy(Vector2.Zero);
		ImGui.NextColumn();
		ImGui.Dummy(Vector2.Zero);
		ImGui.NextColumn();
		ImGui.BeginDisabled(!Plugin.Config.ClickThrough);
		if (ImGuiEx.Checkbox("Ctrl", Plugin.Config, "ClickThroughBypassCtrl"))
		{
			Plugin.Config.Save();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("When ClickThrough is enabled, Overlay will be interactable while the Ctrl key is held down."));
		}
		ImGui.NextColumn();
		if (ImGuiEx.Checkbox("Shift", Plugin.Config, "ClickThroughBypassShift"))
		{
			Plugin.Config.Save();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("When ClickThrough is enabled, Overlay will be interactable while the Shift key is held down."));
		}
		ImGui.NextColumn();
		if (ImGuiEx.Checkbox("Alt", Plugin.Config, "ClickThroughBypassAlt"))
		{
			Plugin.Config.Save();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("When ClickThrough is enabled, Overlay will be interactable while the Alt key is held down."));
		}
		ImGui.EndDisabled();
		ImGui.NextColumn();
		ImGui.Separator();
		ImGui.Text(ImU8String.op_Implicit("Mouse Bindings"));
		ImGui.NextColumn();
		ImGui.Dummy(Vector2.Zero);
		ImGui.NextColumn();
		ImGui.Dummy(Vector2.Zero);
		ImGui.NextColumn();
		ImGui.Text(ImU8String.op_Implicit("Target Player"));
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Click player name in the overlay to target them."));
		}
		ImGui.NextColumn();
		if (ImGuiEx.Combo("##targetBinding", Plugin.Config, "TargetClickButton", new string[4] { "Left Click", "Right Click", "Middle Click", "None" }, 4))
		{
			if (Plugin.Config.RemoveClickButton == Plugin.Config.TargetClickButton)
			{
				Plugin.Config.RemoveClickButton = ClickButton.None;
			}
			if (Plugin.Config.PlateClickButton == Plugin.Config.TargetClickButton)
			{
				Plugin.Config.PlateClickButton = ClickButton.None;
			}
			if (Plugin.Config.InspectClickButton == Plugin.Config.TargetClickButton)
			{
				Plugin.Config.InspectClickButton = ClickButton.None;
			}
			if (Plugin.Config.CamOrbitClickButton == Plugin.Config.TargetClickButton)
			{
				Plugin.Config.CamOrbitClickButton = ClickButton.None;
			}
			if (Plugin.Config.ContextClickButton == Plugin.Config.TargetClickButton)
			{
				Plugin.Config.ContextClickButton = ClickButton.None;
			}
			Plugin.Config.Save();
		}
		ImGui.NextColumn();
		ImGui.Dummy(Vector2.Zero);
		ImGui.NextColumn();
		ImGui.Text(ImU8String.op_Implicit("Remove Player"));
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Click player name in the overlay to remove from the list."));
		}
		ImGui.NextColumn();
		if (ImGuiEx.Combo("##removeBinding", Plugin.Config, "RemoveClickButton", new string[4] { "Left Click", "Right Click", "Middle Click", "None" }, 4))
		{
			if (Plugin.Config.TargetClickButton == Plugin.Config.RemoveClickButton)
			{
				Plugin.Config.TargetClickButton = ClickButton.None;
			}
			if (Plugin.Config.PlateClickButton == Plugin.Config.RemoveClickButton)
			{
				Plugin.Config.PlateClickButton = ClickButton.None;
			}
			if (Plugin.Config.InspectClickButton == Plugin.Config.RemoveClickButton)
			{
				Plugin.Config.InspectClickButton = ClickButton.None;
			}
			if (Plugin.Config.CamOrbitClickButton == Plugin.Config.RemoveClickButton)
			{
				Plugin.Config.CamOrbitClickButton = ClickButton.None;
			}
			if (Plugin.Config.ContextClickButton == Plugin.Config.RemoveClickButton)
			{
				Plugin.Config.ContextClickButton = ClickButton.None;
			}
			Plugin.Config.Save();
		}
		ImGui.NextColumn();
		ImGui.Dummy(Vector2.Zero);
		ImGui.NextColumn();
		ImGui.Text(ImU8String.op_Implicit("Adventure Plate"));
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Click player name in the overlay to open their adventure plate."));
		}
		ImGui.NextColumn();
		if (ImGuiEx.Combo("##plateBinding", Plugin.Config, "PlateClickButton", new string[4] { "Left Click", "Right Click", "Middle Click", "None" }, 4))
		{
			if (Plugin.Config.TargetClickButton == Plugin.Config.PlateClickButton)
			{
				Plugin.Config.TargetClickButton = ClickButton.None;
			}
			if (Plugin.Config.RemoveClickButton == Plugin.Config.PlateClickButton)
			{
				Plugin.Config.RemoveClickButton = ClickButton.None;
			}
			if (Plugin.Config.InspectClickButton == Plugin.Config.PlateClickButton)
			{
				Plugin.Config.InspectClickButton = ClickButton.None;
			}
			if (Plugin.Config.CamOrbitClickButton == Plugin.Config.PlateClickButton)
			{
				Plugin.Config.CamOrbitClickButton = ClickButton.None;
			}
			if (Plugin.Config.ContextClickButton == Plugin.Config.PlateClickButton)
			{
				Plugin.Config.ContextClickButton = ClickButton.None;
			}
			Plugin.Config.Save();
		}
		ImGui.NextColumn();
		ImGui.Dummy(Vector2.Zero);
		ImGui.NextColumn();
		ImGui.Text(ImU8String.op_Implicit("Examine"));
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Click player name in the overlay to open their character window."));
		}
		ImGui.NextColumn();
		if (ImGuiEx.Combo("##examineBinding", Plugin.Config, "InspectClickButton", new string[4] { "Left Click", "Right Click", "Middle Click", "None" }, 4))
		{
			if (Plugin.Config.TargetClickButton == Plugin.Config.InspectClickButton)
			{
				Plugin.Config.TargetClickButton = ClickButton.None;
			}
			if (Plugin.Config.RemoveClickButton == Plugin.Config.InspectClickButton)
			{
				Plugin.Config.RemoveClickButton = ClickButton.None;
			}
			if (Plugin.Config.PlateClickButton == Plugin.Config.InspectClickButton)
			{
				Plugin.Config.PlateClickButton = ClickButton.None;
			}
			if (Plugin.Config.CamOrbitClickButton == Plugin.Config.InspectClickButton)
			{
				Plugin.Config.CamOrbitClickButton = ClickButton.None;
			}
			if (Plugin.Config.ContextClickButton == Plugin.Config.InspectClickButton)
			{
				Plugin.Config.ContextClickButton = ClickButton.None;
			}
			Plugin.Config.Save();
		}
		ImGui.NextColumn();
		ImGui.Dummy(Vector2.Zero);
		ImGui.NextColumn();
		ImGui.Text(ImU8String.op_Implicit("Camera Orbit"));
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Click player name in the overlay to toggle locking camera orbit on their character."));
		}
		ImGui.NextColumn();
		if (ImGuiEx.Combo("##camBinding", Plugin.Config, "CamOrbitClickButton", new string[4] { "Left Click", "Right Click", "Middle Click", "None" }, 4))
		{
			if (Plugin.Config.TargetClickButton == Plugin.Config.CamOrbitClickButton)
			{
				Plugin.Config.TargetClickButton = ClickButton.None;
			}
			if (Plugin.Config.RemoveClickButton == Plugin.Config.CamOrbitClickButton)
			{
				Plugin.Config.RemoveClickButton = ClickButton.None;
			}
			if (Plugin.Config.PlateClickButton == Plugin.Config.CamOrbitClickButton)
			{
				Plugin.Config.PlateClickButton = ClickButton.None;
			}
			if (Plugin.Config.InspectClickButton == Plugin.Config.CamOrbitClickButton)
			{
				Plugin.Config.InspectClickButton = ClickButton.None;
			}
			if (Plugin.Config.ContextClickButton == Plugin.Config.CamOrbitClickButton)
			{
				Plugin.Config.ContextClickButton = ClickButton.None;
			}
			Plugin.Config.Save();
		}
		ImGui.NextColumn();
		if (IPC.PyonCamEnabled)
		{
			Vector4 healerGreen = ImGuiColors.HealerGreen;
			ImU8String val = default(ImU8String);
			((ImU8String)(ref val))._002Ector(15, 0);
			((ImU8String)(ref val)).AppendLiteral("PyonCam Enabled");
			ImGui.TextColored(ref healerGreen, val);
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("TargetPyon is poking PyonCam plugin for Camera Orbit functionality!"));
			}
		}
		else
		{
			Vector4 healerGreen = ImGuiColors.DPSRed;
			ImU8String val2 = default(ImU8String);
			((ImU8String)(ref val2))._002Ector(16, 0);
			((ImU8String)(ref val2)).AppendLiteral("PyonCam Disabled");
			ImGui.TextColored(ref healerGreen, val2);
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("You need the PyonCam plugin to use Camera Orbit functionality."));
			}
		}
		ImGui.NextColumn();
		ImGui.Text(ImU8String.op_Implicit("Context Menu"));
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Click player name in the overlay to display the above functions in a context menu."));
		}
		ImGui.NextColumn();
		if (ImGuiEx.Combo("##contextBinding", Plugin.Config, "ContextClickButton", new string[4] { "Left Click", "Right Click", "Middle Click", "None" }, 4))
		{
			if (Plugin.Config.TargetClickButton == Plugin.Config.ContextClickButton)
			{
				Plugin.Config.TargetClickButton = ClickButton.None;
			}
			if (Plugin.Config.RemoveClickButton == Plugin.Config.ContextClickButton)
			{
				Plugin.Config.RemoveClickButton = ClickButton.None;
			}
			if (Plugin.Config.PlateClickButton == Plugin.Config.ContextClickButton)
			{
				Plugin.Config.PlateClickButton = ClickButton.None;
			}
			if (Plugin.Config.InspectClickButton == Plugin.Config.ContextClickButton)
			{
				Plugin.Config.InspectClickButton = ClickButton.None;
			}
			if (Plugin.Config.CamOrbitClickButton == Plugin.Config.ContextClickButton)
			{
				Plugin.Config.CamOrbitClickButton = ClickButton.None;
			}
			Plugin.Config.Save();
		}
		ImGui.NextColumn();
		ImGui.Dummy(Vector2.Zero);
		ImGui.NextColumn();
	}

	private void DrawConfigNotification()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Columns(1, ImU8String.op_Implicit(""), false);
		if (!ImGui.CollapsingHeader(ImU8String.op_Implicit("Notification"), (ImGuiTreeNodeFlags)0))
		{
			return;
		}
		ImGui.Columns(3, ImU8String.op_Implicit(""), false);
		ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale * 80f);
		ImGui.BeginDisabled(Plugin.Config.UseCustomAudioAlert);
		if (ImGuiEx.InputInt("Audio Alert##audioAlert", Plugin.Config, "SoundID"))
		{
			if (Plugin.Config.SoundID < 0)
			{
				Plugin.Config.SoundID = 0;
			}
			else if (Plugin.Config.SoundID > 16)
			{
				Plugin.Config.SoundID = 16;
			}
			Plugin.Config.Save();
			plugin.PlaySound("");
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("The audio alert to play when being targeted.\nSet to 0 to disable audio.\nThis option is ignored if 'Use Custom Audio' is enabled."));
		}
		ImGui.EndDisabled();
		ImGui.NextColumn();
		if (ImGuiEx.Checkbox("Chat Alert##chatAlert", Plugin.Config, "ChatAlert"))
		{
			Plugin.Config.Save();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Output a message to the chatlog when being targeted.\nThis option is ignored in PvP duties."));
		}
		ImGui.NextColumn();
		ImGui.Dummy(Vector2.Zero);
		ImGui.NextColumn();
		if (ImGuiEx.Checkbox("Use Custom Audio##useCustomAudio", Plugin.Config, "UseCustomAudioAlert"))
		{
			Plugin.Config.Save();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Use custom audio alert instead of the above default sound.\nSelect the audio file to use for the alert with the Browse button to the right."));
		}
		ImGui.SameLine();
		if (ImGuiEx.IconButton((FontAwesomeIcon)61563))
		{
			FileDialogManager.OpenFileDialog("Select Audio Alert", "Audio Files (*.mp3 *.aac *.wma *.wav){.mp3,.aac,.wma,.wav}", (Action<bool, List<string>>)delegate(bool success, List<string> path)
			{
				if (success && path.Count() == 1)
				{
					plugin.AudioManager.SetAudioFile(path[0]);
				}
			}, 1, (string)null, false);
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Select an audio file to use as the audio alert."));
		}
		FileDialogManager.Draw();
		ImGui.SameLine();
		ImGui.BeginDisabled(!plugin.AudioManager.AudioFileExists);
		if (ImGuiEx.IconButton((FontAwesomeIcon)61515))
		{
			plugin.AudioManager.Play();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Test the audio alert."));
		}
		ImGui.EndDisabled();
		ImGui.NextColumn();
		if (ImGuiEx.SliderInt("Volume##audioVolume", Plugin.Config, "AudioVolume", 0, 100))
		{
			Plugin.Config.Save();
		}
		ImGui.NextColumn();
		if (ImGuiEx.Checkbox("Use Game SFX Volume##useGameSFXVolume", Plugin.Config, "UseGameSFXVolume"))
		{
			Plugin.Config.Save();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Max volume of audio alert will respect the SFX Volume set in FFXIV Settings."));
		}
		ImGui.NextColumn();
		ImGui.Separator();
		if (ImGuiEx.Checkbox("NoDuty Alert##ndAlly", Plugin.Config, "NoDutyAllyAlert"))
		{
			Plugin.Config.Save();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Play audio alert when a player in non-duty content targets you."));
		}
		ImGui.NextColumn();
		if (ImGuiEx.Checkbox("PvE Ally Alert##pveAlly", Plugin.Config, "PvEAllyAlert"))
		{
			Plugin.Config.Save();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Play audio alert when an ally in a PvE duty targets you."));
		}
		ImGui.NextColumn();
		if (ImGuiEx.Checkbox("PvP Ally Alert##pvpAlly", Plugin.Config, "PvPAllyAlert"))
		{
			Plugin.Config.Save();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Play audio alert when an ally in a PvP duty targets you."));
		}
		ImGui.NextColumn();
		ImGui.Dummy(Vector2.Zero);
		ImGui.NextColumn();
		ImGui.Dummy(Vector2.Zero);
		ImGui.NextColumn();
		if (ImGuiEx.Checkbox("PvP Enemy Alert##pvpEnemy", Plugin.Config, "PvPEnemyAlert"))
		{
			Plugin.Config.Save();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Play audio alert when an enemy in a PvP duty targets you.\nThis can be pretty noisy in Frontlines,\nbut I think it's nice to know when someone wants to murder you ;w;"));
		}
		ImGui.NextColumn();
	}

	private void DrawConfigOverlay()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0547: Unknown result type (might be due to invalid IL or missing references)
		//IL_0599: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Columns(1, ImU8String.op_Implicit(""), false);
		if (ImGui.CollapsingHeader(ImU8String.op_Implicit("Overlay"), (ImGuiTreeNodeFlags)0))
		{
			ImGui.Columns(2, ImU8String.op_Implicit(""), false);
			if (ImGuiEx.Checkbox("Only Nearby Players##onlyNearby", Plugin.Config, "OnlyShowNearbyPlayers"))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Only players within range will be visible in the overlay.\nThey will be removed if too far away or in a different zone."));
			}
			ImGui.NextColumn();
			if (ImGuiEx.DragInt("Max Players##maxPlayers", Plugin.Config, "MaxPlayers", 1f, 1, 50))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Max number of players to display in the overlay."));
			}
			ImGui.NextColumn();
			if (ImGuiEx.DragInt("Duration##displayTime", Plugin.Config, "DisplayTime", 1f, 0, 1440))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Number of minutes until a name of a player no longer targeting you is removed from the overlay.\nSet to 0 to never automatically remove names."));
			}
			ImGui.NextColumn();
			if (ImGuiEx.DragInt("Update Ms##updateMs", Plugin.Config, "UpdateMs", 10f, 250, 5000))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Polling rate to check for players targeting you."));
			}
			ImGui.NextColumn();
			ImGui.Separator();
			ImGui.LabelText(ImU8String.op_Implicit("##showTarget"), ImU8String.op_Implicit("Show Player's Target"));
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Show who each player listed in the overlay is targeting, when they're not targeting you."));
			}
			ImGui.NextColumn();
			if (ImGui.RadioButton(ImU8String.op_Implicit("Disabled##st0"), Plugin.Config.ShowTarget == 0))
			{
				Plugin.Config.ShowTarget = 0;
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Do not show who each player is targeting when they're not targeting you."));
			}
			ImGui.SameLine();
			if (ImGui.RadioButton(ImU8String.op_Implicit("Overlay##st1"), Plugin.Config.ShowTarget == 1))
			{
				Plugin.Config.ShowTarget = 1;
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Show the player's target under the player's name in the overlay."));
			}
			ImGui.SameLine();
			if (ImGui.RadioButton(ImU8String.op_Implicit("Hover##st2"), Plugin.Config.ShowTarget == 2))
			{
				Plugin.Config.ShowTarget = 2;
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Show as a tooltip when player's name is hovered in the overlay."));
			}
			ImGui.NextColumn();
			ImGui.LabelText(ImU8String.op_Implicit("##showTargeters"), ImU8String.op_Implicit("Show Player's Targeters"));
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Show who each player listed in the overlay is being targeted by."));
			}
			ImGui.NextColumn();
			if (ImGui.RadioButton(ImU8String.op_Implicit("Disabled##stt0"), Plugin.Config.ShowTargeters == 0))
			{
				Plugin.Config.ShowTargeters = 0;
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Do not show who each player is targeted by."));
			}
			ImGui.SameLine();
			if (ImGui.RadioButton(ImU8String.op_Implicit("Overlay##stt1"), Plugin.Config.ShowTargeters == 1))
			{
				Plugin.Config.ShowTargeters = 1;
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Show the player's targeters under the player's name in the overlay."));
			}
			ImGui.SameLine();
			if (ImGui.RadioButton(ImU8String.op_Implicit("Hover##stt2"), Plugin.Config.ShowTargeters == 2))
			{
				Plugin.Config.ShowTargeters = 2;
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Show as a tooltip when player's name is hovered in the overlay."));
			}
			ImGui.NextColumn();
			ImGui.Separator();
			if (ImGuiEx.DragInt("Width##overlayWidth", Plugin.Config, "OverlayWidth", 1f, 40, 800))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Width of the overlay."));
			}
			ImGui.NextColumn();
			if (ImGuiEx.DragInt("Height##overlayHeight", Plugin.Config, "OverlayHeight", 1f, 40, 800))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Height of the overlay."));
			}
			ImGui.NextColumn();
			ImGui.Separator();
			if (ImGuiEx.DragFloat("BG Opacity##bgOpacity", Plugin.Config, "OverlayBGOpacity", 0.01f, 0f, 1f))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Background opacity of the overlay."));
			}
			ImGui.NextColumn();
			if (ImGuiEx.DragFloat("Font Scale##fontScale", Plugin.Config, "FontScale", 0.01f, 0f, 2f))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Scaling of player names in the overlay."));
			}
			ImGui.NextColumn();
			if (ImGuiEx.Combo("Font Name##fontName", Plugin.Config, "Font", FontNames, FontNames.Length))
			{
				plugin.UpdateFont();
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("The font to display player names in.\nAxis or Meidinger Mid are the suggested fonts to use."));
			}
			ImGui.NextColumn();
			if (ImGuiEx.DragInt("Font Size##fontSize", Plugin.Config, "FontSize", 1f, 1, 124))
			{
				plugin.UpdateFont(delayUpdate: true);
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("The size of the font."));
			}
			ImGui.NextColumn();
			ImGui.Dummy(Vector2.Zero);
			ImGui.NextColumn();
			if (ImGuiEx.DragInt("Font Outline##fontOutline", Plugin.Config, "FontOutline", 1f, 0, 10))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Thickness of outline around player names.\n0 to disable outline."));
			}
			ImGui.NextColumn();
		}
	}

	private void DrawConfigMarker()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Columns(1, ImU8String.op_Implicit(""), false);
		if (ImGui.CollapsingHeader(ImU8String.op_Implicit("Marker"), (ImGuiTreeNodeFlags)0))
		{
			ImGui.Columns(2, ImU8String.op_Implicit(""), false);
			if (ImGuiEx.Checkbox("Marker Only on Hover", Plugin.Config, "OnlyShowMarkerOnHover"))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("If enabled, marker on players targeting you will only be visible when you hover over their name in the overlay."));
			}
			ImGui.NextColumn();
			if (ImGuiEx.DragFloat("Marker Size##sizeMarker", Plugin.Config, "MarkerSize", 0.05f, 0f, 20f))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Size of the marker on players targeting you.\nSet to 0 to disable."));
			}
			ImGui.NextColumn();
			ImGui.Columns(1, ImU8String.op_Implicit(""), false);
			if (ImGuiEx.ColorPicker4("Marker Colour", "colMarker", Plugin.Config, "MarkerColour"))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Colour of marker on players targeting you."));
			}
			ImGui.NextColumn();
		}
	}

	private void DrawConfigFormatting()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_050b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0559: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f4: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Columns(1, ImU8String.op_Implicit(""), false);
		if (ImGui.CollapsingHeader(ImU8String.op_Implicit("Formatting"), (ImGuiTreeNodeFlags)0))
		{
			ImGui.Columns(1, ImU8String.op_Implicit(""), false);
			ImGui.Text(ImU8String.op_Implicit("Current Target"));
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Formatting options for players currently targeting you."));
			}
			ImGui.Spacing();
			ImGui.NextColumn();
			if (ImGuiEx.ColorPicker4("Text Colour", "colCurTar", Plugin.Config, "TargetColour"))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Text colour for players currently targeting you."));
			}
			ImGui.SameLine();
			if (ImGuiEx.ColorPicker4("Outline Colour", "colCurTarOutline", Plugin.Config, "OutlineColour"))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Outline colour for players currently targeting you."));
			}
			ImGui.SameLine();
			ImGui.SetNextItemWidth(200f);
			if (ImGuiEx.InputText("##curTarFormat", Plugin.Config, "CurrentTargetFormat"))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Format of the text for players currently targeting you.\n%%fn%% = Player Forename\n%%sn%% = Player Surname\n%%dir%% = Direction\n%%h%% / %%m%% / %%s%% = Hours / Minutes / Seconds"));
			}
			ImGui.NextColumn();
			ImGui.Separator();
			ImGui.Spacing();
			ImGui.Columns(1, ImU8String.op_Implicit(""), false);
			ImGui.Text(ImU8String.op_Implicit("Previous Target"));
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Formatting options for players previously targeting you."));
			}
			ImGui.Spacing();
			ImGui.NextColumn();
			if (ImGuiEx.ColorPicker4("Text Colour", "colPrevTar", Plugin.Config, "NoTargetColour"))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Text colour for players previously targeting you."));
			}
			ImGui.SameLine();
			if (ImGuiEx.ColorPicker4("Outline Colour", "colPrevTarOutline", Plugin.Config, "NoTargetOutlineColour"))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Outline colour for players previously targeting you."));
			}
			ImGui.SameLine();
			ImGui.SetNextItemWidth(200f);
			if (ImGuiEx.InputText("##prevTarFormat", Plugin.Config, "PreviousTargetFormat"))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Format of the text for players previously targeting you.\n%%fn%% = Player Forename\n%%sn%% = Player Surname\n%%dir%% = Direction\n%%h%% / %%m%% / %%s%% = Hours / Minutes / Seconds"));
			}
			ImGui.NextColumn();
			ImGui.Separator();
			ImGui.Spacing();
			ImGui.Columns(1, ImU8String.op_Implicit(""), false);
			ImGui.Text(ImU8String.op_Implicit("Player's Target"));
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Formatting options for player's target & targeters."));
			}
			ImGui.Spacing();
			ImGui.NextColumn();
			if (ImGuiEx.ColorPicker4("Text Colour", "colPlyTar", Plugin.Config, "PlayersTargetColour"))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Text colour for player's target & targeters."));
			}
			ImGui.SameLine();
			if (ImGuiEx.ColorPicker4("Outline Colour", "colPlyTarOutline", Plugin.Config, "PlayersTargetOutlineColour"))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Outline colour for player's target & targeters."));
			}
			ImGui.SameLine();
			ImGui.SetNextItemWidth(200f);
			if (ImGuiEx.InputText("##plyTarFormat", Plugin.Config, "PlayersTargetFormat"))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Format of the text for player's target & targeters.\n%%d%% = Target/Targeter Icon\n%%fn%% = Player Forename\n%%sn%% = Player Surname"));
			}
			ImGui.NextColumn();
			ImGui.Columns(1, ImU8String.op_Implicit(""), false);
			ImGui.SetNextItemWidth(200f);
			if (ImGuiEx.DragInt("Indent##plyIndent", Plugin.Config, "PlayersTargetIndent", 1f, 1, 200))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Indent amount for player's target & targeters."));
			}
			ImGui.NextColumn();
			ImGui.Separator();
			ImGui.Spacing();
			ImGui.Text(ImU8String.op_Implicit("Custom Target/Targeter Icons"));
			ImGui.Spacing();
			ImGui.NextColumn();
			ImGui.SetNextItemWidth(30f);
			if (ImGuiEx.InputText("Player's Targeter##cdileftInput", Plugin.Config, "CustomDirLeft", 2))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("The character to display for player's targeter.\nReplaces %%d%% in the above formatting field for 'Player's Target'.\nLeave blank to use default."));
			}
			ImGui.NextColumn();
			ImGui.SetNextItemWidth(30f);
			if (ImGuiEx.InputText("Player's Target##cdirightInput", Plugin.Config, "CustomDirRight", 2))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("The character to display for player's target.\nReplaces %%d%% in the above formatting field for 'Player's Target'.\nLeave blank to use default."));
			}
			ImGui.NextColumn();
			ImGui.SetNextItemWidth(30f);
			if (ImGuiEx.InputText("Mutual Target##cdibothInput", Plugin.Config, "CustomDirBoth", 2))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("The character to display when player's targeter is also their target.\nReplaces %%d%% in the above formatting field for 'Player's Target'.\nLeave blank to use default."));
			}
			ImGui.NextColumn();
			ImGui.Separator();
			ImGui.Text(ImU8String.op_Implicit("Direction Icon"));
			ImGui.Spacing();
			ImGui.NextColumn();
			ImGui.SetNextItemWidth(200f);
			if (ImGuiEx.DragInt("Size Offset##sizeOffset", Plugin.Config, "DirectionIconSizeOffset", 1f, -20, 20))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Adjust the size of direction icons."));
			}
			ImGui.NextColumn();
			ImGui.SetNextItemWidth(200f);
			if (ImGuiEx.DragInt("Left Offset##leftOffset", Plugin.Config, "DirectionIconLeftOffset", 1f, -20, 20))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Padding to add to the left side of direction icons."));
			}
			ImGui.NextColumn();
			ImGui.SetNextItemWidth(200f);
			if (ImGuiEx.DragInt("Right Offset##rightOffset", Plugin.Config, "DirectionIconRightOffset", 1f, -20, 20))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Padding to add to the right side of direction icons."));
			}
			ImGui.NextColumn();
			ImGui.SetNextItemWidth(200f);
			if (ImGuiEx.DragInt("Min Distance##minDistance", Plugin.Config, "DirectionIconMinDistance", 1f, 0, 50))
			{
				Plugin.Config.Save();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("The minimum distance from a player before displaying the direction icon."));
			}
			ImGui.NextColumn();
		}
	}

	private void DrawPlayers()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		if (Plugin.Objects.LocalPlayer == null)
		{
			return;
		}
		Vector4 dalamudViolet = ImGuiColors.DalamudViolet;
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(29, 1);
		((ImU8String)(ref val)).AppendFormatted<int>(PlayerCount);
		((ImU8String)(ref val)).AppendLiteral(" players within range of you.");
		ImGui.TextColored(ref dalamudViolet, val);
		dalamudViolet = ImGuiColors.DalamudViolet;
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(47, 0);
		((ImU8String)(ref val2)).AppendLiteral("Right-click table header or player for options.");
		ImGui.TextColored(ref dalamudViolet, val2);
		List<PlayerEntityInfo> formattedNearbyPlayers = EntityManager.GetFormattedNearbyPlayers(Plugin.Config.ListPlayersMax, Plugin.Config.ListPlayersOrderByDistance, SearchText);
		PlayerCount = Math.Max(0, formattedNearbyPlayers.Count - 1);
		Vector2 vector = new Vector2(0f, 0f);
		if (ImGui.BeginChild(ImU8String.op_Implicit("##playersChild"), vector, false, (ImGuiWindowFlags)24) && ImGui.BeginTable(ImU8String.op_Implicit("##playersTable"), 3, (ImGuiTableFlags)34277248, default(Vector2), 0f))
		{
			ImGui.TableSetupColumn(ImU8String.op_Implicit("##name"), (ImGuiTableColumnFlags)68, 140f, 0u);
			ImGui.TableSetupColumn(ImU8String.op_Implicit("Dist."), (ImGuiTableColumnFlags)4, 50f, 0u);
			ImGui.TableSetupColumn(ImU8String.op_Implicit("Target"), (ImGuiTableColumnFlags)4, ImGui.GetWindowWidth() - 200f, 0u);
			ImGui.TableSetupScrollFreeze(0, 1);
			ImGui.TableNextColumn();
			ImGui.PushItemWidth(ImGui.GetColumnWidth());
			ImGui.InputTextWithHint(ImU8String.op_Implicit("##playerSearch"), ImU8String.op_Implicit("Search.."), ref SearchText, 100, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null);
			ImGui.PopItemWidth();
			ImGui.TableNextColumn();
			ImGui.TableHeader(ImU8String.op_Implicit("Dist."));
			if (ImGui.IsItemClicked((ImGuiMouseButton)1))
			{
				ImU8String val3 = default(ImU8String);
				((ImU8String)(ref val3))._002Ector(25, 0);
				((ImU8String)(ref val3)).AppendLiteral("tableCM##tableContextMenu");
				ImGui.OpenPopup(val3, (ImGuiPopupFlags)0);
			}
			ImGui.TableNextColumn();
			ImGui.TableHeader(ImU8String.op_Implicit("Target"));
			if (ImGui.IsItemClicked((ImGuiMouseButton)1))
			{
				ImU8String val4 = default(ImU8String);
				((ImU8String)(ref val4))._002Ector(25, 0);
				((ImU8String)(ref val4)).AppendLiteral("tableCM##tableContextMenu");
				ImGui.OpenPopup(val4, (ImGuiPopupFlags)0);
			}
			ImU8String val5 = default(ImU8String);
			((ImU8String)(ref val5))._002Ector(25, 0);
			((ImU8String)(ref val5)).AppendLiteral("tableCM##tableContextMenu");
			if (ImGui.BeginPopup(val5, (ImGuiWindowFlags)0))
			{
				if (ImGui.Selectable(ImU8String.op_Implicit(Plugin.Config.ListPlayersOrderByDistance ? "Order by Name" : "Order by Distance"), false, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					Plugin.Config.ListPlayersOrderByDistance = !Plugin.Config.ListPlayersOrderByDistance;
					Plugin.Config.Save();
				}
				Vector2 cursorPos = ImGui.GetCursorPos();
				int num = 140;
				ImGui.SetNextItemWidth((float)num);
				int listPlayersMax = Plugin.Config.ListPlayersMax;
				if (ImGui.SliderInt(ImU8String.op_Implicit("##maxPlayers"), ref listPlayersMax, 1, 200, ImU8String.op_Implicit(""), (ImGuiSliderFlags)0))
				{
					Plugin.Config.ListPlayersMax = listPlayersMax;
					Plugin.Config.Save();
				}
				ImGui.SetCursorPos(new Vector2(cursorPos.X + 2f, cursorPos.Y + 2f));
				ImGui.Text(ImU8String.op_Implicit("Max Players"));
				float num2 = cursorPos.X + (float)(num - 2);
				ImU8String val6 = default(ImU8String);
				((ImU8String)(ref val6))._002Ector(0, 1);
				((ImU8String)(ref val6)).AppendFormatted<int>(Plugin.Config.ListPlayersMax);
				ImGui.SetCursorPos(new Vector2(num2 - ImGui.CalcTextSize(val6, false, -1f).X, cursorPos.Y + 2f));
				ImU8String val7 = default(ImU8String);
				((ImU8String)(ref val7))._002Ector(0, 1);
				((ImU8String)(ref val7)).AppendFormatted<int>(Plugin.Config.ListPlayersMax);
				ImGui.Text(val7);
				ImGui.EndPopup();
			}
			ImGuiClip.ClippedDraw<PlayerEntityInfo>((IReadOnlyList<PlayerEntityInfo>)formattedNearbyPlayers, (Action<PlayerEntityInfo>)delegate(PlayerEntityInfo player)
			{
				//IL_002c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0076: Unknown result type (might be due to invalid IL or missing references)
				//IL_0113: Unknown result type (might be due to invalid IL or missing references)
				//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
				//IL_0190: Unknown result type (might be due to invalid IL or missing references)
				ImGui.TableNextColumn();
				Vector4 nameColour = player.NameColour;
				ImU8String val8 = default(ImU8String);
				((ImU8String)(ref val8))._002Ector(0, 1);
				((ImU8String)(ref val8)).AppendFormatted<string>(player.Name);
				ImGui.TextColored(ref nameColour, val8);
				DrawPlayerContextMenu(player, player.Name);
				ImGui.TableNextColumn();
				Vector2 cursorPos2 = ImGui.GetCursorPos();
				string obj = $"{player.Distance}y";
				Vector2 vector2 = ImGui.CalcTextSize(ImU8String.op_Implicit(obj), false, -1f);
				float num3 = 5f;
				float num4 = (ImGui.GetTextLineHeightWithSpacing() - ImGui.GetTextLineHeight()) * 3f;
				Vector2 center = ImGui.GetCursorScreenPos() + new Vector2(num4 + 1f, vector2.Y / 2f);
				player.DrawDirection(center, num3, 1f, player.NameColour, new Vector4(0f, 0f, 0f, 255f));
				ImGui.SetCursorPos(new Vector2(cursorPos2.X + num4 * 2f + num3, cursorPos2.Y));
				ImGui.Text(ImU8String.op_Implicit(obj));
				ImGui.TableNextColumn();
				IGameObject targetObject = player.GameObject.TargetObject;
				if (targetObject != null && targetObject.IsValid())
				{
					IPlayerCharacter playerTargetC = (IPlayerCharacter)(object)((targetObject is IPlayerCharacter) ? targetObject : null);
					if (playerTargetC != null)
					{
						PlayerEntityInfo playerEntityInfo = EntityManager.NearbyPlayers.Find((PlayerEntityInfo x) => x.Character == playerTargetC);
						if (playerEntityInfo != null)
						{
							nameColour = playerEntityInfo.NameColour;
							ImU8String val9 = default(ImU8String);
							((ImU8String)(ref val9))._002Ector(0, 1);
							((ImU8String)(ref val9)).AppendFormatted<string>(playerEntityInfo.Name);
							ImGui.TextColored(ref nameColour, val9);
							DrawPlayerContextMenu(playerEntityInfo, player.Name + playerEntityInfo.Name);
						}
						return;
					}
				}
				ImGui.Text(ImU8String.op_Implicit(""));
			}, ImGui.GetTextLineHeightWithSpacing());
			ImGui.EndTable();
		}
		ImGui.EndChild();
	}

	private unsafe void DrawPlayerContextMenu(PlayerEntityInfo player, string hash)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(19, 2);
		((ImU8String)(ref val)).AppendFormatted<string>(player.Name);
		((ImU8String)(ref val)).AppendFormatted<string>(hash);
		((ImU8String)(ref val)).AppendLiteral("##playerContextMenu");
		if (!ImGui.BeginPopupContextItem(val, (ImGuiPopupFlags)1))
		{
			return;
		}
		Vector4* styleColorVec = ImGui.GetStyleColorVec4((ImGuiCol)1);
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(1, 2);
		((ImU8String)(ref val2)).AppendFormatted<string>(player.Name);
		((ImU8String)(ref val2)).AppendLiteral("@");
		((ImU8String)(ref val2)).AppendFormatted<string>(player.HomeWorld);
		ImGui.TextColored(ref *styleColorVec, val2);
		Vector4 dalamudGrey = ImGuiColors.DalamudGrey;
		ImU8String val3 = default(ImU8String);
		((ImU8String)(ref val3))._002Ector(2, 1);
		((ImU8String)(ref val3)).AppendLiteral("Lv");
		((ImU8String)(ref val3)).AppendFormatted<byte>(player.Level);
		ImGui.TextColored(ref dalamudGrey, val3);
		ImGui.SameLine();
		JobInfo job = player.Job;
		ref readonly Vector4 jobColour = ref job.JobColour;
		ImU8String val4 = default(ImU8String);
		((ImU8String)(ref val4))._002Ector(0, 1);
		((ImU8String)(ref val4)).AppendFormatted<string>(player.Job.Name);
		ImGui.TextColored(ref jobColour, val4);
		if (player.CompanyTag != "")
		{
			ImGui.SameLine();
			dalamudGrey = ImGuiColors.DalamudGrey;
			ImU8String val5 = default(ImU8String);
			((ImU8String)(ref val5))._002Ector(2, 1);
			((ImU8String)(ref val5)).AppendLiteral("«");
			((ImU8String)(ref val5)).AppendFormatted<string>(player.CompanyTag);
			((ImU8String)(ref val5)).AppendLiteral("»");
			ImGui.TextColored(ref dalamudGrey, val5);
		}
		if (player.IsMareSynced)
		{
			dalamudGrey = ImGuiColors.ParsedPink;
			ImU8String val6 = default(ImU8String);
			((ImU8String)(ref val6))._002Ector(11, 0);
			((ImU8String)(ref val6)).AppendLiteral("Mare Synced");
			ImGui.TextColored(ref dalamudGrey, val6);
		}
		ImGui.Separator();
		ImGui.Dummy(new Vector2(0f, 2f));
		if (IPC.PyonCamEnabled)
		{
			if (ImGui.Selectable(ImU8String.op_Implicit(player.IsCamTarget ? "Reset Camera" : "Camera Orbit"), false, (ImGuiSelectableFlags)0, default(Vector2)))
			{
				player.ToggleCamTarget();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Toggle camera orbit on this player.\nYou can reset camera with the Escape key."));
			}
		}
		if (ImGui.Selectable(ImU8String.op_Implicit("Target"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			player.SetAsTarget();
		}
		if (ImGui.Selectable(ImU8String.op_Implicit("Focus Target"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			player.SetAsFocusTarget();
		}
		if (ImGui.Selectable(ImU8String.op_Implicit("Send Tell"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			player.SendTell();
		}
		if (ImGui.Selectable(ImU8String.op_Implicit("Adventure Plate"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			player.OpenPlate();
		}
		if (ImGui.Selectable(ImU8String.op_Implicit("Examine"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			player.OpenExamine();
		}
		if (ImGui.Selectable(ImU8String.op_Implicit("Locate on Map"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			player.FlagAndOpenMap((MapType)1);
		}
		if (ImGui.Selectable(ImU8String.op_Implicit("Open Lodestone"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			player.SearchPlayerOnLodestone();
		}
		if (ImGui.Selectable(ImU8String.op_Implicit("Open Tomestone"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			player.SearchPlayerOnTomestone();
		}
		if (ImGui.Selectable(ImU8String.op_Implicit(player.IsVisible ? "Hide Character" : "Show Character"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			if (player.IsVisible)
			{
				player.Hide();
			}
			else
			{
				player.Show();
			}
		}
		if (ImGui.Selectable(ImU8String.op_Implicit(player.IsBlocked ? "Remove from Blacklist" : "Add to Blacklist"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			if (player.IsBlocked)
			{
				player.Unblock();
			}
			else
			{
				player.Block();
			}
		}
		ImGui.EndPopup();
	}

	private void DrawObjects()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		if (Plugin.Objects.LocalPlayer == null)
		{
			return;
		}
		Vector4 dalamudViolet = ImGuiColors.DalamudViolet;
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(29, 1);
		((ImU8String)(ref val)).AppendFormatted<int>(ObjectCount);
		((ImU8String)(ref val)).AppendLiteral(" objects within range of you.");
		ImGui.TextColored(ref dalamudViolet, val);
		dalamudViolet = ImGuiColors.DalamudViolet;
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(47, 0);
		((ImU8String)(ref val2)).AppendLiteral("Right-click table header or object for options.");
		ImGui.TextColored(ref dalamudViolet, val2);
		List<IObjectEntityInfo> formattedObjects = EntityManager.GetFormattedObjects(Plugin.Config.ListObjectsMax, Plugin.Config.ListObjectsOrderByDistance, ObjectSearchText);
		ObjectCount = formattedObjects.Count;
		Vector2 vector = new Vector2(0f, 0f);
		if (ImGui.BeginChild(ImU8String.op_Implicit("##objectsChild"), vector, false, (ImGuiWindowFlags)24) && ImGui.BeginTable(ImU8String.op_Implicit("##objectsTable"), 3, (ImGuiTableFlags)34277248, default(Vector2), 0f))
		{
			ImGui.TableSetupColumn(ImU8String.op_Implicit("##name"), (ImGuiTableColumnFlags)68, 140f, 0u);
			ImGui.TableSetupColumn(ImU8String.op_Implicit("Dist."), (ImGuiTableColumnFlags)4, 50f, 0u);
			ImGui.TableSetupColumn(ImU8String.op_Implicit("Type"), (ImGuiTableColumnFlags)4, ImGui.GetWindowWidth() - 200f, 0u);
			ImGui.TableSetupScrollFreeze(0, 1);
			ImGui.TableNextColumn();
			ImGui.PushItemWidth(ImGui.GetColumnWidth());
			ImGui.InputTextWithHint(ImU8String.op_Implicit("##objectSearch"), ImU8String.op_Implicit("Search.."), ref ObjectSearchText, 100, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null);
			ImGui.PopItemWidth();
			ImGui.TableNextColumn();
			ImGui.TableHeader(ImU8String.op_Implicit("Dist."));
			if (ImGui.IsItemClicked((ImGuiMouseButton)1))
			{
				ImU8String val3 = default(ImU8String);
				((ImU8String)(ref val3))._002Ector(31, 0);
				((ImU8String)(ref val3)).AppendLiteral("objtableCM##objtableContextMenu");
				ImGui.OpenPopup(val3, (ImGuiPopupFlags)0);
			}
			ImGui.TableNextColumn();
			ImGui.TableHeader(ImU8String.op_Implicit("Type"));
			if (ImGui.IsItemClicked((ImGuiMouseButton)1))
			{
				ImU8String val4 = default(ImU8String);
				((ImU8String)(ref val4))._002Ector(31, 0);
				((ImU8String)(ref val4)).AppendLiteral("objtableCM##objtableContextMenu");
				ImGui.OpenPopup(val4, (ImGuiPopupFlags)0);
			}
			ImU8String val5 = default(ImU8String);
			((ImU8String)(ref val5))._002Ector(31, 0);
			((ImU8String)(ref val5)).AppendLiteral("objtableCM##objtableContextMenu");
			if (ImGui.BeginPopup(val5, (ImGuiWindowFlags)0))
			{
				if (ImGui.Selectable(ImU8String.op_Implicit(Plugin.Config.ListObjectsOrderByDistance ? "Order by Name" : "Order by Distance"), false, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					Plugin.Config.ListObjectsOrderByDistance = !Plugin.Config.ListObjectsOrderByDistance;
					Plugin.Config.Save();
				}
				Vector2 cursorPos = ImGui.GetCursorPos();
				int num = 140;
				ImGui.SetNextItemWidth((float)num);
				int listObjectsMax = Plugin.Config.ListObjectsMax;
				if (ImGui.SliderInt(ImU8String.op_Implicit("##maxObjects"), ref listObjectsMax, 1, 1000, ImU8String.op_Implicit(""), (ImGuiSliderFlags)0))
				{
					Plugin.Config.ListObjectsMax = listObjectsMax;
					Plugin.Config.Save();
				}
				ImGui.SetCursorPos(new Vector2(cursorPos.X + 2f, cursorPos.Y + 2f));
				ImGui.Text(ImU8String.op_Implicit("Max Objects"));
				float num2 = cursorPos.X + (float)(num - 2);
				ImU8String val6 = default(ImU8String);
				((ImU8String)(ref val6))._002Ector(0, 1);
				((ImU8String)(ref val6)).AppendFormatted<int>(Plugin.Config.ListObjectsMax);
				ImGui.SetCursorPos(new Vector2(num2 - ImGui.CalcTextSize(val6, false, -1f).X, cursorPos.Y + 2f));
				ImU8String val7 = default(ImU8String);
				((ImU8String)(ref val7))._002Ector(0, 1);
				((ImU8String)(ref val7)).AppendFormatted<int>(Plugin.Config.ListObjectsMax);
				ImGui.Text(val7);
				ImGui.Separator();
				ImGuiIOPtr iO = ImGui.GetIO();
				ImGui.SetNextItemWidth(140f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
				ImU8String val8 = ImU8String.op_Implicit("##filterTypes");
				ImU8String val9 = default(ImU8String);
				((ImU8String)(ref val9))._002Ector(12, 0);
				((ImU8String)(ref val9)).AppendLiteral("Filter Types");
				if (ImGui.BeginCombo(val8, val9, (ImGuiComboFlags)0))
				{
					foreach (ObjectTypeFilter value in Enum.GetValues(typeof(ObjectTypeFilter)))
					{
						if (value == ObjectTypeFilter.None)
						{
							continue;
						}
						bool flag = Plugin.Config.ListObjectsTypeFilter.HasFlag(value);
						ImGuiEx.IconCheckbox(flag);
						ImGui.SameLine();
						ImU8String val10 = new ImU8String(0, 1);
						((ImU8String)(ref val10)).AppendFormatted<ObjectTypeFilter>(value);
						if (ImGui.Selectable(val10, flag, (ImGuiSelectableFlags)1, default(Vector2)))
						{
							if (flag)
							{
								Plugin.Config.ListObjectsTypeFilter &= ~value;
							}
							else
							{
								Plugin.Config.ListObjectsTypeFilter |= value;
							}
							Plugin.Config.Save();
						}
					}
					ImGui.EndCombo();
				}
				ImGuiEx.SetItemTooltip("Filter which object types are listed.", (ImGuiHoveredFlags)0);
				ImGui.EndPopup();
			}
			ImGuiClip.ClippedDraw<IObjectEntityInfo>((IReadOnlyList<IObjectEntityInfo>)formattedObjects, (Action<IObjectEntityInfo>)delegate(IObjectEntityInfo obj)
			{
				//IL_0026: Unknown result type (might be due to invalid IL or missing references)
				//IL_0087: Unknown result type (might be due to invalid IL or missing references)
				//IL_0121: Unknown result type (might be due to invalid IL or missing references)
				//IL_0151: Unknown result type (might be due to invalid IL or missing references)
				ImGui.TableNextColumn();
				Vector4 nameColour = obj.NameColour;
				ImU8String val11 = default(ImU8String);
				((ImU8String)(ref val11))._002Ector(0, 1);
				((ImU8String)(ref val11)).AppendFormatted<string>(obj.Name);
				ImGui.TextColored(ref nameColour, val11);
				DrawObjectContextMenu(obj, $"{obj.Id}");
				ImGui.TableNextColumn();
				Vector2 cursorPos2 = ImGui.GetCursorPos();
				string obj2 = $"{obj.Distance}y";
				Vector2 vector2 = ImGui.CalcTextSize(ImU8String.op_Implicit(obj2), false, -1f);
				float num3 = 5f;
				float num4 = (ImGui.GetTextLineHeightWithSpacing() - ImGui.GetTextLineHeight()) * 3f;
				Vector2 center = ImGui.GetCursorScreenPos() + new Vector2(num4 + 1f, vector2.Y / 2f);
				obj.DrawDirection(center, num3, 1f, obj.NameColour, new Vector4(0f, 0f, 0f, 255f));
				ImGui.SetCursorPos(new Vector2(cursorPos2.X + num4 * 2f + num3, cursorPos2.Y));
				ImGui.Text(ImU8String.op_Implicit(obj2));
				ImGui.TableNextColumn();
				nameColour = obj.TypeColour;
				ImU8String val12 = default(ImU8String);
				((ImU8String)(ref val12))._002Ector(0, 1);
				((ImU8String)(ref val12)).AppendFormatted<string>(obj.TypeName);
				ImGui.TextColored(ref nameColour, val12);
			}, ImGui.GetTextLineHeightWithSpacing());
			ImGui.EndTable();
		}
		ImGui.EndChild();
	}

	private unsafe void DrawObjectContextMenu(IObjectEntityInfo obj, string hash)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(19, 2);
		((ImU8String)(ref val)).AppendFormatted<string>(obj.Name);
		((ImU8String)(ref val)).AppendFormatted<string>(hash);
		((ImU8String)(ref val)).AppendLiteral("##objectContextMenu");
		if (!ImGui.BeginPopupContextItem(val, (ImGuiPopupFlags)1))
		{
			return;
		}
		Vector4* styleColorVec = ImGui.GetStyleColorVec4((ImGuiCol)1);
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(0, 1);
		((ImU8String)(ref val2)).AppendFormatted<string>(obj.Name);
		ImGui.TextColored(ref *styleColorVec, val2);
		Vector4 typeColour = obj.TypeColour;
		ImU8String val3 = default(ImU8String);
		((ImU8String)(ref val3))._002Ector(0, 1);
		((ImU8String)(ref val3)).AppendFormatted<string>(obj.TypeName);
		ImGui.TextColored(ref typeColour, val3);
		ImGui.Separator();
		ImGui.Dummy(new Vector2(0f, 2f));
		if (obj is GameObjectEntityInfo gameObjectEntityInfo)
		{
			if (IPC.PyonCamEnabled)
			{
				if (ImGui.Selectable(ImU8String.op_Implicit(gameObjectEntityInfo.IsCamTarget ? "Reset Camera" : "Camera Orbit"), false, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					gameObjectEntityInfo.ToggleCamTarget();
				}
				if (ImGui.IsItemHovered())
				{
					ImGui.SetTooltip(ImU8String.op_Implicit("Toggle camera orbit on this object.\nYou can reset camera with the Escape key."));
				}
			}
			if (gameObjectEntityInfo.IsTargetable)
			{
				if (ImGui.Selectable(ImU8String.op_Implicit("Target"), false, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					gameObjectEntityInfo.SetAsTarget();
				}
				if (ImGui.Selectable(ImU8String.op_Implicit("Focus Target"), false, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					gameObjectEntityInfo.SetAsFocusTarget();
				}
			}
		}
		if (ImGui.Selectable(ImU8String.op_Implicit("Locate on Map"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			obj.FlagAndOpenMap((MapType)1);
		}
		if (obj is GameObjectEntityInfo && ImGui.Selectable(ImU8String.op_Implicit(obj.IsVisible ? "Hide Object" : "Show Object"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			if (obj.IsVisible)
			{
				obj.Hide();
			}
			else
			{
				obj.Show();
			}
		}
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		float x = obj.Position.X;
		float y = obj.Position.Y;
		float z = obj.Position.Z;
		ImGuiIOPtr iO = ImGui.GetIO();
		ImGui.SetNextItemWidth(50f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
		if (ImGui.DragFloat(ImU8String.op_Implicit("##xPos"), ref x, 0.1f, 0f, 0f, default(ImU8String), (ImGuiSliderFlags)0))
		{
			obj.Position = new Vector3(x, y, z);
		}
		ImGuiEx.SetItemTooltip("X Position", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		iO = ImGui.GetIO();
		ImGui.SetNextItemWidth(50f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
		if (ImGui.DragFloat(ImU8String.op_Implicit("##yPos"), ref y, 0.1f, 0f, 0f, default(ImU8String), (ImGuiSliderFlags)0))
		{
			obj.Position = new Vector3(x, y, z);
		}
		ImGuiEx.SetItemTooltip("Y Position", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		iO = ImGui.GetIO();
		ImGui.SetNextItemWidth(50f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
		if (ImGui.DragFloat(ImU8String.op_Implicit("##zPos"), ref z, 0.1f, 0f, 0f, default(ImU8String), (ImGuiSliderFlags)0))
		{
			obj.Position = new Vector3(x, y, z);
		}
		ImGuiEx.SetItemTooltip("Z Position", (ImGuiHoveredFlags)0);
		float rotation = obj.Rotation;
		iO = ImGui.GetIO();
		ImGui.SetNextItemWidth(50f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
		if (ImGui.DragFloat(ImU8String.op_Implicit("##rot"), ref rotation, 0.01f, 0.01f, 0f, default(ImU8String), (ImGuiSliderFlags)0))
		{
			obj.Rotation = rotation;
		}
		ImGuiEx.SetItemTooltip("Rotation", (ImGuiHoveredFlags)0);
		float scale = obj.Scale;
		iO = ImGui.GetIO();
		ImGui.SetNextItemWidth(50f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
		if (ImGui.DragFloat(ImU8String.op_Implicit("##sca"), ref scale, 0.01f, 0.01f, 0f, default(ImU8String), (ImGuiSliderFlags)0))
		{
			obj.Scale = scale;
		}
		ImGuiEx.SetItemTooltip("Scale", (ImGuiHoveredFlags)0);
		ImGui.EndPopup();
	}

	private unsafe void DrawObjectss()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		if (Plugin.Objects.LocalPlayer == null)
		{
			return;
		}
		Vector4 dalamudViolet = ImGuiColors.DalamudViolet;
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(64, 1);
		((ImU8String)(ref val)).AppendFormatted<int>(ObjectCount);
		((ImU8String)(ref val)).AppendLiteral(" objects within range of you. Right-click an object for options.");
		ImGui.TextColored(ref dalamudViolet, val);
		ImU8String val2 = default(ImU8String);
		ImGui.Columns(4, val2, true);
		ImGui.SetColumnWidth(0, 160f + 5f * ImGuiHelpers.GlobalScale);
		ImGui.SetColumnWidth(1, 44f + 5f * ImGuiHelpers.GlobalScale);
		ImGui.SetColumnWidth(2, 44f + 5f * ImGuiHelpers.GlobalScale);
		ImGui.SetColumnWidth(3, 70f + 5f * ImGuiHelpers.GlobalScale);
		ImGui.Text(ImU8String.op_Implicit("Name"));
		ImGui.NextColumn();
		ImGui.Text(ImU8String.op_Implicit("Dist."));
		ImGui.NextColumn();
		ImGui.Text(ImU8String.op_Implicit("Visible"));
		ImGui.NextColumn();
		ImGui.Text(ImU8String.op_Implicit(""));
		ImGui.NextColumn();
		ImGui.Separator();
		List<IGameObject> list = ((IEnumerable<IGameObject>)Plugin.Objects).Where((IGameObject x) => (int)x.ObjectKind != 1).ToList();
		list.Sort((IGameObject x, IGameObject y) => x.Name.TextValue.CompareTo(y.Name.TextValue));
		ObjectCount = list.Count;
		foreach (IGameObject item in list)
		{
			if (!string.IsNullOrEmpty(item.Name.TextValue))
			{
				GameObject* address = (GameObject*)item.Address;
				ImGui.SetNextItemWidth(-1f);
				ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX(), ImGui.GetCursorPosY() + 3f));
				val2 = new ImU8String(0, 1);
				((ImU8String)(ref val2)).AppendFormatted<string>(item.Name.TextValue);
				ImGui.Text(val2);
				ImGui.NextColumn();
				ImGui.SetNextItemWidth(-1f);
				ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX(), ImGui.GetCursorPosY() + 3f));
				float num = Vector3.Distance(((IGameObject)Plugin.Objects.LocalPlayer).Position, item.Position);
				ImU8String val3 = new ImU8String(1, 1);
				((ImU8String)(ref val3)).AppendFormatted<float>(num, "0.0");
				((ImU8String)(ref val3)).AppendLiteral("y");
				ImGui.Text(val3);
				ImGui.NextColumn();
				ImGui.SetNextItemWidth(-1f);
				ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX() + 5f, ImGui.GetCursorPosY()));
				bool flag = false;
				ImGui.Checkbox(ImU8String.op_Implicit("##visibleCheck"), ref flag);
				ImGui.IsItemClicked((ImGuiMouseButton)0);
				if (ImGui.IsItemHovered())
				{
					ImGui.SetTooltip(ImU8String.op_Implicit("Attempt to toggle visibility of this object."));
				}
				ImGui.NextColumn();
				ImGui.SetNextItemWidth(-1f);
				ImU8String val4 = new ImU8String(12, 1);
				((ImU8String)(ref val4)).AppendLiteral("Target##tBtn");
				((ImU8String)(ref val4)).AppendFormatted<ulong>(item.GameObjectId);
				if (ImGui.Button(val4, default(Vector2)))
				{
					Plugin.Targets.Target = item;
				}
				if (ImGui.IsItemHovered())
				{
					ImGui.SetTooltip(ImU8String.op_Implicit("Attempt to target this object."));
				}
				ImGui.NextColumn();
				ImGui.Separator();
			}
		}
	}
}
