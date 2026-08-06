using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using PyonPix.Extensions;
using PyonPix.Shared.Sync.Dto.Client;
using PyonPix.Utility;

namespace PyonPix.Ui.Components;

public class ContextMenu
{
	public readonly string Id;

	private readonly List<ContextMenuItem>? Items;

	private readonly List<ContextMenuTab>? Tabs;

	public float Width;

	public float ItemHeight;

	public float SeperatorHeight = 12f;

	public int MaxItemsDisplayed;

	public Action<int>? ActiveTabUpdated;

	public float TabHeight = 26f;

	private int ActiveTabIndex;

	private Vector2 MousePos;

	public float SubTextHeight => ItemHeight * 0.5f;

	public ContextMenu(string id, List<ContextMenuItem> items, float width = 140f, float itemHeight = 0f, int maxItemsDisplayed = 12)
	{
		Id = "##ctx_" + id;
		Items = items;
		Tabs = null;
		Width = width;
		ItemHeight = itemHeight;
		MaxItemsDisplayed = maxItemsDisplayed;
	}

	public ContextMenu(string id, List<ContextMenuTab> tabs, int activeTabIndex = 0, float width = 140f, float itemHeight = 0f, int maxItemsDisplayed = 12, Action<int>? activeTabUpdated = null)
	{
		Id = "##ctx_" + id;
		Items = null;
		Tabs = tabs;
		ActiveTabIndex = activeTabIndex;
		Width = width;
		ItemHeight = itemHeight;
		MaxItemsDisplayed = maxItemsDisplayed;
		ActiveTabUpdated = activeTabUpdated;
	}

	public void Open()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		MousePos = ImGui.GetMousePos();
		ImGui.OpenPopup(ImU8String.op_Implicit(Id), (ImGuiPopupFlags)0);
	}

	public void Open(string id)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if ("##ctx_" + id == Id)
		{
			MousePos = ImGui.GetMousePos();
			ImGui.OpenPopup(ImU8String.op_Implicit(Id), (ImGuiPopupFlags)0);
		}
	}

	public bool IsOpen()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ImGui.IsPopupOpen(ImU8String.op_Implicit(Id), (ImGuiPopupFlags)0);
	}

	public bool IsOpen(string id)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if ("##ctx_" + id == Id)
		{
			return ImGui.IsPopupOpen(ImU8String.op_Implicit(Id), (ImGuiPopupFlags)0);
		}
		return false;
	}

	public void Draw(string id, Vector2? anchorPos = null)
	{
		if (!("##ctx_" + id != Id))
		{
			Draw(anchorPos ?? MousePos);
		}
	}

	public void Draw(Vector2? anchorPos = null)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_055d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0506: Unknown result type (might be due to invalid IL or missing references)
		if (!ImGui.IsPopupOpen(ImU8String.op_Implicit(Id), (ImGuiPopupFlags)0) || ((Items == null || Items.Count == 0) && (Tabs == null || Tabs.Count == 0)))
		{
			return;
		}
		float globalScale = ImGuiHelpers.GlobalScale;
		float num = Width * globalScale;
		float padding = 6f * globalScale;
		bool flag = Tabs != null && Tabs.Count > 0;
		float num2 = (flag ? (TabHeight * globalScale) : 0f);
		List<ContextMenuItem> list = (flag ? Tabs[Math.Clamp(ActiveTabIndex, 0, Tabs.Count - 1)].Items.Where((ContextMenuItem x) => x.IsVisible).ToList() : Items.Where((ContextMenuItem x) => x.IsVisible).ToList());
		int num3 = Math.Min(list.Count(delegate(ContextMenuItem x)
		{
			bool flag6 = ((x is ContextMenuSeparator || x is ContextMenuHeader || x is ContextMenuSubText) ? true : false);
			return !flag6;
		}), Math.Max(1, MaxItemsDisplayed));
		float num4 = (GetNonContentHeight(list) + (float)num3 * ItemHeight) * globalScale;
		float y = num2 + num4;
		if (!anchorPos.HasValue)
		{
			anchorPos = MousePos;
		}
		ImGui.SetNextWindowPos(anchorPos.Value, (ImGuiCond)8);
		ImGui.SetNextWindowSize(new Vector2(num, y), (ImGuiCond)8);
		ImGui.PushStyleVar((ImGuiStyleVar)13, Vector2.Zero);
		ImGui.PushStyleVar((ImGuiStyleVar)8, UIShared.InputRounding);
		ImGui.PushStyleColor((ImGuiCol)4, Vector4.Zero);
		ImGui.PushStyleColor((ImGuiCol)3, Vector4.Zero);
		if (ImGui.BeginPopup(ImU8String.op_Implicit(Id), (ImGuiWindowFlags)263))
		{
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			Vector2 value = anchorPos.Value;
			Vector2 vector = value + new Vector2(num, y);
			((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(value, vector, ImGui.GetColorU32(UIShared.ContextMenuBg), UIShared.InputRounding);
			((ImDrawListPtr)(ref windowDrawList)).AddRect(value, vector, ImGui.GetColorU32(UIShared.ContextMenuBorder), UIShared.InputRounding);
			if (flag)
			{
				DrawTabBar(windowDrawList, num, num2, padding);
			}
			ImU8String val = default(ImU8String);
			((ImU8String)(ref val))._002Ector(7, 1);
			((ImU8String)(ref val)).AppendFormatted<string>(Id);
			((ImU8String)(ref val)).AppendLiteral("content");
			ImGui.BeginChild(val, new Vector2(num, num4), false, (ImGuiWindowFlags)0);
			Vector2 windowPos = ImGui.GetWindowPos();
			Vector2 vector2 = windowPos + ImGui.GetWindowSize();
			((ImDrawListPtr)(ref windowDrawList)).PushClipRect(windowPos, vector2, true);
			ImU8String val2 = default(ImU8String);
			for (int num5 = 0; num5 < list.Count; num5++)
			{
				ContextMenuItem contextMenuItem = list[num5];
				float y2 = ((contextMenuItem is ContextMenuSeparator) ? (SeperatorHeight * globalScale) : ((contextMenuItem is ContextMenuSubText) ? (SubTextHeight * globalScale) : ((ItemHeight > 0f) ? (ItemHeight * globalScale) : ImGui.GetFrameHeight())));
				((ImU8String)(ref val2))._002Ector(5, 2);
				((ImU8String)(ref val2)).AppendFormatted<string>(Id);
				((ImU8String)(ref val2)).AppendFormatted<int>(num5);
				((ImU8String)(ref val2)).AppendLiteral("dummy");
				ImGui.InvisibleButton(val2, new Vector2(num, y2), (ImGuiButtonFlags)0);
				if (!(contextMenuItem is ContextMenuSeparator))
				{
					if (!(contextMenuItem is ContextMenuHeader contextMenuHeader))
					{
						if (!(contextMenuItem is ContextMenuSubText contextMenuSubText))
						{
							ContextMenuButton contextMenuButton = contextMenuItem as ContextMenuButton;
							if (contextMenuButton == null)
							{
								ContextMenuCheckbox contextMenuCheckbox = contextMenuItem as ContextMenuCheckbox;
								if (contextMenuCheckbox == null)
								{
									if (contextMenuItem is ContextMenuSubmenu contextMenuSubmenu)
									{
										bool flag2 = contextMenuSubmenu.IsDisabled?.Invoke() ?? false;
										bool num6 = ImGui.IsItemHovered();
										DrawLabelRow(windowDrawList, contextMenuSubmenu.Text(), contextMenuSubmenu.Icon, padding, AnyInteractiveItemHasIcon(list), UIShared.SubFont, UIShared.ItemSubText);
										if (num6 && !flag2)
										{
											Vector2 value2 = new Vector2(ImGui.GetItemRectMax().X, ImGui.GetItemRectMin().Y);
											new ContextMenu($"{Id}_sub_{num5}", contextMenuSubmenu.SubItems, Width, ItemHeight, MaxItemsDisplayed).Draw(value2);
										}
									}
									continue;
								}
								bool flag3 = contextMenuCheckbox.IsDisabled?.Invoke() ?? false;
								bool flag4 = !flag3 && contextMenuCheckbox.GetValue();
								FontAwesomeIcon value3 = (FontAwesomeIcon)(flag4 ? 61770 : 61640);
								DrawInteractiveRow(windowDrawList, contextMenuCheckbox.Text(), value3, padding, globalAnyIcon: true, flag4, flag3, ContextMenuTint.Icon, contextMenuCheckbox.DisabledTint, contextMenuCheckbox.Tooltip, delegate
								{
									contextMenuCheckbox.SetValue(!contextMenuCheckbox.GetValue());
									if (contextMenuCheckbox.CloseOnClick)
									{
										ImGui.CloseCurrentPopup();
									}
								});
								continue;
							}
							bool flag5 = contextMenuButton.IsDisabled?.Invoke() ?? false;
							bool active = !flag5 && (contextMenuButton.IsActive?.Invoke() ?? false);
							DrawInteractiveRow(windowDrawList, contextMenuButton.Text(), contextMenuButton.Icon, padding, AnyInteractiveItemHasIcon(list), active, flag5, contextMenuButton.ActiveTint, contextMenuButton.DisabledTint, contextMenuButton.Tooltip, delegate
							{
								contextMenuButton.OnClick?.Invoke();
								if (contextMenuButton.CloseOnClick)
								{
									ImGui.CloseCurrentPopup();
								}
							});
						}
						else
						{
							DrawLabelRow(windowDrawList, contextMenuSubText.Text(), contextMenuSubText.Icon, padding, AnyInteractiveItemHasIcon(list), UIShared.SubFont, UIShared.ItemSubText);
						}
					}
					else
					{
						DrawLabelRow(windowDrawList, contextMenuHeader.Text(), contextMenuHeader.Icon, padding, AnyInteractiveItemHasIcon(list), UIShared.NormalFont, UIShared.WindowTitle);
					}
				}
				else
				{
					DrawSeparatorItem(windowDrawList, globalScale);
				}
			}
			((ImDrawListPtr)(ref windowDrawList)).PopClipRect();
			ImGui.EndChild();
			ImGui.EndPopup();
		}
		ImGui.PopStyleColor(2);
		ImGui.PopStyleVar(2);
	}

	private void DrawTabBar(ImDrawListPtr draw, float width, float tabHeight, float padding)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		int count = Tabs.Count;
		if (count <= 0)
		{
			return;
		}
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		Vector2 vector = cursorScreenPos + new Vector2(width, tabHeight);
		float num = width / (float)count;
		ImU8String val = default(ImU8String);
		for (int i = 0; i < count; i++)
		{
			ContextMenuTab contextMenuTab = Tabs[i];
			Vector2 vector2 = cursorScreenPos + new Vector2(num * (float)i, 0f);
			Vector2 vector3 = vector2 + new Vector2(num, tabHeight);
			ImGui.SetCursorScreenPos(vector2);
			((ImU8String)(ref val))._002Ector(6, 2);
			((ImU8String)(ref val)).AppendFormatted<string>(Id);
			((ImU8String)(ref val)).AppendLiteral("##tab_");
			((ImU8String)(ref val)).AppendFormatted<int>(i);
			ImGui.InvisibleButton(val, new Vector2(num, tabHeight), (ImGuiButtonFlags)0);
			bool flag = ImGui.IsItemHovered();
			bool num2 = i == ActiveTabIndex;
			if (ImGui.IsItemClicked() && i != ActiveTabIndex)
			{
				ActiveTabIndex = i;
				ActiveTabUpdated?.Invoke(ActiveTabIndex);
			}
			Vector4 vector4 = (num2 ? UIShared.ContextItemBgActive : (flag ? UIShared.ContextItemBgHovered : Vector4.Zero));
			if (vector4 != Vector4.Zero)
			{
				ImDrawFlags val2 = (ImDrawFlags)256;
				if (i == 0)
				{
					val2 = (ImDrawFlags)(val2 | 0x10);
				}
				if (i == count - 1)
				{
					val2 = (ImDrawFlags)(val2 | 0x20);
				}
				((ImDrawListPtr)(ref draw)).AddRectFilled(vector2, vector3, ImGui.GetColorU32(vector4), UIShared.InputRounding, val2);
			}
			Vector4 vector5 = (num2 ? UIShared.ContextItemTextHovered : UIShared.ContextItemTextNormal);
			using (UIShared.SubFont.Push())
			{
				string text = contextMenuTab.Text();
				Vector2 vector6 = ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f);
				Vector2 vector7 = ((contextMenuTab.Icon.HasValue && (int)contextMenuTab.Icon.Value != 0) ? ImGui.CalcTextSize(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(contextMenuTab.Icon.Value)), false, -1f) : Vector2.Zero);
				float num3 = vector6.X + ((vector7.X > 0f) ? (vector7.X + padding * 0.5f) : 0f);
				float num4 = vector2.X + (num - num3) * 0.5f;
				float y = vector2.Y + (tabHeight - vector6.Y) * 0.5f;
				if (contextMenuTab.Icon.HasValue && (int)contextMenuTab.Icon.Value != 0)
				{
					using (UIShared.NormalIconFont.Push())
					{
						string text2 = FontAwesomeExtensions.ToIconString(contextMenuTab.Icon.Value);
						((ImDrawListPtr)(ref draw)).AddText(new Vector2(num4, vector2.Y + (tabHeight - vector7.Y) * 0.5f), ImGui.GetColorU32(vector5), ImU8String.op_Implicit(text2));
					}
					num4 += vector7.X + padding * 0.5f;
				}
				((ImDrawListPtr)(ref draw)).AddText(new Vector2(num4, y), ImGui.GetColorU32(vector5), ImU8String.op_Implicit(text));
			}
		}
		float globalScale = ImGuiHelpers.GlobalScale;
		((ImDrawListPtr)(ref draw)).AddLine(new Vector2(cursorScreenPos.X, vector.Y), new Vector2(cursorScreenPos.X + width, vector.Y), ImGui.GetColorU32(UIShared.Separator), 1f * globalScale);
	}

	private static void DrawSeparatorItem(ImDrawListPtr draw, float scale)
	{
		Vector2 itemRectMin = ImGui.GetItemRectMin();
		Vector2 itemRectMax = ImGui.GetItemRectMax();
		float num = itemRectMax.X - itemRectMin.X;
		float num2 = itemRectMax.Y - itemRectMin.Y;
		float y = itemRectMin.Y + num2 * 0.5f;
		float num3 = 6f * scale;
		((ImDrawListPtr)(ref draw)).AddLine(new Vector2(itemRectMin.X + num3, y), new Vector2(itemRectMin.X + num - num3, y), ImGui.GetColorU32(UIShared.Separator), 1f * scale);
	}

	private static void DrawLabelRow(ImDrawListPtr draw, string text, FontAwesomeIcon? icon, float padding, bool globalAnyIcon, IFontHandle fontHandle, Vector4 textCol)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		Vector2 itemRectMin = ImGui.GetItemRectMin();
		Vector2 itemRectMax = ImGui.GetItemRectMax();
		float x = ((icon.HasValue && icon != (FontAwesomeIcon?)0) ? (itemRectMin.X + UiUtil.CalcTextSize(UIShared.SubIconFont, FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61459)).X + padding * 2f) : (itemRectMin.X + padding));
		if (icon.HasValue && icon != (FontAwesomeIcon?)0)
		{
			using (UIShared.SubIconFont.Push())
			{
				string text2 = FontAwesomeExtensions.ToIconString(icon.Value);
				Vector2 vector = ImGui.CalcTextSize(ImU8String.op_Implicit(text2), false, -1f);
				float x2 = itemRectMin.X + padding;
				float y = itemRectMin.Y + (itemRectMax.Y - itemRectMin.Y - vector.Y) * 0.5f;
				((ImDrawListPtr)(ref draw)).AddText(new Vector2(x2, y), ImGui.GetColorU32(textCol), ImU8String.op_Implicit(text2));
			}
		}
		using (fontHandle.Push())
		{
			float y2 = itemRectMin.Y + (itemRectMax.Y - itemRectMin.Y - ImGui.GetFontSize()) * 0.5f;
			ImGuiEx.StyledText(ImU8String.op_Implicit(text), colorA: textCol.AsVector3(), targetDrawList: draw, screenOffset: new Vector2(x, y2), fontSize: null, opacity: 0.8f, bgOpacity: 0f, bgRounding: 4f, glowStrength: 0.2f, animationType: AnimationType.Static, colorB: null, glowA: null, glowB: null, bgColor: null, xPadding: null, yPadding: null, width: null, wrapWidth: float.MaxValue);
		}
	}

	private static void DrawInteractiveRow(ImDrawListPtr draw, string text, FontAwesomeIcon? icon, float padding, bool globalAnyIcon, bool active, bool disabled, ContextMenuTint activeTint, ContextMenuTint disabledTint, Func<(string, string?)?>? tooltipFunc, Action onClick)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		Vector2 itemRectMin = ImGui.GetItemRectMin();
		Vector2 itemRectMax = ImGui.GetItemRectMax();
		bool flag = ImGui.IsItemHovered();
		int num;
		if (flag)
		{
			num = ((!disabled) ? 1 : 0);
			if (num != 0)
			{
				((ImDrawListPtr)(ref draw)).AddRectFilled(itemRectMin, itemRectMax, ImGui.GetColorU32(UIShared.ContextItemBgHovered), UIShared.InputRounding);
				goto IL_0056;
			}
		}
		else
		{
			num = 0;
		}
		if (active)
		{
			((ImDrawListPtr)(ref draw)).AddRectFilled(itemRectMin, itemRectMax, ImGui.GetColorU32(UIShared.ContextItemBgActive), UIShared.InputRounding);
		}
		goto IL_0056;
		IL_0056:
		Vector4 value = ResolveColor((byte)num != 0, active, disabled, activeTint, disabledTint, ContextMenuTint.Text);
		Vector4 vector = ResolveColor((byte)num != 0, active, disabled, activeTint, disabledTint, ContextMenuTint.Icon);
		float x = ((icon.HasValue && icon != (FontAwesomeIcon?)0) ? (itemRectMin.X + UiUtil.CalcTextSize(UIShared.SubIconFont, FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61459)).X + padding * 2f) : (itemRectMin.X + padding));
		if (icon.HasValue && icon != (FontAwesomeIcon?)0)
		{
			using (UIShared.SubIconFont.Push())
			{
				string text2 = FontAwesomeExtensions.ToIconString(icon.Value);
				Vector2 vector2 = ImGui.CalcTextSize(ImU8String.op_Implicit(text2), false, -1f);
				float x2 = itemRectMin.X + padding;
				float y = itemRectMin.Y + (itemRectMax.Y - itemRectMin.Y - vector2.Y) * 0.5f;
				((ImDrawListPtr)(ref draw)).AddText(new Vector2(x2, y), ImGui.GetColorU32(vector), ImU8String.op_Implicit(text2));
			}
		}
		using (UIShared.SubFont.Push())
		{
			float y2 = itemRectMin.Y + (itemRectMax.Y - itemRectMin.Y - ImGui.GetFontSize()) * 0.5f;
			ImGuiEx.StyledText(ImU8String.op_Implicit(text), colorA: value.AsVector3(), targetDrawList: draw, screenOffset: new Vector2(x, y2), fontSize: null, opacity: 0.8f, bgOpacity: 0f, bgRounding: 4f, glowStrength: 0f, animationType: AnimationType.Static, colorB: null, glowA: null, glowB: null, bgColor: null, xPadding: null, yPadding: null, width: null, wrapWidth: float.MaxValue);
		}
		if (!disabled && UiUtil.IsRectClicked(itemRectMin, itemRectMax, (ImGuiMouseButton)0))
		{
			onClick();
		}
		if (flag && tooltipFunc != null)
		{
			(string, string)? tuple = tooltipFunc();
			if (tuple.HasValue)
			{
				Tooltip.Show(tuple.Value.Item1, tuple.Value.Item2, itemRectMin, itemRectMax);
			}
		}
	}

	private float GetNonContentHeight(List<ContextMenuItem> items)
	{
		float num = 0f;
		foreach (ContextMenuItem item in items)
		{
			if (item is ContextMenuSeparator)
			{
				num += SeperatorHeight;
			}
			else if (item is ContextMenuHeader)
			{
				num += ItemHeight;
			}
			else if (item is ContextMenuSubText)
			{
				num += SubTextHeight;
			}
		}
		return num;
	}

	private static Vector4 ResolveColor(bool hovered, bool active, bool disabled, ContextMenuTint activeTint, ContextMenuTint disabledTint, ContextMenuTint channel)
	{
		if (disabled && disabledTint.HasFlag(channel))
		{
			Vector4 contextItemTextNormal = UIShared.ContextItemTextNormal;
			contextItemTextNormal.W = UIShared.ContextItemTextNormal.W * 0.4f;
			return contextItemTextNormal;
		}
		if (hovered)
		{
			return UIShared.ContextItemTextHovered;
		}
		if (active && activeTint.HasFlag(channel))
		{
			return UIShared.ContextItemTextActive;
		}
		return UIShared.ContextItemTextNormal;
	}

	private static bool AnyInteractiveItemHasIcon(List<ContextMenuItem> items)
	{
		foreach (ContextMenuItem item in items)
		{
			if (item is ContextMenuButton contextMenuButton && contextMenuButton.Icon.HasValue)
			{
				return true;
			}
			if (item is ContextMenuCheckbox)
			{
				return true;
			}
		}
		return false;
	}
}
