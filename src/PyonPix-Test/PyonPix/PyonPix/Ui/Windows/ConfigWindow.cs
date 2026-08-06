using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using PyonPix.Config;
using PyonPix.Config.Global;
using PyonPix.Extensions;
using PyonPix.Services;
using PyonPix.Services.Game;
using PyonPix.Shared.Sync.Dto.Client;
using PyonPix.Structs.Browser;
using PyonPix.Structs.Ui;
using PyonPix.Utility;

namespace PyonPix.Ui.Windows;

public class ConfigWindow : BaseWindow
{
	private readonly List<UiTab> Tabs;

	private UiTab ActiveTab;

	private RendererService RendererService => Services.Get<RendererService>();

	protected override WindowState State
	{
		get
		{
			if (!Config.UI.Config.Collapsed)
			{
				return WindowState.Expanded;
			}
			return WindowState.Collapsed;
		}
	}

	protected override Vector2 ExpandedSize => Config.UI.Config.ExpandedSize;

	protected override Vector2 ExpandedMinSize => new Vector2(350f, 150f);

	protected override Vector2 ExpandedMaxSize => UiUtil.GameResolution;

	protected override bool ShowTitleBarSettingsButton => false;

	public override void OnOpen()
	{
		((Window)this).OnOpen();
		Config.UI.Config.IsOpen = true;
		Config.Save();
	}

	public override void OnClose()
	{
		((Window)this).OnClose();
		Config.UI.Config.IsOpen = false;
		Config.Save();
	}

	protected override void OnCollapsed(Vector2 windowSize)
	{
		Config.UI.Config.ExpandedSize = windowSize;
		Config.Save();
	}

	protected override void SetState(WindowState newState)
	{
		if (State != newState)
		{
			Config.UI.Config.Collapsed = newState == WindowState.Collapsed;
			Config.Save();
		}
	}

	protected override void OnCloseClicked()
	{
		((Window)this).IsOpen = false;
	}

	public ConfigWindow(Configuration config, IServiceContext services, IWindowContext windows)
		: base("PyonPix Config###PyonPixConfig", config, services, windows, (ImGuiWindowFlags)0)
	{
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(350f, 420f) * ImGuiHelpers.GlobalScale;
		int num = 5;
		List<UiTab> list = new List<UiTab>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<UiTab> span = CollectionsMarshal.AsSpan(list);
		span[0] = new UiTab((FontAwesomeIcon)61948, "UI Properties", DrawUiTab);
		span[1] = new UiTab((FontAwesomeIcon)61612, "Shared Browser Properties", DrawBrowserTab);
		span[2] = new UiTab((FontAwesomeIcon)57699, "Shared Renderer Properties", DrawRendererTab);
		span[3] = new UiTab((FontAwesomeIcon)61675, "Shared Lighting Properties", DrawLightTab);
		span[4] = new UiTab((FontAwesomeIcon)61441, "Shared Audio Properties", DrawAudioTab);
		Tabs = list;
		ActiveTab = Tabs[0];
	}

	public override void Draw()
	{
		base.Draw();
	}

	protected override void DrawContent()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (((Window)this).IsOpen)
		{
			DrawTabs();
			ImGui.BeginChild(ImU8String.op_Implicit("##container"), ImGui.GetContentRegionAvail(), false, (ImGuiWindowFlags)0);
			ImGui.SetCursorScreenPos(ImGui.GetCursorScreenPos() + WindowPadding);
			ImGui.BeginChild(ImU8String.op_Implicit("##content"), ImGui.GetContentRegionAvail(), false, (ImGuiWindowFlags)0);
			ActiveTab.Draw();
			ImGui.EndChild();
			ImGui.EndChild();
		}
	}

	private void DrawTabs()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		float lineHeight = LineHeight;
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(cursorScreenPos, cursorScreenPos + new Vector2(lineHeight, ImGui.GetContentRegionAvail().Y), ImGui.GetColorU32(UIShared.TabBg));
		foreach (UiTab tab in Tabs)
		{
			int num = Tabs.IndexOf(tab);
			Vector2 vector = cursorScreenPos + new Vector2(0f, lineHeight * (float)num);
			Vector2 max = vector + new Vector2(lineHeight, lineHeight);
			if (DrawTab(vector, max, lineHeight, tab, ActiveTab == tab))
			{
				ActiveTab = tab;
			}
		}
		ImGui.SetCursorScreenPos(cursorScreenPos + new Vector2(lineHeight, 0f));
	}

	private bool DrawTab(Vector2 min, Vector2 max, float iconSize, UiTab tab, bool active)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		bool flag = UiUtil.IsRectHovered(min, max);
		bool flag2 = UiUtil.IsRectClicked(min, max, (ImGuiMouseButton)0);
		Vector4 vector = (active ? UIShared.TabBgActive : (flag2 ? UIShared.TabBgClicked : (flag ? UIShared.TabBgHovered : UIShared.TabBgNormal)));
		((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(min, max, ImGui.GetColorU32(vector), UIShared.TabRounding);
		ImGui.SetCursorScreenPos(UiUtil.AlignCenter(min, max, iconSize));
		if (flag2 || ImGuiEx.IconToggleButton(tab.Icon, $"##tab{tab.Icon}", active, disabled: false, tab.Tooltip, null, iconSize))
		{
			return true;
		}
		return false;
	}

	private void DrawUiTab()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		GlobalProperties global = Config.Global;
		ImGui.GetContentRegionAvail();
		UIState uIState = UIState.None;
		ImGuiEx.StyledText(ImU8String.op_Implicit("UI Accent Colours"), null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, null, null, null, null, null, null, null, null, float.MaxValue);
		uIState |= ImGuiEx.ColorPicker4("Window Bg##accentBg", ref global.General.AccentBg);
		ImGui.SameLine();
		uIState |= ImGuiEx.ColorPicker4("Window Title##accentTitle", ref global.General.AccentTitle);
		uIState |= ImGuiEx.ColorPicker4("Item Hovered##accentHovered", ref global.General.AccentHovered);
		ImGui.SameLine();
		uIState |= ImGuiEx.ColorPicker4("Item Active##accentActive", ref global.General.AccentActive);
		if (uIState != UIState.None)
		{
			UIShared.Update();
		}
		if (false || uIState == UIState.Ended)
		{
			Config.Save();
		}
	}

	private void DrawBrowserTab()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
		GlobalProperties global = Config.Global;
		float num = ImGui.GetContentRegionAvail().X - WindowPadding.X;
		UIState uIState = UIState.None;
		ImGuiEx.StyledText(ImU8String.op_Implicit("Shared Browser Properties"), null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, null, null, null, null, null, null, null, null, float.MaxValue);
		float num2 = 70f * ImGuiHelpers.GlobalScale;
		int num3 = 0 | (ImGuiEx.EnumCombo("##homeType", string.Empty, ref global.Browser.HomeUriType, ComboButtonDisplayType.Items, disabled: false, "Home Uri Type", "- Blank: Display a blank homepage (pix:// or about:blank)\n- Starry: Display a starry homepage (pix://starry)\n- Custom: Display a custom homepage", 6, num2) ? 1 : 0);
		ImGui.SameLine(0f, ItemSpacing);
		uIState |= ImGuiEx.StyledInput(ImU8String.op_Implicit("##home"), ref global.Browser.HomeUri, "Custom Home Uri", global.Browser.HomeUriType != HomeUriType.Custom, 65535, num - num2 - ItemSpacing, (ImGuiInputTextFlags)16, "Custom Home Uri", "Homepage to display when creating a new Pix or when clicking the browser Home button.\n- Eg. https://google.com", null, (FontAwesomeIcon)0, null, (FontAwesomeIcon)0);
		ImGuiEx.SpacingY(ItemSpacing);
		float width = (num - ItemSpacing) * 0.5f;
		int num4 = num3 | (ImGuiEx.EnumFlagsCombo("##spawnBehaviour", "Spawn Behaviour", ref global.Browser.TerritorySpawnBehaviour, ComboButtonDisplayType.Label, disabled: false, "Territory Spawn Behaviour", "The spawn behaviour of a Pix browser environment when changing territory.", 6, width) ? 1 : 0);
		ImGui.SameLine(0f, ItemSpacing);
		int num5 = num4 | (ImGuiEx.EnumFlagsCombo("##despawnBehaviour", "Despawn Behaviour", ref global.Browser.TerritoryDespawnBehaviour, ComboButtonDisplayType.Label, disabled: false, "Territory Despawn Behaviour", "The despawn behaviour of a Pix browser environment when changing territory.", 6, width) ? 1 : 0);
		ImGuiEx.Separator(num - UIShared.SeparatorSpacing, UIShared.SeparatorSpacing);
		ImGuiEx.StyledText(ImU8String.op_Implicit("Screen Interaction Conditions"), null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, null, null, null, null, null, null, null, null, float.MaxValue, multiline: false, "Screen Interaction Conditions", "These are global properties which affect how a screen can receive interactions.\nYou can individually toggle screen interactions per pix from its pix config window in Browser Properties.");
		ImGuiEx.SpacingY(UIShared.SeparatorSpacing);
		ImGuiEx.StyledText(ImU8String.op_Implicit("Capture:"), null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, null, null, null, null, null, null, null, null, float.MaxValue);
		ImGui.SameLine();
		if (ImGuiEx.Checkbox("LButton", ref global.Browser.ScreenInteractionCaptureLButton, disabled: false, "Capture LButton", "Whether any screen can receive left mouse button click."))
		{
			Config.Save();
		}
		ImGui.SameLine();
		if (ImGuiEx.Checkbox("RButton", ref global.Browser.ScreenInteractionCaptureRButton, disabled: false, "Capture RButton", "Whether any screen can receive right mouse button click."))
		{
			Config.Save();
		}
		ImGui.SameLine();
		if (ImGuiEx.Checkbox("MButton", ref global.Browser.ScreenInteractionCaptureMButton, disabled: false, "Capture MButton", "Whether any screen can receive middle mouse button click."))
		{
			Config.Save();
		}
		ImGui.SameLine();
		if (ImGuiEx.Checkbox("Scroll", ref global.Browser.ScreenInteractionCaptureScroll, disabled: false, "Capture Scroll Wheel", "Whether any screen can receive mouse scroll events."))
		{
			Config.Save();
		}
		ImGuiEx.SpacingY(ItemSpacing);
		ImGuiEx.StyledText(ImU8String.op_Implicit("Require Modifier:"), null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, null, null, null, null, null, null, null, null, float.MaxValue);
		ImGui.SameLine();
		if (ImGuiEx.Checkbox("Ctrl", ref global.Browser.ScreenInteractionReqCtrl, disabled: false, "Require Ctrl", "A screen will only begin receiving input if clicked while Ctrl is held."))
		{
			Config.Save();
		}
		ImGui.SameLine();
		if (ImGuiEx.Checkbox("Shift", ref global.Browser.ScreenInteractionReqShift, disabled: false, "Require Shift", "A screen will only begin receiving input if clicked while Shift is held."))
		{
			Config.Save();
		}
		ImGuiEx.SpacingY(ItemSpacing);
		if (ImGuiEx.Checkbox("Front Face Only", ref global.Browser.ScreenInteractionFrontFace, disabled: false, "Front Face Only", "Whether mouse events will only trigger interaction when clicking the front face of the screen."))
		{
			Config.Save();
		}
		ImGui.SameLine();
		if (ImGuiEx.Checkbox("Perform Cursor Changes", ref global.Browser.ScreenInteractionCursorChanges, disabled: false, "Perform Cursor Changes", "Whether the mouse cursor will change when hovering over elements on a screen.\n\nThis is currently experimental, there may be issues."))
		{
			Config.Save();
		}
		if (num5 != 0 || uIState == UIState.Ended)
		{
			Config.Save();
		}
	}

	private void DrawRendererTab()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		GlobalProperties global = Config.Global;
		float num = ImGui.GetContentRegionAvail().X - WindowPadding.X;
		UIState uIState = UIState.None;
		ImGuiEx.StyledText(ImU8String.op_Implicit("Shared Renderer Properties"), null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, null, null, null, null, null, null, null, null, float.MaxValue);
		UIState num2 = uIState;
		ref int pixSpawnLimit = ref global.General.PixSpawnLimit;
		float width = num;
		uIState = num2 | ImGuiEx.Drag("Pix Spawn Limit##pixLimit", ref pixSpawnLimit, 0.2f, 1, 99, 2, default(ImU8String), disabled: false, width, null, 0.1f, insetLabel: true, "Pix Spawn Limit", "When this limit is exceeded, the earliest activated Pix will be despawned.\nHigh spawn limit may incur high system resource usage depending on media content.");
		if (false || uIState == UIState.Ended)
		{
			Config.Save();
		}
	}

	private void DrawLightTab()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		GlobalProperties global = Config.Global;
		float num = ImGui.GetContentRegionAvail().X - WindowPadding.X;
		UIState uIState = UIState.None;
		ImGuiEx.StyledText(ImU8String.op_Implicit("Shared Light Properties"), null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, null, null, null, null, null, null, null, null, float.MaxValue);
		UIState num2 = uIState;
		ref float influenceSmoothing = ref global.Light.InfluenceSmoothing;
		float width = num;
		uIState = num2 | ImGuiEx.Drag("Influence Smoothing##influenceSmoothing", ref influenceSmoothing, 0.001f, 0f, 1f, 2, default(ImU8String), disabled: false, width, null, 0.1f, insetLabel: true, "Screen Colour Influence Smoothing", "Adjust how smooth the transition of colour changes are for rendered screens with light that is influenced by screen colour.\nHigher smoothing will reduce light flickering when rendered frames rapidly change colours.");
		UIState num3 = uIState;
		ref float influenceSmoothingDuration = ref global.Light.InfluenceSmoothingDuration;
		width = num;
		uIState = num3 | ImGuiEx.Drag("Smoothing Duration##smoothingDuration", ref influenceSmoothingDuration, 0.004f, 0f, 2f, 2, default(ImU8String), disabled: false, width, null, 0.1f, insetLabel: true, "Influence Smoothing Duration", "The time taken (in seconds) for a smoothing transition to complete.");
		if (false || uIState == UIState.Ended)
		{
			Config.Save();
		}
	}

	private void DrawAudioTab()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		GlobalProperties global = Config.Global;
		float num = ImGui.GetContentRegionAvail().X - WindowPadding.X;
		UIState uIState = UIState.None;
		ImGuiEx.StyledText(ImU8String.op_Implicit("Shared Audio Properties"), null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, null, null, null, null, null, null, null, null, float.MaxValue);
		float num2 = 90f * ImGuiHelpers.GlobalScale;
		int num3 = 0 | (ImGuiEx.EnumCombo("##listenerType", string.Empty, ref global.Audio.ListenerType, ComboButtonDisplayType.Items, disabled: false, "Spatial Audio Listener Type", "- Character: Spatial audio relative to character position & facing direction from screen.\n- Camera: Spatial audio relative to camera position & rotation from screen.", 6, num2) ? 1 : 0);
		ImGui.SameLine(0f, ItemSpacing);
		UIState num4 = uIState;
		ref float masterVolume = ref global.Audio.MasterVolume;
		float width = num - num2 - ItemSpacing;
		uIState = num4 | ImGuiEx.Drag("Master Volume##masterVolume", ref masterVolume, 0.001f, 0f, 1f, 2, default(ImU8String), disabled: false, width, null, 0.1f, insetLabel: true, "Master Volume", "Per Pix audio volume will be in relation to this master volume.");
		if (num3 != 0 || uIState == UIState.Ended)
		{
			Config.Save();
		}
	}
}
