using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using PyonPix.Config;
using PyonPix.Config.Pix;
using PyonPix.Events;
using PyonPix.Extensions;
using PyonPix.Services;
using PyonPix.Services.Core;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Sync.Dto.Client;
using PyonPix.Structs.Browser;
using PyonPix.Ui.Components;
using PyonPix.Utility;

namespace PyonPix.Ui.Windows;

public class BrowserWindow : BaseWindow
{
	private readonly ContextMenu ConfigContextMenu;

	private Vector2 PreviousSize;

	private PixService PixService => Services.Get<PixService>();

	private BrowserService BrowserService => Services.Get<BrowserService>();

	private PixInputService PixInputService => Services.Get<PixInputService>();

	private SyncService SyncService => Services.Get<SyncService>();

	protected override WindowState State
	{
		get
		{
			if (!Config.UI.Browser.Collapsed)
			{
				return WindowState.Expanded;
			}
			return WindowState.Collapsed;
		}
	}

	protected override Vector2 ExpandedSize => Config.UI.Browser.ExpandedSize;

	protected override Vector2 ExpandedMinSize => new Vector2(300f, 150f);

	protected override Vector2 ExpandedMaxSize => UiUtil.GameResolution;

	private float ToolBarHeight => 26f * ImGuiHelpers.GlobalScale;

	private float TitleToolbarHeight => base.CollapsedHeight + ToolBarHeight;

	private float ButtonSize => ToolBarHeight;

	private float Spacing => 2f * ImGuiHelpers.GlobalScale;

	public override void OnOpen()
	{
		((Window)this).OnOpen();
		BrowserService.IsHidden = base.IsHidden;
		Config.UI.Browser.IsOpen = true;
		Config.Save();
	}

	public override void OnClose()
	{
		((Window)this).OnClose();
		BrowserService.IsHidden = true;
		Config.UI.Browser.IsOpen = false;
		Config.Save();
	}

	protected override void OnCollapsed(Vector2 windowSize)
	{
		Config.UI.Browser.ExpandedSize = windowSize;
		Config.Save();
	}

	protected override void SetState(WindowState newState)
	{
		if (State != newState)
		{
			Config.UI.Browser.Collapsed = newState == WindowState.Collapsed;
			Config.Save();
			BrowserService.IsHidden = base.IsHidden;
		}
	}

	protected override void OnConfigClicked()
	{
		((Window)Windows.Get<ConfigWindow>()).Toggle();
	}

	protected override void OnCloseClicked()
	{
		((Window)this).IsOpen = false;
		OnCloseUserInteraction();
	}

	public void OnCloseUserInteraction()
	{
		if (BrowserService.State != BrowserState.Stopped)
		{
			_ = BrowserService.State;
			_ = 3;
		}
	}

	public BrowserWindow(Configuration config, IServiceContext services, IWindowContext windows)
		: base("PyonPix Browser###PyonPixBrowser", config, services, windows, (ImGuiWindowFlags)0)
	{
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(960f, 540f) * ImGuiHelpers.GlobalScale;
		int num = 3;
		List<ContextMenuItem> list = new List<ContextMenuItem>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<ContextMenuItem> span = CollectionsMarshal.AsSpan(list);
		span[0] = new ContextMenuButton("Pix Config", delegate
		{
			Windows.Get<PixConfigWindow>().Toggle(PixService.GetPix(BrowserService.FocusedTab?.PixId));
		}, closeOnClick: true, (FontAwesomeIcon)61459);
		span[1] = new ContextMenuButton("Extension Manager", delegate
		{
			((Window)Windows.Get<ExtensionsWindow>()).Toggle();
		}, closeOnClick: true, (FontAwesomeIcon)61742);
		span[2] = new ContextMenuButton("Data Manager", delegate
		{
			((Window)Windows.Get<DataWindow>()).Toggle();
		}, closeOnClick: true, (FontAwesomeIcon)61563);
		ConfigContextMenu = new ContextMenu("browserConfig", list, 140f, 26f);
		PixService.PixSpawned += delegate(IPix p, bool isUserAction)
		{
			if (!isUserAction)
			{
				SpawnBehaviour territorySpawnBehaviour = Config.Global.Browser.TerritorySpawnBehaviour;
				if (territorySpawnBehaviour.HasFlag(SpawnBehaviour.Expand))
				{
					((Window)this).IsOpen = true;
					SetState(WindowState.Expanded);
				}
				else if (territorySpawnBehaviour.HasFlag(SpawnBehaviour.Show))
				{
					((Window)this).IsOpen = true;
				}
			}
		};
		PixService.PixDespawned += delegate(IPix p, bool isUserAction)
		{
			if (!isUserAction)
			{
				DespawnBehaviour territoryDespawnBehaviour = Config.Global.Browser.TerritoryDespawnBehaviour;
				if (territoryDespawnBehaviour.HasFlag(DespawnBehaviour.Collapse))
				{
					((Window)this).IsOpen = true;
					SetState(WindowState.Collapsed);
				}
				else if (territoryDespawnBehaviour.HasFlag(DespawnBehaviour.Hide))
				{
					((Window)this).IsOpen = false;
				}
			}
		};
		BrowserService.OnStatusUpdate += delegate(StatusUpdate e)
		{
			if (e.StatusType == StatusType.None)
			{
				StatusBar.Hide();
			}
			else
			{
				StatusBar.Show(e.Status, e.DisplayTime, e.Overlay);
			}
		};
	}

	public override void Draw()
	{
		base.Draw();
	}

	protected override void DrawTitleBarText(float leftCursor, float rightCursor)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		if (!((Window)this).IsOpen)
		{
			return;
		}
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		float num = leftCursor + 4f * ImGuiHelpers.GlobalScale;
		float num2 = rightCursor - 4f * ImGuiHelpers.GlobalScale;
		Math.Max(0f, num2 - num);
		ImGui.SetCursorScreenPos(new Vector2(num, base.HeaderMin.Y));
		float num3 = 4f * ImGuiHelpers.GlobalScale;
		float num4 = 10f * ImGuiHelpers.GlobalScale;
		float x = 140f * ImGuiHelpers.GlobalScale;
		List<Tab> list = BrowserService.Tabs.Values.ToList();
		float num5 = 0f;
		int num6 = 0;
		for (int i = 0; i < list.Count; i++)
		{
			Tab tab = list[i];
			bool flag = BrowserService.FocusedTab == tab;
			float num7 = ((tab.FavIcon == null) ? 0f : (16f * ImGuiHelpers.GlobalScale));
			string text = StringExtensions.Truncate(tab.GetTitle(), 10, string.Empty);
			float x2 = UiUtil.CalcTextSize(text, 14f).X;
			float y = num7 + 2f * num3 + x2 + 16f * ImGuiHelpers.GlobalScale;
			y = MathF.Min(x, y);
			if (i > 0)
			{
				ImGui.SameLine((float)((num5 == 0f) ? 0 : 0), 0f);
			}
			ImGui.PushID(ImU8String.op_Implicit(tab.PixId));
			Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
			Vector2 vector = cursorScreenPos + new Vector2(y, base.TitleBarFrameHeight);
			if (tab.FavIcon != null)
			{
				ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X + num3, cursorScreenPos.Y + (base.TitleBarFrameHeight - num7) * 0.5f));
				ImGui.Image(tab.FavIcon.Handle, new Vector2(num7, num7), (!flag) ? new Vector4(0.7f, 0.7f, 0.7f, 0.7f) : Vector4.One);
			}
			ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X + num3 + num7 + 6f * ImGuiHelpers.GlobalScale, cursorScreenPos.Y + (base.TitleBarFrameHeight - ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f).Y) * 0.5f));
			ImU8String text2 = ImU8String.op_Implicit(text);
			Vector3? colorA = (flag ? UIShared.BrowserTabFocused.AsVector3() : UIShared.BrowserTabInactive.AsVector3());
			ImGuiEx.StyledText(text2, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
			ImGui.SetCursorScreenPos(cursorScreenPos);
			ImGui.InvisibleButton(ImU8String.op_Implicit("##tabHit"), new Vector2(y, base.TitleBarFrameHeight), (ImGuiButtonFlags)0);
			if (!flag)
			{
				if (ImGui.IsItemHovered())
				{
					((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(cursorScreenPos, new Vector2(vector.X, vector.Y), ImGui.GetColorU32(new Vector4(0.4f, 0.4f, 0.4f, 0.2f)), 2f);
				}
				if (ImGui.IsItemClicked())
				{
					BrowserService.FocusTab(tab.PixId);
				}
			}
			else
			{
				((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(cursorScreenPos, new Vector2(vector.X, vector.Y), ImGui.GetColorU32(new Vector4(0.4f, 0.4f, 0.4f, 0.1f)), 2f);
			}
			ImGui.PopID();
			num5 += y + num4;
			num6++;
		}
		if (num6 == 0)
		{
			base.DrawTitleBarText(leftCursor, rightCursor);
		}
	}

	protected override void DrawContent()
	{
		if (!((Window)this).IsOpen)
		{
			return;
		}
		DrawToolbar();
		Vector2 vector = (base.IsCollapsed ? Config.UI.Browser.ExpandedSize : ImGui.GetWindowSize());
		bool sizeChanged = vector != PreviousSize;
		bool mouseDragging = ImGui.IsMouseDragging((ImGuiMouseButton)0);
		BrowserService.DetermineResizeState(sizeChanged, mouseDragging);
		PreviousSize = vector;
		Vector2 vector2 = (base.IsCollapsed ? (vector - new Vector2(0f, TitleToolbarHeight)) : ImGui.GetContentRegionAvail());
		if (BrowserService.Draw(ImGui.GetCursorScreenPos(), base.IsCollapsed ? vector2 : ImGui.GetContentRegionAvail()))
		{
			if (!base.IsHidden)
			{
				PixInputService.HandleImGuiPresentationMouseInput();
			}
		}
		else
		{
			((Window)this).WindowName = "PyonPix###PyonPixBrowser";
			PixInputService.ClearImGuiPresentationFocus();
		}
	}

	private void DrawToolbar()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		float globalScale = ImGuiHelpers.GlobalScale;
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
		ImGui.BeginChild(ImU8String.op_Implicit("##toolbar"), new Vector2(0f, ToolBarHeight + 1f), false, (ImGuiWindowFlags)8);
		Vector2 vector = new Vector2(cursorScreenPos.X, cursorScreenPos.Y + 1f);
		Vector2 vector2 = cursorScreenPos + new Vector2(contentRegionAvail.X, ToolBarHeight + 1f);
		((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(vector, vector2, ImGui.GetColorU32(UIShared.TitleBarBg));
		Vector2 vector3 = cursorScreenPos;
		((ImDrawListPtr)(ref windowDrawList)).AddLine(vector3, new Vector2(vector3.X + contentRegionAvail.X, vector3.Y), ImGui.GetColorU32(UIShared.ToolBarSeparator), MathF.Max(1f, 1f * globalScale));
		Vector2 vector4 = new Vector2(vector3.X, vector3.Y + ToolBarHeight + 1f);
		((ImDrawListPtr)(ref windowDrawList)).AddLine(vector4, new Vector2(vector4.X + contentRegionAvail.X, vector4.Y), ImGui.GetColorU32(UIShared.ToolBarSeparator), MathF.Max(1f, 1f * globalScale));
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X, vector3.Y + 1f));
		if (ImGuiEx.IconButton((FontAwesomeIcon)61700, "##navBack", !BrowserService.CanGoBack, "Back", "Right-click for history", ButtonSize))
		{
			BrowserService.NavBack();
		}
		if (ImGui.IsItemHovered() && ImGui.IsMouseReleased((ImGuiMouseButton)1))
		{
			Tab? focusedTab = BrowserService.FocusedTab;
			if (focusedTab != null && focusedTab.History.Count > 0)
			{
				ImGui.OpenPopup(ImU8String.op_Implicit("##historyContext"), (ImGuiPopupFlags)0);
			}
		}
		ImGui.SameLine(0f, Spacing);
		if (ImGuiEx.IconButton((FontAwesomeIcon)61701, "##navForward", !BrowserService.CanGoForward, "Forward", "Right-click for history", ButtonSize))
		{
			BrowserService.NavForward();
		}
		if (ImGui.IsItemHovered() && ImGui.IsMouseReleased((ImGuiMouseButton)1))
		{
			Tab? focusedTab2 = BrowserService.FocusedTab;
			if (focusedTab2 != null && focusedTab2.History.Count > 0)
			{
				ImGui.OpenPopup(ImU8String.op_Implicit("##historyContext"), (ImGuiPopupFlags)0);
			}
		}
		DrawHistoryContextMenu(vector, vector2, delegate(int selectedIndex)
		{
			BrowserService.NavHistory(selectedIndex);
		});
		ImGui.SameLine(0f, Spacing);
		if (BrowserService.CanCancel)
		{
			if (ImGuiEx.IconButton((FontAwesomeIcon)61453, "##navCancel", disabled: false, "Abort", null, ButtonSize))
			{
				BrowserService.NavCancel();
			}
		}
		else if (ImGuiEx.IconButton((FontAwesomeIcon)61470, "##navReload", !BrowserService.CanReload, "Reload", null, ButtonSize))
		{
			BrowserService.NavReload();
		}
		ImGui.SameLine(0f, Spacing);
		if (ImGuiEx.IconButton((FontAwesomeIcon)61461, "##navHome", !BrowserService.CanNavigate, "Home", null, ButtonSize))
		{
			BrowserService.NavHome();
		}
		int num;
		int num2;
		if (SyncService.IsConnectedAuth)
		{
			num = ((PixService.GetPix(BrowserService.FocusedTab?.PixId) is SyncedPix) ? 1 : 0);
			if (num != 0)
			{
				num2 = 4;
				goto IL_039c;
			}
		}
		else
		{
			num = 0;
		}
		num2 = 3;
		goto IL_039c;
		IL_039c:
		int num3 = num2;
		ImGui.SameLine(0f, Spacing);
		ImGuiEx.StyledInput(width: ImGui.GetContentRegionAvail().X - ToolBarHeight * (float)num3 - Spacing * (float)num3, label: ImU8String.op_Implicit("##uriInput"), text: ref BrowserService.PresentationUri, hint: "Search Google or enter a URI", disabled: !BrowserService.CanNavigate, maxLength: 65535, flags: (ImGuiInputTextFlags)16, tooltip: null, tooltipSub: null, onEnter: delegate
		{
			BrowserService.Navigate(BrowserService.PresentationUri);
		}, buttonIcon: (FontAwesomeIcon)0, onButtonClick: null, labelIcon: (FontAwesomeIcon)0);
		ImGui.SameLine(0f, Spacing);
		if (ImGuiEx.IconButton((FontAwesomeIcon)61537, "##navSubmit", !BrowserService.CanNavigate, "Submit", null, ButtonSize))
		{
			BrowserService.Navigate(BrowserService.PresentationUri);
		}
		if (num != 0)
		{
			ImGui.SameLine(0f, Spacing);
			if (ImGuiEx.IconButton((FontAwesomeIcon)61473, "##sync", !BrowserService.CanNavigate, "Resync", null, ButtonSize))
			{
				SyncService.SyncMediaState(BrowserService.FocusedTab.PixId, null);
			}
		}
		ImGui.SameLine(0f, Spacing);
		if (ImGuiEx.IconButton((FontAwesomeIcon)61541, "##theatreMode", !BrowserService.CanNavigate, "Toggle Theatre Mode", null, ButtonSize))
		{
			BrowserService.ToggleTheatreMode();
		}
		ImGui.SameLine(0f, Spacing);
		if (ImGuiEx.IconButton((FontAwesomeIcon)61762, "##configMenu", disabled: false, "Settings", null, ButtonSize))
		{
			ConfigContextMenu.Open();
		}
		ConfigContextMenu.Draw(new Vector2(vector2.X - 140f * ImGuiHelpers.GlobalScale, vector2.Y + 1f * ImGuiHelpers.GlobalScale));
		ImGui.EndChild();
	}

	private void DrawHistoryContextMenu(Vector2 anchorMin, Vector2 anchorMax, Action<int>? onItemSelected)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		if (BrowserService.FocusedTab == null || !ImGui.IsPopupOpen(ImU8String.op_Implicit("##historyContext"), (ImGuiPopupFlags)0))
		{
			return;
		}
		float globalScale = ImGuiHelpers.GlobalScale;
		float x = 200f * globalScale;
		float num = 4f * globalScale;
		float frameHeight = ImGui.GetFrameHeight();
		int num2 = 10;
		int count = BrowserService.FocusedTab.History.Count;
		if (count == 0)
		{
			return;
		}
		Vector2 vector = new Vector2(anchorMin.X, anchorMax.Y + 1f * globalScale);
		int currentNavigationIndex = BrowserService.FocusedTab.CurrentNavigationIndex;
		int num3 = num2;
		int num4 = currentNavigationIndex - num3 / 2;
		int num5 = num4 + num3;
		if (num4 < 0)
		{
			num4 = 0;
			num5 = Math.Min(num3, count);
		}
		else if (num5 > count)
		{
			num5 = count;
			num4 = Math.Max(0, num5 - num3);
		}
		int num6 = Math.Min(count, num5 - num4);
		if (num6 <= 0)
		{
			return;
		}
		float y = frameHeight * (float)num6;
		ImGui.SetNextWindowPos(vector, (ImGuiCond)8, new Vector2(0f, 0f));
		ImGui.SetNextWindowSize(new Vector2(x, y), (ImGuiCond)8);
		ImGui.PushStyleColor((ImGuiCol)4, Vector4.Zero);
		ImGui.PushStyleColor((ImGuiCol)3, Vector4.Zero);
		if (ImGui.BeginPopup(ImU8String.op_Implicit("##historyContext"), (ImGuiWindowFlags)263))
		{
			ImGui.BeginChild(ImU8String.op_Implicit("##historyList"), new Vector2(x, y), false, (ImGuiWindowFlags)8);
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(vector, vector + new Vector2(x, y), ImGui.GetColorU32(UIShared.ContextMenuBg), UIShared.InputRounding);
			((ImDrawListPtr)(ref windowDrawList)).AddRect(vector, vector + new Vector2(x, y), ImGui.GetColorU32(UIShared.ContextMenuBorder), UIShared.InputRounding);
			for (int i = 0; i < num6; i++)
			{
				int num7 = num5 - 1 - i;
				bool flag = num7 == currentNavigationIndex;
				NavigationItem navigationItem = BrowserService.FocusedTab.History[num7];
				Vector2 vector2 = vector + new Vector2(0f, (float)i * frameHeight);
				Vector2 vector3 = vector2 + new Vector2(x, frameHeight);
				bool flag2 = UiUtil.IsRectHovered(vector2, vector3);
				if (flag2 || flag)
				{
					((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(vector2, vector3, ImGui.GetColorU32(flag2 ? UIShared.ContextItemBgHovered : UIShared.ContextItemBgActive), UIShared.InputRounding);
				}
				if (UiUtil.IsRectClicked(vector2, vector3, (ImGuiMouseButton)0))
				{
					onItemSelected?.Invoke(num7);
					ImGui.CloseCurrentPopup();
				}
				using (UIShared.SubFont.Push())
				{
					Vector4 vector4 = (flag2 ? UIShared.ContextItemTextHovered : (flag ? UIShared.ContextItemTextActive : UIShared.ContextItemTextNormal));
					((ImDrawListPtr)(ref windowDrawList)).AddText(new Vector2(vector2.X + num, vector2.Y + (vector3.Y - vector2.Y - ImGui.GetFontSize()) * 0.5f), ImGui.GetColorU32(vector4), ImU8String.op_Implicit(navigationItem.GetDisplayTitle()));
				}
				if (string.IsNullOrWhiteSpace(navigationItem.Title))
				{
					Tooltip.Show(navigationItem.Uri.TruncateMiddle(60), null, vector2, vector3);
				}
				else
				{
					Tooltip.Show(navigationItem.GetDisplayTitle(), navigationItem.Uri.TruncateMiddle(60), vector2, vector3);
				}
			}
			ImGui.EndChild();
			ImGui.EndPopup();
		}
		ImGui.PopStyleColor(2);
	}
}
