using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImGuizmo;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using PyonPix.Config;
using PyonPix.Config.Pix;
using PyonPix.Events;
using PyonPix.Extensions;
using PyonPix.Services;
using PyonPix.Services.Core;
using PyonPix.Services.Game;
using PyonPix.Shared.Structs.Browser;
using PyonPix.Shared.Structs.Light;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Structs.Pix.Properties;
using PyonPix.Shared.Structs.Renderer;
using PyonPix.Shared.Structs.Territory;
using PyonPix.Shared.Sync.Dto.Client;
using PyonPix.Shared.Utility;
using PyonPix.Structs.PlayerState;
using PyonPix.Structs.Ui;
using PyonPix.Ui.Components;
using PyonPix.Utility;

namespace PyonPix.Ui.Windows;

public class PixConfigWindow : BaseWindow
{
	private readonly List<UiTab> Tabs;

	private UiTab ActiveTab;

	public IPix? SelectedPix;

	private ContextMenu? SyncOverrideContextMenu;

	private readonly TransformEditor TransformEditor;

	private ContextMenu? WorldContextMenu;

	private bool ResidentialOnly = true;

	private PixService PixService => Services.Get<PixService>();

	private SyncService SyncService => Services.Get<SyncService>();

	private StateService StateService => Services.Get<StateService>();

	private BrowserService BrowserService => Services.Get<BrowserService>();

	protected override WindowState State
	{
		get
		{
			if (!Config.UI.PixConfig.Collapsed)
			{
				return WindowState.Expanded;
			}
			return WindowState.Collapsed;
		}
	}

	protected override Vector2 ExpandedSize => Config.UI.PixConfig.ExpandedSize;

	protected override Vector2 ExpandedMinSize => new Vector2(420f, 190f);

	protected override Vector2 ExpandedMaxSize => UiUtil.GameResolution;

	private float Spacing => 6f * ImGuiHelpers.GlobalScale;

	public override void OnClose()
	{
		((Window)this).OnClose();
		TransformEditor.HideGizmo();
	}

	protected override void OnCollapsed(Vector2 windowSize)
	{
		Config.UI.PixConfig.ExpandedSize = windowSize;
		Config.Save();
	}

	protected override void SetState(WindowState newState)
	{
		if (State != newState)
		{
			Config.UI.PixConfig.Collapsed = newState == WindowState.Collapsed;
			Config.Save();
		}
	}

	protected override void OnConfigClicked()
	{
		((Window)Windows.Get<ConfigWindow>()).Toggle();
	}

	protected override void OnCloseClicked()
	{
		SelectedPix = null;
		((Window)this).IsOpen = false;
	}

	public PixConfigWindow(Configuration config, IServiceContext services, IWindowContext windows)
		: base("Pix Config###PyonPixPixConfig", config, services, windows, (ImGuiWindowFlags)0)
	{
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(420f, 420f) * ImGuiHelpers.GlobalScale;
		((Window)this).PositionCondition = (ImGuiCond)4;
		((Window)this).Position = UiUtil.CenterWindow(((Window)this).Size.Value);
		int num = 6;
		List<UiTab> list = new List<UiTab>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<UiTab> span = CollectionsMarshal.AsSpan(list);
		span[0] = new UiTab((FontAwesomeIcon)61737, "Info Properties", DrawInfoTab);
		span[1] = new UiTab((FontAwesomeIcon)61612, "Browser Properties", DrawBrowserTab);
		span[2] = new UiTab((FontAwesomeIcon)57699, "Renderer Properties", DrawRendererTab);
		span[3] = new UiTab((FontAwesomeIcon)61675, "Light Properties", DrawLightTab);
		span[4] = new UiTab((FontAwesomeIcon)61441, "Audio Properties", DrawAudioTab);
		span[5] = new UiTab((FontAwesomeIcon)61473, "Sync Properties", DrawSyncTab);
		Tabs = list;
		ActiveTab = Tabs[0];
		TransformEditor = new TransformEditor();
		SyncService.SyncedPixCreated += delegate(LocalPix local, SyncedPix synced)
		{
			if (!(SelectedPix?.Id != local.Id))
			{
				SetSelectedPix(synced);
			}
		};
		SyncService.SyncedPixDeleted += delegate(string syncedPixId, LocalPix? local)
		{
			if (IsSelectedPixId(syncedPixId))
			{
				if (local != null)
				{
					SetSelectedPix(local);
				}
				else
				{
					SetSelectedPix(null);
				}
			}
		};
		SyncService.SyncedPixUnsubscribed += delegate(string pixId)
		{
			if (IsSelectedPixId(pixId))
			{
				Toggle(null);
			}
		};
		SyncService.StateChanged += delegate(ConnectionState connectionState, string? statusMessage, StatusType statusType)
		{
			if (SelectedPix is SyncedPix && connectionState == ConnectionState.Disconnected)
			{
				Toggle(null);
			}
		};
	}

	public bool IsSelectedPixId(string? pixId)
	{
		if (string.IsNullOrWhiteSpace(pixId))
		{
			return false;
		}
		return string.Equals(SelectedPix?.Id, pixId, StringComparison.OrdinalIgnoreCase);
	}

	public void SetSelectedPix(IPix? pix)
	{
		if (pix == null)
		{
			SelectedPix = null;
			((Window)this).IsOpen = false;
		}
		else
		{
			((Window)this).WindowName = pix.Id + " Config###PyonPixPixConfig";
			SelectedPix = pix;
			((Window)this).IsOpen = true;
		}
	}

	public void Toggle(IPix? pix)
	{
		if (pix == null || pix == SelectedPix)
		{
			SelectedPix = null;
			((Window)this).IsOpen = false;
		}
		else
		{
			((Window)this).WindowName = pix.Id + " Config###PyonPixPixConfig";
			SelectedPix = pix;
			((Window)this).IsOpen = true;
		}
	}

	public override void Draw()
	{
		base.Draw();
	}

	protected override void DrawContent()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (SelectedPix == null)
		{
			((Window)this).IsOpen = false;
		}
		if (((Window)this).IsOpen)
		{
			DrawTabs();
			ImGui.BeginChild(ImU8String.op_Implicit("##pixContainer"), ImGui.GetContentRegionAvail(), false, (ImGuiWindowFlags)0);
			ImGui.SetCursorScreenPos(ImGui.GetCursorScreenPos() + new Vector2(Spacing, Spacing));
			ImGui.BeginChild(ImU8String.op_Implicit("##pixContent"), ImGui.GetContentRegionAvail() - new Vector2(0f, Spacing), false, (ImGuiWindowFlags)0);
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
				TransformEditor.HideGizmo();
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

	private void DrawInfoTab()
	{
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07be: Unknown result type (might be due to invalid IL or missing references)
		//IL_07de: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e03: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e09: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f04: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f0a: Unknown result type (might be due to invalid IL or missing references)
		//IL_104c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1052: Unknown result type (might be due to invalid IL or missing references)
		//IL_115f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1165: Unknown result type (might be due to invalid IL or missing references)
		//IL_1211: Unknown result type (might be due to invalid IL or missing references)
		//IL_1217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e5e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e64: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f5f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f65: Unknown result type (might be due to invalid IL or missing references)
		//IL_10a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_10ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_11ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_11c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1105: Unknown result type (might be due to invalid IL or missing references)
		//IL_110b: Unknown result type (might be due to invalid IL or missing references)
		OwnerFieldBinding<InfoPixProperties> ownerFieldBinding = PixService.BindOwnerField(SelectedPix, (BasePix p) => p.Info, delegate(BasePix p, InfoPixProperties v)
		{
			p.Info = v;
		});
		InfoPixProperties value = ownerFieldBinding.Value;
		bool isSynced = SelectedPix.Sync.IsSynced;
		bool flag = ownerFieldBinding.CanEdit && (!isSynced || SyncService.IsConnectedAuth);
		UIState uIState = UIState.None;
		Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
		float globalScale = ImGuiHelpers.GlobalScale;
		float num = 70f * globalScale;
		int num2 = 0 | (ImGuiEx.EnumCombo("##pixType", string.Empty, ref value.Type, ComboButtonDisplayType.Items, !flag, "Pix Category", "Select a category which best relates to the primary use for this Pix.\n- Video: Watching videos/livestreams.\n- Audio: Listening to music or background ambience.\n- Image: Displaying static/animated images.\n- Game: Playing/spectating games.\n- Light: Rendering a source of light.\n- Other: Any use in general.", 6, num) ? 1 : 0);
		ImGui.SameLine(0f, Spacing);
		uIState |= ImGuiEx.StyledInput(ImU8String.op_Implicit("##name"), ref value.Name, "Name", !flag, 32, contentRegionAvail.X - num - Spacing * 2f, (ImGuiInputTextFlags)16, "Pix Name", "A name to identify this Pix in place of its Id.", null, (FontAwesomeIcon)0, null, (FontAwesomeIcon)0);
		uIState |= ImGuiEx.StyledInput(ImU8String.op_Implicit("##desc"), ref value.Description, "Description", !flag, 192, contentRegionAvail.X - Spacing, (ImGuiInputTextFlags)16, "Pix Description", "Optional description detailing the usage of this Pix.", null, (FontAwesomeIcon)0, null, (FontAwesomeIcon)0);
		string error = string.Empty;
		bool flag2 = !isSynced || NameUtil.ValidatePix(value.Name, value.Description, SelectedPix.Sync.SecretKey, SelectedPix.Sync.Privacy, SyncService.Client.Premium, out error);
		if ((num2 != 0 || uIState == UIState.Ended) && flag2 && flag)
		{
			ownerFieldBinding.Commit(value);
			PixService.UpdateInfoProperties(SelectedPix);
		}
		else if (!flag2 && flag)
		{
			using (UIShared.SubFont.Push())
			{
				ImU8String text = ImU8String.op_Implicit(error);
				Vector3? colorA = new Vector3(0.6f, 0f, 0f);
				Vector3? colorB = new Vector3(1f, 0f, 0f);
				ImGuiEx.StyledText(text, null, 0.8f, 0.4f, 4f, 0.1f, AnimationType.Pulse, colorA, colorB, null, null, null, null, null, null, float.MaxValue);
			}
		}
		ImGuiEx.Separator(contentRegionAvail.X - Spacing);
		TerritoryPixProperties tProps = SelectedPix.Territory;
		TerritoryData currentTerritory = StateService.CurrentTerritory;
		bool flag3 = tProps.Matches(currentTerritory, tProps.Persistent);
		if (SelectedPix.Sync.IsSynced)
		{
			using (UIShared.SubFont.Push())
			{
				Vector3 value2 = (flag3 ? UIShared.AccentActive.AsVector3() : UIShared.AccentHovered.AsVector3());
				float num3 = 0.1f;
				float num4 = (flag3 ? 0.4f : 0.2f);
				string worldName = StateService.GetWorldName(tProps.WorldId);
				if (!string.IsNullOrEmpty(worldName))
				{
					ImGui.SameLine(0f, 0f);
					ImU8String val = new ImU8String(0, 1);
					((ImU8String)(ref val)).AppendFormatted<string>(worldName);
					ImU8String text2 = val;
					Vector3? colorB = value2;
					float glowStrength = num3;
					float bgOpacity = num4;
					ImGuiEx.StyledText(text2, null, 0.8f, bgOpacity, 4f, glowStrength, AnimationType.Static, colorB, null, null, null, null, null, null, null, float.MaxValue);
				}
				string territoryName = StateService.GetTerritoryName(tProps.TerritoryId);
				if (!string.IsNullOrEmpty(territoryName))
				{
					ImGui.SameLine(0f, 0f);
					ImU8String val2 = new ImU8String(0, 1);
					((ImU8String)(ref val2)).AppendFormatted<string>(territoryName);
					ImU8String text3 = val2;
					Vector3? colorB = value2;
					float bgOpacity = num3;
					float glowStrength = num4;
					ImGuiEx.StyledText(text3, null, 0.8f, glowStrength, 4f, bgOpacity, AnimationType.Static, colorB, null, null, null, null, null, null, null, float.MaxValue);
				}
				if (tProps.Ward > 0)
				{
					ImGui.SameLine(0f, 0f);
					ImU8String val3 = new ImU8String(1, 1);
					((ImU8String)(ref val3)).AppendLiteral("W");
					((ImU8String)(ref val3)).AppendFormatted<short>(tProps.Ward);
					ImU8String text4 = val3;
					Vector3? colorB = value2;
					float glowStrength = num3;
					float bgOpacity = num4;
					ImGuiEx.StyledText(text4, null, 0.8f, bgOpacity, 4f, glowStrength, AnimationType.Static, colorB, null, null, null, null, null, null, null, float.MaxValue);
				}
				if (tProps.Plot > 0)
				{
					ImGui.SameLine(0f, 0f);
					ImU8String val4 = new ImU8String(1, 1);
					((ImU8String)(ref val4)).AppendLiteral("P");
					((ImU8String)(ref val4)).AppendFormatted<short>(tProps.Plot);
					ImU8String text5 = val4;
					Vector3? colorB = value2;
					float bgOpacity = num3;
					float glowStrength = num4;
					ImGuiEx.StyledText(text5, null, 0.8f, glowStrength, 4f, bgOpacity, AnimationType.Static, colorB, null, null, null, null, null, null, null, float.MaxValue);
				}
				if (tProps.Room > 0)
				{
					ImGui.SameLine(0f, 0f);
					ImU8String val5 = new ImU8String(1, 1);
					((ImU8String)(ref val5)).AppendLiteral("R");
					((ImU8String)(ref val5)).AppendFormatted<short>(tProps.Room);
					ImU8String text6 = val5;
					Vector3? colorB = value2;
					float glowStrength = num3;
					float bgOpacity = num4;
					ImGuiEx.StyledText(text6, null, 0.8f, bgOpacity, 4f, glowStrength, AnimationType.Static, colorB, null, null, null, null, null, null, null, float.MaxValue);
				}
				if (tProps.Floor != Floor.None)
				{
					ImGui.SameLine(0f, 0f);
					ImU8String val6 = new ImU8String(0, 1);
					((ImU8String)(ref val6)).AppendFormatted<Floor>(tProps.Floor);
					ImU8String text7 = val6;
					Vector3? colorB = value2;
					float bgOpacity = num3;
					float glowStrength = num4;
					ImGuiEx.StyledText(text7, null, 0.8f, glowStrength, 4f, bgOpacity, AnimationType.Static, colorB, null, null, null, null, null, null, null, float.MaxValue);
				}
				return;
			}
		}
		if (ImGuiEx.IconButton((FontAwesomeIcon)62405, "##setTerritory", currentTerritory == null, "Set to Current Territory", null, LineHeight))
		{
			tProps.WorldId = currentTerritory.WorldId;
			tProps.TerritoryId = currentTerritory.TerritoryId;
			tProps.Ward = currentTerritory.Ward;
			tProps.Plot = currentTerritory.Plot;
			tProps.Room = currentTerritory.Room;
			tProps.Floor = currentTerritory.Floor;
			PixService.UpdateTerritory(SelectedPix);
			SelectedPix.Renderer.Position = new Vector3(StateService.LocalPlayerPosition.X, StateService.LocalPlayerPosition.Y + 1f, StateService.LocalPlayerPosition.Z);
			PixService.UpdateRendererTransform(SelectedPix, editFinished: true);
		}
		ImGui.SameLine(0f, Spacing);
		float num5 = 100f * ImGuiHelpers.GlobalScale;
		bool flag4 = WorldContextMenu?.IsOpen() ?? false;
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		if (ImGuiEx.IconTextButton((FontAwesomeIcon)(flag4 ? 61655 : 61658), StateService.GetWorldName(tProps.WorldId), "##world", disabled: false, null, null, num5, LineHeight))
		{
			List<ContextMenuTab> list = new List<ContextMenuTab>();
			Region[] values = Enum.GetValues<Region>();
			foreach (Region region in values)
			{
				List<WorldInfo> list2 = StateService.Worlds[region];
				List<ContextMenuItem> list3 = new List<ContextMenuItem>();
				foreach (WorldInfo world in list2)
				{
					list3.Add(new ContextMenuButton(world.Name, delegate
					{
						if (tProps.WorldId != world.Id)
						{
							tProps.WorldId = world.Id;
							PixService.UpdateTerritory(SelectedPix);
						}
					}, closeOnClick: true, null, () => tProps.WorldId == world.Id));
				}
				list.Add(new ContextMenuTab($"{region}", $"{region}", list3));
			}
			int activeTabIndex = (int)(StateService.GetRegionFromWorld(tProps.WorldId) - 1);
			WorldContextMenu = new ContextMenu("##worldContext", list, activeTabIndex, 240f, 26f);
			WorldContextMenu.Open();
		}
		if (flag4)
		{
			WorldContextMenu?.Draw(new Vector2(cursorScreenPos.X, cursorScreenPos.Y + LineHeight));
		}
		ImGui.SameLine(0f, Spacing);
		if (ImGuiEx.ListCombo("##territory", string.Empty, "Selected Territory", ref tProps.TerritoryId, StateService.UITerritoryList.Select<(uint, string, bool), (uint, string)>(((uint Id, string Name, bool IsResidential) t) => (Id: t.Id, $"{t.Name} ({t.Id})")), ComboButtonDisplayType.Items, disabled: false, null, null, 6, contentRegionAvail.X - num5 - LineHeight - Spacing * 3f, null, delegate
		{
			Vector2 vector = UiUtil.CalcTextSize(UIShared.NormalIconFont, FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61770));
			float y = (LineHeight - vector.Y) * 0.5f;
			ImGui.SetCursorScreenPos(ImGui.GetCursorScreenPos() + new Vector2(Spacing, y));
			if (ImGuiEx.Checkbox("Residential Only##terrFilter", ref ResidentialOnly))
			{
				StateService.BuildUITerritoryList(ResidentialOnly);
			}
		}))
		{
			tProps.Ward = (tProps.Plot = (tProps.Room = 0));
			PixService.UpdateTerritory(SelectedPix);
		}
		ResidentialTerritory residentialTerritory = StateService.ResidentialTerritories.FirstOrDefault((ResidentialTerritory x) => x.Id == tProps.TerritoryId);
		if (residentialTerritory == null || residentialTerritory.ResidentialType == ResidentialType.Workshop)
		{
			return;
		}
		float num7 = 80f * ImGuiHelpers.GlobalScale;
		switch (residentialTerritory.ResidentialType)
		{
		case ResidentialType.Ward:
		{
			ref short ward4 = ref tProps.Ward;
			float glowStrength = num7;
			if (ImGuiEx.Drag<short>("Ward##ward", ref ward4, 0.1f, 1, 30, 2, default(ImU8String), disabled: false, glowStrength) == UIState.Ended)
			{
				PixService.UpdateTerritory(SelectedPix);
			}
			ImGui.SameLine();
			ref short plot2 = ref tProps.Plot;
			glowStrength = num7;
			if (ImGuiEx.Drag<short>("Plot##plot", ref plot2, 0.1f, 0, 60, 2, default(ImU8String), disabled: false, glowStrength, null, 0.1f, insetLabel: true, "Plot (Garden)", "Set to '0' to have this Pix active within a Ward outside of garden Plots.\nOtherwise limit to a specific Plot's garden.") == UIState.Ended)
			{
				PixService.UpdateTerritory(SelectedPix);
			}
			ImGui.SameLine();
			if (ImGuiEx.Checkbox("Persistent Plot##persist", ref tProps.Persistent, disabled: false, "Persistent Plot", "If true, this Pix will persist across garden Plots within the Ward.", LineHeight))
			{
				PixService.UpdateTerritory(SelectedPix);
			}
			break;
		}
		case ResidentialType.House:
		{
			ref short ward2 = ref tProps.Ward;
			float glowStrength = num7;
			if (ImGuiEx.Drag<short>("Ward##ward", ref ward2, 0.1f, 1, 30, 2, default(ImU8String), disabled: false, glowStrength) == UIState.Ended)
			{
				PixService.UpdateTerritory(SelectedPix);
			}
			ImGui.SameLine();
			ref short plot = ref tProps.Plot;
			glowStrength = num7;
			if (ImGuiEx.Drag<short>("Plot##plot", ref plot, 0.1f, 1, 60, 2, default(ImU8String), disabled: false, glowStrength) == UIState.Ended)
			{
				PixService.UpdateTerritory(SelectedPix);
			}
			ImGui.SameLine();
			string empty = string.Empty;
			ref Floor floor = ref tProps.Floor;
			Floor? ignoredValue = Floor.None;
			if (ImGuiEx.EnumCombo("##floor", empty, ref floor, ComboButtonDisplayType.Items, disabled: false, null, null, 6, num7, null, null, ignoredValue))
			{
				PixService.UpdateTerritory(SelectedPix);
			}
			ImGui.SameLine();
			if (ImGuiEx.Checkbox("Persistent Floor##persist", ref tProps.Persistent, disabled: false, "Persistent Floor", "If true, this Pix will persist across Floors within the Plot.", LineHeight))
			{
				PixService.UpdateTerritory(SelectedPix);
			}
			break;
		}
		case ResidentialType.Chambers:
		{
			ref short ward5 = ref tProps.Ward;
			float glowStrength = num7;
			if (ImGuiEx.Drag<short>("Ward##ward", ref ward5, 0.1f, 1, 30, 2, default(ImU8String), disabled: false, glowStrength) == UIState.Ended)
			{
				PixService.UpdateTerritory(SelectedPix);
			}
			ImGui.SameLine();
			ref short plot3 = ref tProps.Plot;
			glowStrength = num7;
			if (ImGuiEx.Drag<short>("Plot##plot", ref plot3, 0.1f, 1, 60, 2, default(ImU8String), disabled: false, glowStrength) == UIState.Ended)
			{
				PixService.UpdateTerritory(SelectedPix);
			}
			ImGui.SameLine();
			ref short room2 = ref tProps.Room;
			glowStrength = num7;
			if (ImGuiEx.Drag<short>("Room##room", ref room2, 0.1f, 1, 512, 2, default(ImU8String), disabled: false, glowStrength) == UIState.Ended)
			{
				PixService.UpdateTerritory(SelectedPix);
			}
			break;
		}
		case ResidentialType.Apartment:
		{
			ref short ward3 = ref tProps.Ward;
			float glowStrength = num7;
			if (ImGuiEx.Drag<short>("Ward##ward", ref ward3, 0.1f, 1, 30, 2, default(ImU8String), disabled: false, glowStrength) == UIState.Ended)
			{
				PixService.UpdateTerritory(SelectedPix);
			}
			ImGui.SameLine();
			ref short room = ref tProps.Room;
			glowStrength = num7;
			if (ImGuiEx.Drag<short>("Room##room", ref room, 0.1f, 1, 90, 2, default(ImU8String), disabled: false, glowStrength) == UIState.Ended)
			{
				PixService.UpdateTerritory(SelectedPix);
			}
			break;
		}
		case ResidentialType.ApartmentLobby:
		{
			ref short ward = ref tProps.Ward;
			float glowStrength = num7;
			if (ImGuiEx.Drag<short>("Ward##ward", ref ward, 0.1f, 1, 30, 2, default(ImU8String), disabled: false, glowStrength) == UIState.Ended)
			{
				PixService.UpdateTerritory(SelectedPix);
			}
			break;
		}
		case ResidentialType.Workshop:
			break;
		}
	}

	private void DrawBrowserTab()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05eb: Unknown result type (might be due to invalid IL or missing references)
		BrowserPixProperties browser = SelectedPix.Browser;
		Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
		PixVariant variant = PixService.GetVariant(SelectedPix, create: true);
		ImGuiEx.StyledText(ImU8String.op_Implicit("Local Properties"), null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, null, null, null, null, null, null, null, null, float.MaxValue, multiline: false, "Local Properties", "These are properties which you can configure locally for a pix, they are not synced to other clients.");
		if (ImGuiEx.Checkbox("Persistent Cache", ref variant.PersistentCache, disabled: false, "Persistent Cache", "Disables automatic clearing of cached data when this Pix despawns."))
		{
			Config.Save();
		}
		ImGui.SameLine();
		if (ImGuiEx.Checkbox("Shared Cookies", ref variant.SyncCookies, disabled: false, "Shared Cookies", "Whether this Pix should share cookies via the Host environment.\nThis allows auto login to web services across Pix environments.\nCan cause session issues with some services like Youtube.\nNote: Cookies are not synced with other users, this option only controls sharing cookies between local Pix environments."))
		{
			Config.Save();
		}
		if (ImGuiEx.Checkbox("Screen Interaction", ref variant.ScreenInteraction, disabled: false, "Screen Interaction", "Whether the rendered screen of this Pix can receive mouse/keyboard interactions to send to browser.\n\nKeyboard input is locked to the screen while it has focus.\nYou can release focus either by clicking outside of the screen or pressing Escape.\nYou can configure conditions for interactions from the main config window in Shared Browser Properties."))
		{
			Config.Save();
		}
		ImGuiEx.Separator(contentRegionAvail.X - Spacing, Spacing);
		bool flag = true;
		if (SelectedPix is SyncedPix syncedPix)
		{
			flag = syncedPix.CanSyncEdit && SyncService.IsConnectedAuth;
		}
		if (ImGuiEx.StyledInput(ImU8String.op_Implicit("##uri"), ref browser.Uri, "Uri", !flag, 65535, contentRegionAvail.X - Spacing, (ImGuiInputTextFlags)16, "Current Uri", "Uri updates when navigating to other pages.\nLocal files are also supported with file:/// scheme (but won't be synced).", null, (FontAwesomeIcon)0, null, (FontAwesomeIcon)0) == UIState.Ended)
		{
			PixService.UpdateUri(SelectedPix);
		}
		PixFieldBinding<bool> pixFieldBinding = PixService.BindBrowserField(SelectedPix, (BrowserPixProperties p) => p.GpuAcceleration, delegate(BrowserPixProperties p, bool v)
		{
			p.GpuAcceleration = v;
		}, (BrowserPixVariantOverrides o) => o.GpuAcceleration, delegate(BrowserPixVariantOverrides o, bool? v)
		{
			o.GpuAcceleration = v;
		});
		bool value = pixFieldBinding.Value;
		if (ImGuiEx.Checkbox("GPU Acceleration", ref value, disabled: false, "GPU Acceleration", "You may need to disable this when viewing DRM protected content.\nChanges will only be applied when the Pix is restarted."))
		{
			pixFieldBinding.Commit(value);
		}
		DrawSyncOverrideContext(pixFieldBinding, "##syncBrowserGpu");
		ImGuiEx.Separator(contentRegionAvail.X - Spacing);
		PixFieldBinding<BrowserScaleMode> pixFieldBinding2 = PixService.BindBrowserField(SelectedPix, (BrowserPixProperties p) => p.ScaleMode, delegate(BrowserPixProperties p, BrowserScaleMode v)
		{
			p.ScaleMode = v;
		}, (BrowserPixVariantOverrides o) => o.ScaleMode, delegate(BrowserPixVariantOverrides o, BrowserScaleMode? v)
		{
			o.ScaleMode = v;
		});
		BrowserScaleMode value2 = pixFieldBinding2.Value;
		float num = 190f * ImGuiHelpers.GlobalScale;
		if (ImGuiEx.EnumCombo("##scaleMode", string.Empty, ref value2, ComboButtonDisplayType.Items, disabled: false, "Render Scale Mode", "Determines how the texture received from the browser should be scaled.\n- BrowserWindow: Use same scale as the interactive browser.\n- GameWindow: Use the same scale as the game window.\n- GameWindowWhenHidden: Use game scale while browser is collapsed/closed, otherwise use browser scale.\n- CustomScale: Use custom scale defined by the Width/Height inputs.\n- CustomScaleWhenHidden: Use custom scale while browser is collapsed/closed, otherwise use browser scale.", 6, num))
		{
			pixFieldBinding2.Commit(value2);
		}
		DrawSyncOverrideContext(pixFieldBinding2, "##syncBrowserScaleMode");
		ImGui.SameLine(0f, Spacing);
		float num2 = (contentRegionAvail.X - num - Spacing * 3f) * 0.5f;
		PixFieldBinding<uint> pixFieldBinding3 = PixService.BindBrowserField(SelectedPix, (BrowserPixProperties p) => p.CustomScaleWidth, delegate(BrowserPixProperties p, uint v)
		{
			p.CustomScaleWidth = v;
		}, (BrowserPixVariantOverrides o) => o.CustomScaleWidth, delegate(BrowserPixVariantOverrides o, uint? v)
		{
			o.CustomScaleWidth = v;
		});
		uint value3 = pixFieldBinding3.Value;
		float width = num2;
		UIState uIState = ImGuiEx.Drag("Width##scaleWidth", ref value3, 1f, 5u, 5120u, 0, default(ImU8String), disabled: false, width, null, 0.1f, insetLabel: true, "Custom Scale Width");
		if (uIState != UIState.None)
		{
			pixFieldBinding3.Commit(value3, uIState == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding3, "##syncBrowserScaleWidth");
		ImGui.SameLine(0f, Spacing);
		PixFieldBinding<uint> pixFieldBinding4 = PixService.BindBrowserField(SelectedPix, (BrowserPixProperties p) => p.CustomScaleHeight, delegate(BrowserPixProperties p, uint v)
		{
			p.CustomScaleHeight = v;
		}, (BrowserPixVariantOverrides o) => o.CustomScaleHeight, delegate(BrowserPixVariantOverrides o, uint? v)
		{
			o.CustomScaleHeight = v;
		});
		uint value4 = pixFieldBinding4.Value;
		width = num2;
		UIState uIState2 = ImGuiEx.Drag("Height##scaleHeight", ref value4, 1f, 5u, 5120u, 0, default(ImU8String), disabled: false, width, null, 0.1f, insetLabel: true, "Custom Scale Height");
		if (uIState2 != UIState.None)
		{
			pixFieldBinding4.Commit(value4, uIState2 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding4, "##syncBrowserScaleHeight");
		UIState uIState3 = uIState | uIState2;
		if (uIState3 == UIState.Using && !BrowserService.IsRescaling)
		{
			BrowserService.IsRescaling = true;
		}
		else if (uIState3 == UIState.Ended && BrowserService.IsRescaling)
		{
			BrowserService.IsRescaling = false;
		}
	}

	private void DrawRendererTab()
	{
		//IL_085c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0862: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a7e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a84: Unknown result type (might be due to invalid IL or missing references)
		//IL_0af2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0af8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e00: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e06: Unknown result type (might be due to invalid IL or missing references)
		Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
		PixFieldBinding<Vector3> posBinding = PixService.BindRendererTransformField(SelectedPix, (RendererPixProperties p) => p.Position, delegate(RendererPixProperties p, Vector3 v)
		{
			p.Position = v;
		}, (RendererPixVariantOverrides o) => o.Position, delegate(RendererPixVariantOverrides o, Vector3? v)
		{
			o.Position = v;
		});
		PixFieldBinding<Quaternion> rotBinding = PixService.BindRendererTransformField(SelectedPix, (RendererPixProperties p) => p.Rotation, delegate(RendererPixProperties p, Quaternion v)
		{
			p.Rotation = v;
		}, (RendererPixVariantOverrides o) => o.Rotation, delegate(RendererPixVariantOverrides o, Quaternion? v)
		{
			o.Rotation = v;
		});
		PixFieldBinding<Vector3> sclBinding = PixService.BindRendererTransformField(SelectedPix, (RendererPixProperties p) => p.Scale, delegate(RendererPixProperties p, Vector3 v)
		{
			p.Scale = v;
		}, (RendererPixVariantOverrides o) => o.Scale, delegate(RendererPixVariantOverrides o, Vector3? v)
		{
			o.Scale = v;
		});
		Vector3 pos = posBinding.Value;
		Quaternion rot = rotBinding.Value;
		Vector3 scl = sclBinding.Value;
		UIState uIState = TransformEditor.DrawTable("##rendererTable", ref pos, ref rot, ref scl, delegate(string id)
		{
			DrawSyncOverrideContext(posBinding, "##syncRendererPos" + id);
		}, delegate(string id)
		{
			DrawSyncOverrideContext(rotBinding, "##syncRendererRot" + id);
		}, delegate(string id)
		{
			DrawSyncOverrideContext(sclBinding, "##syncRendererScl" + id);
		});
		if (uIState != UIState.None)
		{
			posBinding.Commit(pos, editFinished: false);
			rotBinding.Commit(rot, editFinished: false);
			sclBinding.Commit(scl, uIState == UIState.Ended);
		}
		uIState = TransformEditor.DrawGizmo("##rendererGizmo", ref pos, ref rot, ref scl, (ImGuizmoMode)0);
		if (uIState != UIState.None)
		{
			posBinding.Commit(pos, editFinished: false);
			rotBinding.Commit(rot, editFinished: false);
			sclBinding.Commit(scl, uIState == UIState.Ended);
		}
		ImGuiEx.Separator(contentRegionAvail.X - Spacing);
		PixFieldBinding<Vector4> pixFieldBinding = PixService.BindRendererPropertyField(SelectedPix, (RendererPixProperties p) => p.ScreenTint, delegate(RendererPixProperties p, Vector4 v)
		{
			p.ScreenTint = v;
		}, (RendererPixVariantOverrides o) => o.ScreenTint, delegate(RendererPixVariantOverrides o, Vector4? v)
		{
			o.ScreenTint = v;
		});
		PixFieldBinding<Vector4> pixFieldBinding2 = PixService.BindRendererPropertyField(SelectedPix, (RendererPixProperties p) => p.EdgeColour, delegate(RendererPixProperties p, Vector4 v)
		{
			p.EdgeColour = v;
		}, (RendererPixVariantOverrides o) => o.EdgeColour, delegate(RendererPixVariantOverrides o, Vector4? v)
		{
			o.EdgeColour = v;
		});
		PixFieldBinding<Vector4> pixFieldBinding3 = PixService.BindRendererPropertyField(SelectedPix, (RendererPixProperties p) => p.BackColour, delegate(RendererPixProperties p, Vector4 v)
		{
			p.BackColour = v;
		}, (RendererPixVariantOverrides o) => o.BackColour, delegate(RendererPixVariantOverrides o, Vector4? v)
		{
			o.BackColour = v;
		});
		Vector4 value = pixFieldBinding.Value;
		Vector4 value2 = pixFieldBinding2.Value;
		Vector4 value3 = pixFieldBinding3.Value;
		UIState uIState2 = ImGuiEx.ColorPicker4("Screen Tint##screenTint", ref value);
		if (uIState2 != UIState.None)
		{
			pixFieldBinding.Commit(value, uIState2 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding, "##syncRendererScreenTint");
		ImGui.SameLine();
		UIState uIState3 = ImGuiEx.ColorPicker4("Edge Colour##edgeColour", ref value2);
		if (uIState3 != UIState.None)
		{
			pixFieldBinding2.Commit(value2, uIState3 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding2, "##syncRendererEdgeColour");
		ImGui.SameLine();
		UIState uIState4 = ImGuiEx.ColorPicker4("Back Colour##backColour", ref value3);
		if (uIState4 != UIState.None)
		{
			pixFieldBinding3.Commit(value3, uIState4 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding3, "##syncRendererBackColour");
		ImGuiEx.Separator(contentRegionAvail.X - Spacing);
		PixFieldBinding<Vector4> pixFieldBinding4 = PixService.BindRendererPropertyField(SelectedPix, (RendererPixProperties p) => p.BorderColour, delegate(RendererPixProperties p, Vector4 v)
		{
			p.BorderColour = v;
		}, (RendererPixVariantOverrides o) => o.BorderColour, delegate(RendererPixVariantOverrides o, Vector4? v)
		{
			o.BorderColour = v;
		});
		PixFieldBinding<BorderMode> pixFieldBinding5 = PixService.BindRendererPropertyField(SelectedPix, (RendererPixProperties p) => p.BorderMode, delegate(RendererPixProperties p, BorderMode v)
		{
			p.BorderMode = v;
		}, (RendererPixVariantOverrides o) => o.BorderMode, delegate(RendererPixVariantOverrides o, BorderMode? v)
		{
			o.BorderMode = v;
		});
		PixFieldBinding<float> pixFieldBinding6 = PixService.BindRendererPropertyField(SelectedPix, (RendererPixProperties p) => p.BorderWidthH, delegate(RendererPixProperties p, float v)
		{
			p.BorderWidthH = v;
		}, (RendererPixVariantOverrides o) => o.BorderWidthH, delegate(RendererPixVariantOverrides o, float? v)
		{
			o.BorderWidthH = v;
		});
		PixFieldBinding<float> pixFieldBinding7 = PixService.BindRendererPropertyField(SelectedPix, (RendererPixProperties p) => p.BorderWidthV, delegate(RendererPixProperties p, float v)
		{
			p.BorderWidthV = v;
		}, (RendererPixVariantOverrides o) => o.BorderWidthV, delegate(RendererPixVariantOverrides o, float? v)
		{
			o.BorderWidthV = v;
		});
		Vector4 value4 = pixFieldBinding4.Value;
		UIState uIState5 = ImGuiEx.ColorPicker4("Border Colour##borderColour", ref value4);
		if (uIState5 != UIState.None)
		{
			pixFieldBinding4.Commit(value4, uIState5 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding4, "##syncRendererBorderColour");
		BorderMode value5 = pixFieldBinding5.Value;
		if (ImGuiEx.EnumCombo("##borderMode", "Border Mode: ", ref value5, ComboButtonDisplayType.Items, disabled: false, null, null, 6, contentRegionAvail.X - Spacing))
		{
			pixFieldBinding5.Commit(value5);
		}
		DrawSyncOverrideContext(pixFieldBinding5, "##syncRendererBorderMode");
		float value6 = pixFieldBinding6.Value;
		float width = contentRegionAvail.X - Spacing;
		UIState uIState6 = ImGuiEx.Drag("HBorder Width##hBorderWidth", ref value6, 0.001f, 0f, 1f, 2, default(ImU8String), disabled: false, width);
		if (uIState6 != UIState.None)
		{
			pixFieldBinding6.Commit(value6, uIState6 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding6, "##syncRendererBorderWidthH");
		float value7 = pixFieldBinding7.Value;
		width = contentRegionAvail.X - Spacing;
		UIState uIState7 = ImGuiEx.Drag("VBorder Width##vBorderWidth", ref value7, 0.001f, 0f, 1f, 2, default(ImU8String), disabled: false, width);
		if (uIState7 != UIState.None)
		{
			pixFieldBinding7.Commit(value7, uIState7 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding7, "##syncRendererBorderWidthV");
		ImGuiEx.Separator(contentRegionAvail.X - Spacing);
		PixFieldBinding<float> pixFieldBinding8 = PixService.BindRendererPropertyField(SelectedPix, (RendererPixProperties p) => p.BorderFeather, delegate(RendererPixProperties p, float v)
		{
			p.BorderFeather = v;
		}, (RendererPixVariantOverrides o) => o.BorderFeather, delegate(RendererPixVariantOverrides o, float? v)
		{
			o.BorderFeather = v;
		});
		PixFieldBinding<float> pixFieldBinding9 = PixService.BindRendererPropertyField(SelectedPix, (RendererPixProperties p) => p.EdgeFeather, delegate(RendererPixProperties p, float v)
		{
			p.EdgeFeather = v;
		}, (RendererPixVariantOverrides o) => o.EdgeFeather, delegate(RendererPixVariantOverrides o, float? v)
		{
			o.EdgeFeather = v;
		});
		float value8 = pixFieldBinding8.Value;
		width = contentRegionAvail.X - Spacing;
		UIState uIState8 = ImGuiEx.Drag("Border Feather##borderFeather", ref value8, 0.001f, 0f, 10f, 2, default(ImU8String), disabled: false, width);
		if (uIState8 != UIState.None)
		{
			pixFieldBinding8.Commit(value8, uIState8 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding8, "##syncRendererBorderFeather");
		float value9 = pixFieldBinding9.Value;
		width = contentRegionAvail.X - Spacing;
		UIState uIState9 = ImGuiEx.Drag("Edge Feather##edgeFeather", ref value9, 0.001f, 0f, 10f, 2, default(ImU8String), disabled: false, width);
		if (uIState9 != UIState.None)
		{
			pixFieldBinding9.Commit(value9, uIState9 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding9, "##syncRendererEdgeFeather");
		ImGuiEx.Separator(contentRegionAvail.X - Spacing);
		PixFieldBinding<bool> pixFieldBinding10 = PixService.BindRendererPropertyField(SelectedPix, (RendererPixProperties p) => p.Depth, delegate(RendererPixProperties p, bool v)
		{
			p.Depth = v;
		}, (RendererPixVariantOverrides o) => o.Depth, delegate(RendererPixVariantOverrides o, bool? v)
		{
			o.Depth = v;
		});
		PixFieldBinding<float> pixFieldBinding11 = PixService.BindRendererPropertyField(SelectedPix, (RendererPixProperties p) => p.DepthOffset, delegate(RendererPixProperties p, float v)
		{
			p.DepthOffset = v;
		}, (RendererPixVariantOverrides o) => o.DepthOffset, delegate(RendererPixVariantOverrides o, float? v)
		{
			o.DepthOffset = v;
		});
		PixFieldBinding<DepthComparison> pixFieldBinding12 = PixService.BindRendererPropertyField(SelectedPix, (RendererPixProperties p) => p.DepthComparison, delegate(RendererPixProperties p, DepthComparison v)
		{
			p.DepthComparison = v;
		}, (RendererPixVariantOverrides o) => o.DepthComparison, delegate(RendererPixVariantOverrides o, DepthComparison? v)
		{
			o.DepthComparison = v;
		});
		PixFieldBinding<CullMode> pixFieldBinding13 = PixService.BindRendererPropertyField(SelectedPix, (RendererPixProperties p) => p.CullMode, delegate(RendererPixProperties p, CullMode v)
		{
			p.CullMode = v;
		}, (RendererPixVariantOverrides o) => o.CullMode, delegate(RendererPixVariantOverrides o, CullMode? v)
		{
			o.CullMode = v;
		});
		bool value10 = pixFieldBinding10.Value;
		if (ImGuiEx.Checkbox("Enable Depth##enableDepth", ref value10))
		{
			pixFieldBinding10.Commit(value10);
		}
		DrawSyncOverrideContext(pixFieldBinding10, "##syncRendererDepth");
		float value11 = pixFieldBinding11.Value;
		bool disabled = !value10;
		width = contentRegionAvail.X - Spacing;
		UIState uIState10 = ImGuiEx.Drag("Depth Offset##depthOffset", ref value11, 0.001f, 0f, 10f, 2, default(ImU8String), disabled, width);
		if (uIState10 != UIState.None)
		{
			pixFieldBinding11.Commit(value11, uIState10 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding11, "##syncRendererDepthOffset");
		DepthComparison value12 = pixFieldBinding12.Value;
		if (ImGuiEx.EnumCombo("##depthComp", "Depth Comparison: ", ref value12, ComboButtonDisplayType.Items, !value10, null, null, 6, contentRegionAvail.X - Spacing))
		{
			pixFieldBinding12.Commit(value12);
		}
		DrawSyncOverrideContext(pixFieldBinding12, "##syncRendererDepthComp");
		CullMode value13 = pixFieldBinding13.Value;
		if (ImGuiEx.EnumCombo("##cullMode", "Cull Mode: ", ref value13, ComboButtonDisplayType.Items, disabled: false, null, null, 6, contentRegionAvail.X - Spacing))
		{
			pixFieldBinding13.Commit(value13);
		}
		DrawSyncOverrideContext(pixFieldBinding13, "##syncRendererCullMode");
	}

	private void DrawLightTab()
	{
		//IL_05c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_06eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0901: Unknown result type (might be due to invalid IL or missing references)
		//IL_0907: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a12: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b33: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b39: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c3e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c44: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e52: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e58: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f5d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f63: Unknown result type (might be due to invalid IL or missing references)
		//IL_1167: Unknown result type (might be due to invalid IL or missing references)
		//IL_116d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1272: Unknown result type (might be due to invalid IL or missing references)
		//IL_1278: Unknown result type (might be due to invalid IL or missing references)
		//IL_137d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1383: Unknown result type (might be due to invalid IL or missing references)
		Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
		PixFieldBinding<Vector3> posBinding = PixService.BindLightTransformField(SelectedPix, (LightPixProperties p) => p.Position, delegate(LightPixProperties p, Vector3 v)
		{
			p.Position = v;
		}, (LightPixVariantOverrides o) => o.Position, delegate(LightPixVariantOverrides o, Vector3? v)
		{
			o.Position = v;
		});
		PixFieldBinding<Quaternion> rotBinding = PixService.BindLightTransformField(SelectedPix, (LightPixProperties p) => p.Rotation, delegate(LightPixProperties p, Quaternion v)
		{
			p.Rotation = v;
		}, (LightPixVariantOverrides o) => o.Rotation, delegate(LightPixVariantOverrides o, Quaternion? v)
		{
			o.Rotation = v;
		});
		Vector3 pos = posBinding.Value;
		Quaternion rot = rotBinding.Value;
		UIState uIState = TransformEditor.DrawTable("##lightTable", ref pos, ref rot, delegate(string id)
		{
			DrawSyncOverrideContext(posBinding, "##syncRendererPos" + id);
		}, delegate(string id)
		{
			DrawSyncOverrideContext(rotBinding, "##syncRendererRot" + id);
		});
		if (uIState != UIState.None)
		{
			bool editFinished = uIState == UIState.Ended;
			posBinding.Commit(pos, editFinished: false);
			rotBinding.Commit(rot, editFinished);
		}
		RendererPixProperties renderer = SelectedPix.Renderer;
		Vector3 pos2 = Vector3.Transform(pos, renderer.Rotation) + renderer.Position;
		Quaternion rot2 = Quaternion.Normalize(Quaternion.Multiply(renderer.Rotation, rot));
		uIState = TransformEditor.DrawGizmo("##lightGizmo", ref pos2, ref rot2, (ImGuizmoMode)0);
		if (uIState != UIState.None)
		{
			Quaternion quaternion = Quaternion.Inverse(renderer.Rotation);
			Vector3 vector = Vector3.Transform(pos2 - renderer.Position, quaternion);
			Quaternion quaternion2 = Quaternion.Normalize(Quaternion.Multiply(quaternion, rot2));
			bool editFinished2 = uIState == UIState.Ended;
			posBinding.Commit(vector, editFinished: false);
			rotBinding.Commit(quaternion2, editFinished2);
			pos = vector;
			rot = quaternion2;
		}
		ImGuiEx.Separator(contentRegionAvail.X - Spacing);
		PixFieldBinding<bool> pixFieldBinding = PixService.BindLightPropertyField(SelectedPix, (LightPixProperties p) => p.Enabled, delegate(LightPixProperties p, bool v)
		{
			p.Enabled = v;
		}, (LightPixVariantOverrides o) => o.Enabled, delegate(LightPixVariantOverrides o, bool? v)
		{
			o.Enabled = v;
		});
		bool value = pixFieldBinding.Value;
		if (ImGuiEx.Checkbox("Enable Light##enableLight", ref value))
		{
			pixFieldBinding.Commit(value);
		}
		DrawSyncOverrideContext(pixFieldBinding, "##syncLightEnabled");
		PixFieldBinding<LightType> pixFieldBinding2 = PixService.BindLightPropertyField(SelectedPix, (LightPixProperties p) => p.LightType, delegate(LightPixProperties p, LightType v)
		{
			p.LightType = v;
		}, (LightPixVariantOverrides o) => o.LightType, delegate(LightPixVariantOverrides o, LightType? v)
		{
			o.LightType = v;
		});
		LightType value2 = pixFieldBinding2.Value;
		if (ImGuiEx.EnumCombo("##lightType", "Light Type: ", ref value2, ComboButtonDisplayType.Items, !value, null, null, 6, contentRegionAvail.X - Spacing))
		{
			pixFieldBinding2.Commit(value2);
		}
		DrawSyncOverrideContext(pixFieldBinding2, "##syncLightType");
		PixFieldBinding<Vector4> pixFieldBinding3 = PixService.BindLightPropertyField(SelectedPix, (LightPixProperties p) => p.Colour, delegate(LightPixProperties p, Vector4 v)
		{
			p.Colour = v;
		}, (LightPixVariantOverrides o) => o.Colour, delegate(LightPixVariantOverrides o, Vector4? v)
		{
			o.Colour = v;
		});
		Vector4 value3 = pixFieldBinding3.Value;
		UIState uIState2 = ImGuiEx.ColorPicker4("Colour##lightColour", ref value3);
		if (uIState2 != UIState.None)
		{
			pixFieldBinding3.Commit(value3, uIState2 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding3, "##syncLightColour");
		PixFieldBinding<float> pixFieldBinding4 = PixService.BindLightPropertyField(SelectedPix, (LightPixProperties p) => p.Intensity, delegate(LightPixProperties p, float v)
		{
			p.Intensity = v;
		}, (LightPixVariantOverrides o) => o.Intensity, delegate(LightPixVariantOverrides o, float? v)
		{
			o.Intensity = v;
		});
		float value4 = pixFieldBinding4.Value;
		bool disabled = !value;
		float width = contentRegionAvail.X - Spacing;
		UIState uIState3 = ImGuiEx.Drag("Intensity##lightIntensity", ref value4, 0.01f, 0f, 100f, 2, default(ImU8String), disabled, width);
		if (uIState3 != UIState.None)
		{
			pixFieldBinding4.Commit(value4, uIState3 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding4, "##syncLightIntensity");
		ImGuiEx.Separator(contentRegionAvail.X - Spacing);
		PixFieldBinding<float> pixFieldBinding5 = PixService.BindLightPropertyField(SelectedPix, (LightPixProperties p) => p.ScreenColourInfluence, delegate(LightPixProperties p, float v)
		{
			p.ScreenColourInfluence = v;
		}, (LightPixVariantOverrides o) => o.ScreenColourInfluence, delegate(LightPixVariantOverrides o, float? v)
		{
			o.ScreenColourInfluence = v;
		});
		float value5 = pixFieldBinding5.Value;
		disabled = !value;
		width = contentRegionAvail.X - Spacing;
		UIState uIState4 = ImGuiEx.Drag("Screen Influence##screenInfluence", ref value5, 0.001f, 0f, 1f, 2, default(ImU8String), disabled, width);
		if (uIState4 != UIState.None)
		{
			pixFieldBinding5.Commit(value5, uIState4 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding5, "##syncLightInfluence");
		PixFieldBinding<float> pixFieldBinding6 = PixService.BindLightPropertyField(SelectedPix, (LightPixProperties p) => p.InfluenceColourIntensity, delegate(LightPixProperties p, float v)
		{
			p.InfluenceColourIntensity = v;
		}, (LightPixVariantOverrides o) => o.InfluenceColourIntensity, delegate(LightPixVariantOverrides o, float? v)
		{
			o.InfluenceColourIntensity = v;
		});
		float value6 = pixFieldBinding6.Value;
		disabled = !value;
		width = contentRegionAvail.X - Spacing;
		UIState uIState5 = ImGuiEx.Drag("Screen Colour Intensity##colourIntensity", ref value6, 0.001f, 0f, 10f, 2, default(ImU8String), disabled, width);
		if (uIState5 != UIState.None)
		{
			pixFieldBinding6.Commit(value6, uIState5 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding6, "##syncLightColourIntensity");
		PixFieldBinding<float> pixFieldBinding7 = PixService.BindLightPropertyField(SelectedPix, (LightPixProperties p) => p.InfluenceBrightnessIntensity, delegate(LightPixProperties p, float v)
		{
			p.InfluenceBrightnessIntensity = v;
		}, (LightPixVariantOverrides o) => o.InfluenceBrightnessIntensity, delegate(LightPixVariantOverrides o, float? v)
		{
			o.InfluenceBrightnessIntensity = v;
		});
		float value7 = pixFieldBinding7.Value;
		disabled = !value;
		width = contentRegionAvail.X - Spacing;
		UIState uIState6 = ImGuiEx.Drag("Screen Brightness Intensity##brightnessIntensity", ref value7, 0.001f, 0f, 10f, 2, default(ImU8String), disabled, width);
		if (uIState6 != UIState.None)
		{
			pixFieldBinding7.Commit(value7, uIState6 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding7, "##syncLightBrightnessIntensity");
		PixFieldBinding<float> pixFieldBinding8 = PixService.BindLightPropertyField(SelectedPix, (LightPixProperties p) => p.InfluenceGammaCurve, delegate(LightPixProperties p, float v)
		{
			p.InfluenceGammaCurve = v;
		}, (LightPixVariantOverrides o) => o.InfluenceGammaCurve, delegate(LightPixVariantOverrides o, float? v)
		{
			o.InfluenceGammaCurve = v;
		});
		float value8 = pixFieldBinding8.Value;
		disabled = !value;
		width = contentRegionAvail.X - Spacing;
		UIState uIState7 = ImGuiEx.Drag("Screen Gamma Curve##gammaCurve", ref value8, 0.001f, 0f, 1f, 2, default(ImU8String), disabled, width);
		if (uIState7 != UIState.None)
		{
			pixFieldBinding8.Commit(value8, uIState7 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding8, "##syncLightGamma");
		ImGuiEx.Separator(contentRegionAvail.X - Spacing);
		PixFieldBinding<float> pixFieldBinding9 = PixService.BindLightPropertyField(SelectedPix, (LightPixProperties p) => p.Range, delegate(LightPixProperties p, float v)
		{
			p.Range = v;
		}, (LightPixVariantOverrides o) => o.Range, delegate(LightPixVariantOverrides o, float? v)
		{
			o.Range = v;
		});
		float value9 = pixFieldBinding9.Value;
		disabled = !value;
		width = contentRegionAvail.X - Spacing;
		UIState uIState8 = ImGuiEx.Drag("Light Range##lightRange", ref value9, 0.01f, 0f, 100f, 2, default(ImU8String), disabled, width);
		if (uIState8 != UIState.None)
		{
			pixFieldBinding9.Commit(value9, uIState8 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding9, "##syncLightRange");
		PixFieldBinding<float> pixFieldBinding10 = PixService.BindLightPropertyField(SelectedPix, (LightPixProperties p) => p.LightAngle, delegate(LightPixProperties p, float v)
		{
			p.LightAngle = v;
		}, (LightPixVariantOverrides o) => o.LightAngle, delegate(LightPixVariantOverrides o, float? v)
		{
			o.LightAngle = v;
		});
		float value10 = pixFieldBinding10.Value;
		disabled = !value;
		width = contentRegionAvail.X - Spacing;
		UIState uIState9 = ImGuiEx.Drag("Light Angle##lightAngle", ref value10, 0.01f, 0f, 180f, 2, default(ImU8String), disabled, width);
		if (uIState9 != UIState.None)
		{
			pixFieldBinding10.Commit(value10, uIState9 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding10, "##syncLightAngle");
		ImGuiEx.Separator(contentRegionAvail.X - Spacing);
		PixFieldBinding<FalloffType> pixFieldBinding11 = PixService.BindLightPropertyField(SelectedPix, (LightPixProperties p) => p.FalloffType, delegate(LightPixProperties p, FalloffType v)
		{
			p.FalloffType = v;
		}, (LightPixVariantOverrides o) => o.FalloffType, delegate(LightPixVariantOverrides o, FalloffType? v)
		{
			o.FalloffType = v;
		});
		FalloffType value11 = pixFieldBinding11.Value;
		if (ImGuiEx.EnumCombo("##falloffType", "Falloff Type: ", ref value11, ComboButtonDisplayType.Items, !value, null, null, 6, contentRegionAvail.X - Spacing))
		{
			pixFieldBinding11.Commit(value11);
		}
		DrawSyncOverrideContext(pixFieldBinding11, "##syncLightFalloffType");
		PixFieldBinding<float> pixFieldBinding12 = PixService.BindLightPropertyField(SelectedPix, (LightPixProperties p) => p.FalloffAngle, delegate(LightPixProperties p, float v)
		{
			p.FalloffAngle = v;
		}, (LightPixVariantOverrides o) => o.FalloffAngle, delegate(LightPixVariantOverrides o, float? v)
		{
			o.FalloffAngle = v;
		});
		float value12 = pixFieldBinding12.Value;
		disabled = !value;
		width = contentRegionAvail.X - Spacing;
		UIState uIState10 = ImGuiEx.Drag("Falloff Angle##falloffAngle", ref value12, 0.01f, 0f, 180f, 2, default(ImU8String), disabled, width);
		if (uIState10 != UIState.None)
		{
			pixFieldBinding12.Commit(value12, uIState10 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding12, "##syncLightFalloffAngle");
		PixFieldBinding<float> pixFieldBinding13 = PixService.BindLightPropertyField(SelectedPix, (LightPixProperties p) => p.FalloffPower, delegate(LightPixProperties p, float v)
		{
			p.FalloffPower = v;
		}, (LightPixVariantOverrides o) => o.FalloffPower, delegate(LightPixVariantOverrides o, float? v)
		{
			o.FalloffPower = v;
		});
		float value13 = pixFieldBinding13.Value;
		disabled = !value;
		width = contentRegionAvail.X - Spacing;
		UIState uIState11 = ImGuiEx.Drag("Falloff Power##falloffPower", ref value13, 0.01f, 0f, 100f, 2, default(ImU8String), disabled, width);
		if (uIState11 != UIState.None)
		{
			pixFieldBinding13.Commit(value13, uIState11 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding13, "##syncLightFalloffPower");
		ImGuiEx.Separator(contentRegionAvail.X - Spacing);
		PixFieldBinding<LightFlags> pixFieldBinding14 = PixService.BindLightPropertyField(SelectedPix, (LightPixProperties p) => p.Flags, delegate(LightPixProperties p, LightFlags v)
		{
			p.Flags = v;
		}, (LightPixVariantOverrides o) => o.Flags, delegate(LightPixVariantOverrides o, LightFlags? v)
		{
			o.Flags = v;
		});
		LightFlags value14 = pixFieldBinding14.Value;
		if (ImGuiEx.EnumFlagsCombo("##shadowFlags", "Shadow Flags", ref value14, ComboButtonDisplayType.Label, !value, null, null, 6, contentRegionAvail.X - Spacing))
		{
			pixFieldBinding14.Commit(value14);
		}
		DrawSyncOverrideContext(pixFieldBinding14, "##syncLightFlags");
		PixFieldBinding<float> pixFieldBinding15 = PixService.BindLightPropertyField(SelectedPix, (LightPixProperties p) => p.ShadowRange, delegate(LightPixProperties p, float v)
		{
			p.ShadowRange = v;
		}, (LightPixVariantOverrides o) => o.ShadowRange, delegate(LightPixVariantOverrides o, float? v)
		{
			o.ShadowRange = v;
		});
		float value15 = pixFieldBinding15.Value;
		disabled = !value;
		width = contentRegionAvail.X - Spacing;
		UIState uIState12 = ImGuiEx.Drag("Shadow Range##shadowRange", ref value15, 0.01f, 0f, 50f, 2, default(ImU8String), disabled, width);
		if (uIState12 != UIState.None)
		{
			pixFieldBinding15.Commit(value15, uIState12 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding15, "##syncLightShadowRange");
		PixFieldBinding<float> pixFieldBinding16 = PixService.BindLightPropertyField(SelectedPix, (LightPixProperties p) => p.ShadowNear, delegate(LightPixProperties p, float v)
		{
			p.ShadowNear = v;
		}, (LightPixVariantOverrides o) => o.ShadowNear, delegate(LightPixVariantOverrides o, float? v)
		{
			o.ShadowNear = v;
		});
		float value16 = pixFieldBinding16.Value;
		disabled = !value;
		width = contentRegionAvail.X - Spacing;
		UIState uIState13 = ImGuiEx.Drag("Shadow Near##shadowNear", ref value16, 0.01f, 0f, 50f, 2, default(ImU8String), disabled, width);
		if (uIState13 != UIState.None)
		{
			pixFieldBinding16.Commit(value16, uIState13 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding16, "##syncLightShadowNear");
		PixFieldBinding<float> pixFieldBinding17 = PixService.BindLightPropertyField(SelectedPix, (LightPixProperties p) => p.ShadowFar, delegate(LightPixProperties p, float v)
		{
			p.ShadowFar = v;
		}, (LightPixVariantOverrides o) => o.ShadowFar, delegate(LightPixVariantOverrides o, float? v)
		{
			o.ShadowFar = v;
		});
		float value17 = pixFieldBinding17.Value;
		disabled = !value;
		width = contentRegionAvail.X - Spacing;
		UIState uIState14 = ImGuiEx.Drag("Shadow Far##shadowFar", ref value17, 0.01f, 0f, 50f, 2, default(ImU8String), disabled, width);
		if (uIState14 != UIState.None)
		{
			pixFieldBinding17.Commit(value17, uIState14 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding17, "##syncLightShadowFar");
	}

	private void DrawAudioTab()
	{
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
		PixFieldBinding<bool> pixFieldBinding = PixService.BindAudioField(SelectedPix, (AudioPixProperties p) => p.SpatialEnabled, delegate(AudioPixProperties p, bool v)
		{
			p.SpatialEnabled = v;
		}, (AudioPixVariantOverrides o) => o.SpatialEnabled, delegate(AudioPixVariantOverrides o, bool? v)
		{
			o.SpatialEnabled = v;
		});
		bool value = pixFieldBinding.Value;
		if (ImGuiEx.Checkbox("Enable Spatial Audio##enableSpatial", ref value))
		{
			pixFieldBinding.Commit(value);
		}
		DrawSyncOverrideContext(pixFieldBinding, "##syncSpatialAudio");
		PixFieldBinding<float> pixFieldBinding2 = PixService.BindAudioField(SelectedPix, (AudioPixProperties p) => p.Volume, delegate(AudioPixProperties p, float v)
		{
			p.Volume = v;
		}, (AudioPixVariantOverrides o) => o.Volume, delegate(AudioPixVariantOverrides o, float? v)
		{
			o.Volume = v;
		});
		float value2 = pixFieldBinding2.Value;
		bool disabled = !value;
		float width = contentRegionAvail.X - Spacing;
		UIState uIState = ImGuiEx.Drag("Volume##volume", ref value2, 0.001f, 0f, 1f, 2, default(ImU8String), disabled, width);
		if (uIState != UIState.None)
		{
			pixFieldBinding2.Commit(value2, uIState == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding2, "##syncAudioVolume");
		PixFieldBinding<float> pixFieldBinding3 = PixService.BindAudioField(SelectedPix, (AudioPixProperties p) => p.FalloffMaxDistance, delegate(AudioPixProperties p, float v)
		{
			p.FalloffMaxDistance = v;
		}, (AudioPixVariantOverrides o) => o.FalloffMaxDistance, delegate(AudioPixVariantOverrides o, float? v)
		{
			o.FalloffMaxDistance = v;
		});
		float value3 = pixFieldBinding3.Value;
		disabled = !value;
		width = contentRegionAvail.X - Spacing;
		UIState uIState2 = ImGuiEx.Drag("Falloff Distance##falloffDistance", ref value3, 0.1f, 0f, 100f, 1, default(ImU8String), disabled, width, null, 0.1f, insetLabel: true, "Falloff Max Distance", "The max distance from the rendered screen in world before volume is completely faded out.");
		if (uIState2 != UIState.None)
		{
			pixFieldBinding3.Commit(value3, uIState2 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding3, "##syncAudioFalloffDistance");
		PixFieldBinding<float> pixFieldBinding4 = PixService.BindAudioField(SelectedPix, (AudioPixProperties p) => p.FalloffStrength, delegate(AudioPixProperties p, float v)
		{
			p.FalloffStrength = v;
		}, (AudioPixVariantOverrides o) => o.FalloffStrength, delegate(AudioPixVariantOverrides o, float? v)
		{
			o.FalloffStrength = v;
		});
		float value4 = pixFieldBinding4.Value;
		disabled = !value;
		width = contentRegionAvail.X - Spacing;
		UIState uIState3 = ImGuiEx.Drag("Falloff Strength##falloffStrength", ref value4, 0.1f, 0f, 50f, 1, default(ImU8String), disabled, width, null, 0.1f, insetLabel: true, "Falloff Strength", "Controls how significant the falloff adjustment is relative to distance.");
		if (uIState3 != UIState.None)
		{
			pixFieldBinding4.Commit(value4, uIState3 == UIState.Ended);
		}
		DrawSyncOverrideContext(pixFieldBinding4, "##syncAudioFalloffStrength");
	}

	private void DrawSyncTab()
	{
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_056c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0571: Unknown result type (might be due to invalid IL or missing references)
		//IL_057f: Unknown result type (might be due to invalid IL or missing references)
		OwnerFieldBinding<SyncPixProperties> ownerFieldBinding = PixService.BindOwnerField(SelectedPix, (BasePix p) => p.Sync, delegate(BasePix p, SyncPixProperties v)
		{
			p.Sync = v;
		});
		SyncPixProperties value = ownerFieldBinding.Value;
		SyncedPix syncedPix = SelectedPix as SyncedPix;
		bool flag = ownerFieldBinding.CanEdit && (!SelectedPix.Sync.IsSynced || SyncService.IsConnectedAuth);
		bool flag2 = syncedPix == null || syncedPix.SelfRank == PixRank.Owner;
		UIState uIState = UIState.None;
		bool flag3 = false;
		Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
		float globalScale = ImGuiHelpers.GlobalScale;
		flag3 |= ImGuiEx.EnumCombo("##privacy", string.Empty, ref value.Privacy, ComboButtonDisplayType.Items, !flag, "Privacy", "Public - Pix will be publicly listed in the Sync Search window.\nUnlisted - Pix will only be listed in the Sync Search window for users in the same territory.\nPrivate - Pix will not be listed at all, Id/Password required.", 6, 80f * globalScale);
		string text = ((!flag || value.SecretKey == null) ? string.Empty : value.SecretKey);
		ImGuiEx.SpacingX(Spacing, sameLinePrior: true, sameLineAfter: true);
		uIState |= ImGuiEx.StyledInput(ImU8String.op_Implicit("##secretKey"), ref text, "Password", !flag || value.Privacy != PixPrivacy.Private, 32, ImGui.GetContentRegionAvail().X - Spacing * 2f, (ImGuiInputTextFlags)16, "Password", "Password required for joining a private pix.", null, (FontAwesomeIcon)0, null, (FontAwesomeIcon)0);
		if (flag)
		{
			if (value.Privacy == PixPrivacy.Private)
			{
				flag3 = false;
				if (uIState != UIState.None)
				{
					value.SecretKey = (string.IsNullOrWhiteSpace(text) ? null : text);
				}
			}
			else
			{
				value.SecretKey = null;
			}
		}
		flag3 |= ImGuiEx.EnumCombo("##editRank", string.Empty, ref value.EditorRank, ComboButtonDisplayType.Items, !flag2, "Editor Rank", "The minimum rank required for a user to make changes to synced properties.", 6, 80f * globalScale);
		ImGuiEx.SpacingX(Spacing, sameLinePrior: true, sameLineAfter: true);
		flag3 |= ImGuiEx.Checkbox("Nsfw##nsfw", ref value.Nsfw, !flag, "Nsfw", "Whether this pix may feature mature content.", LineHeight);
		ImGuiEx.Separator(contentRegionAvail.X - Spacing);
		if (flag && !NameUtil.ValidatePix(SelectedPix.Info.Name, SelectedPix.Info.Description, value.SecretKey, value.Privacy, SyncService.Client.Premium, out string error))
		{
			using (UIShared.SubFont.Push())
			{
				ImU8String text2 = ImU8String.op_Implicit(error);
				Vector3? colorA = new Vector3(0.6f, 0f, 0f);
				Vector3? colorB = new Vector3(1f, 0f, 0f);
				ImGuiEx.StyledText(text2, null, 0.8f, 0.4f, 4f, 0.1f, AnimationType.Pulse, colorA, colorB, null, null, null, null, null, null, float.MaxValue);
			}
			ImGuiEx.Separator(contentRegionAvail.X - Spacing);
		}
		else if (SelectedPix.Sync.IsSynced && !flag)
		{
			using (UIShared.SubFont.Push())
			{
				ImU8String val = new ImU8String(56, 0);
				((ImU8String)(ref val)).AppendLiteral("You do not have editing permissions for this synced pix.");
				ImU8String text3 = val;
				Vector3? colorB = UIShared.AccentActive.AsVector3();
				ImGuiEx.StyledText(text3, null, 0.8f, 0.3f, 4f, 0.1f, AnimationType.Static, colorB, null, null, null, null, null, null, null, float.MaxValue);
				ImU8String val2 = new ImU8String(60, 0);
				((ImU8String)(ref val2)).AppendLiteral("You can override properties: Browser, Renderer, Light, Audio");
				ImU8String text4 = val2;
				colorB = UIShared.AccentActive.AsVector3();
				ImGuiEx.StyledText(text4, null, 0.8f, 0.3f, 4f, 0.1f, AnimationType.Static, colorB, null, null, null, null, null, null, null, float.MaxValue);
				ImU8String val3 = new ImU8String(61, 0);
				((ImU8String)(ref val3)).AppendLiteral("Overridden properties can be resynced by right-clicking them.");
				ImU8String text5 = val3;
				colorB = UIShared.AccentActive.AsVector3();
				ImGuiEx.StyledText(text5, null, 0.8f, 0.3f, 4f, 0.1f, AnimationType.Static, colorB, null, null, null, null, null, null, null, float.MaxValue);
			}
			ImGuiEx.Separator(contentRegionAvail.X - Spacing);
		}
		if ((uIState == UIState.Ended || flag3) && flag && NameUtil.ValidatePix(SelectedPix.Info.Name, SelectedPix.Info.Description, value.SecretKey, value.Privacy, SyncService.Client.Premium, out string _))
		{
			ownerFieldBinding.Commit(value);
			PixService.UpdateSyncProperties(SelectedPix);
		}
		if (!SelectedPix.Sync.IsSynced)
		{
			if (ImGuiEx.IconTextButton((FontAwesomeIcon)62193, "Sync Pix", "##syncButton", !SyncService.IsConnectedAuth, "Sync Pix", "Upload this pix to the Sync Service"))
			{
				if (NameUtil.ValidatePix(SelectedPix.Info.Name, SelectedPix.Info.Description, value.SecretKey, value.Privacy, SyncService.Client.Premium, out error))
				{
					SyncService.CreateSyncedPix(SelectedPix, SelectedPix.GetSyncedMetaData());
				}
				else
				{
					StatusBar.Show(error, 4000, overlay: false, StatusType.Error);
				}
			}
		}
		else if (syncedPix != null)
		{
			if (flag2)
			{
				if (ImGuiEx.IconTextButton((FontAwesomeIcon)62189, "Unsync Pix", "##syncDeleteButton", !SyncService.IsConnectedAuth, "Unsync Pix", "Remove this pix from the Sync Service"))
				{
					SyncService.DeleteSyncedPix(syncedPix.Id);
				}
			}
			else if (ImGuiEx.IconTextButton((FontAwesomeIcon)61735, "Leave Pix", "##syncLeaveButton", !SyncService.IsConnectedAuth, "Leave Pix", "Unsubscribe from this pix"))
			{
				SyncService.UnsubscribePix(syncedPix.Id);
			}
		}
		if (!SyncService.IsConnectedAuth)
		{
			string text6 = ((SyncService.State != ConnectionState.Connected) ? "Disconnected" : ((!SyncService.Client.IsAuthenticated) ? "Authentication Required" : "Unavailable"));
			StatusBar.Show("Sync Service: " + text6, 100, overlay: false, StatusType.Error);
		}
	}

	private void DrawSyncOverrideContext<T>(PixFieldBinding<T> binding, string id)
	{
		if (SelectedPix == null || !SyncService.IsConnectedAuth)
		{
			return;
		}
		bool num = SyncOverrideContextMenu?.IsOpen(id) ?? false;
		if (ImGui.IsItemClicked((ImGuiMouseButton)1))
		{
			List<ContextMenuItem> list = new List<ContextMenuItem>();
			if (binding.HasOverride)
			{
				list.Add(new ContextMenuButton("Resync", delegate
				{
					binding.ResetOverride();
				}, closeOnClick: true, (FontAwesomeIcon)61633, null, null, ContextMenuTint.Both, ContextMenuTint.Both, () => ("Resync Property", "Resets overridden property to the synced value.")));
			}
			else if (binding.CanSyncEdit)
			{
				list.Add(new ContextMenuButton("Sync Origin", null, closeOnClick: true, (FontAwesomeIcon)61633, null, () => true, ContextMenuTint.Both, ContextMenuTint.Both, () => ("Sync Origin", "You have editing permissions for this pix.\nChanges will be synced to other connected users.")));
			}
			else
			{
				list.Add(new ContextMenuButton("Synced", null, closeOnClick: true, (FontAwesomeIcon)61633, null, () => true, ContextMenuTint.Both, ContextMenuTint.Both, () => ("Synced Property", "This property is currently synced, changes will override & desync until resynced.")));
			}
			SyncOverrideContextMenu = new ContextMenu(id, list, 100f, 26f);
			SyncOverrideContextMenu.Open(id);
		}
		if (num)
		{
			SyncOverrideContextMenu?.Draw(id);
		}
	}
}
