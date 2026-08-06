using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using PyonPix.Config;
using PyonPix.Extensions;
using PyonPix.Services;
using PyonPix.Services.Core;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Sync.Dto.Client;
using PyonPix.Structs.Data;
using PyonPix.Utility;

namespace PyonPix.Ui.Windows;

public class DataWindow : BaseWindow
{
	private enum Tab
	{
		Cache,
		Cookies
	}

	private Tab ActiveTab;

	private PixService PixService => Services.Get<PixService>();

	private DataService DataService => Services.Get<DataService>();

	protected override WindowState State
	{
		get
		{
			if (!Config.UI.Data.Collapsed)
			{
				return WindowState.Expanded;
			}
			return WindowState.Collapsed;
		}
	}

	protected override Vector2 ExpandedSize => Config.UI.Data.ExpandedSize;

	protected override Vector2 ExpandedMinSize => new Vector2(300f, 150f);

	protected override Vector2 ExpandedMaxSize => UiUtil.GameResolution;

	private float TabHeight => 28f * ImGuiHelpers.GlobalScale;

	private float RowHeight => 72f * ImGuiHelpers.GlobalScale;

	private float IconSize => 16f * ImGuiHelpers.GlobalScale;

	private float HorizontalPadding => 8f * ImGuiHelpers.GlobalScale;

	private float VerticalPadding => 8f * ImGuiHelpers.GlobalScale;

	private float Spacing => 6f * ImGuiHelpers.GlobalScale;

	public override void OnOpen()
	{
		((Window)this).OnOpen();
		DataService.RefreshCacheAsync();
		Config.UI.Data.IsOpen = true;
		Config.Save();
	}

	public override void OnClose()
	{
		((Window)this).OnClose();
		Config.UI.Data.IsOpen = false;
		Config.Save();
	}

	protected override void OnCollapsed(Vector2 windowSize)
	{
		Config.UI.Data.ExpandedSize = windowSize;
		Config.Save();
	}

	protected override void SetState(WindowState newState)
	{
		if (State != newState)
		{
			Config.UI.Data.Collapsed = newState == WindowState.Collapsed;
			Config.Save();
		}
	}

	protected override void OnConfigClicked()
	{
		((Window)Windows.Get<ConfigWindow>()).Toggle();
	}

	protected override void OnCloseClicked()
	{
		((Window)this).IsOpen = false;
	}

	public DataWindow(Configuration config, IServiceContext services, IWindowContext windows)
		: base("PyonPix Data Manager###PyonPixData", config, services, windows, (ImGuiWindowFlags)0)
	{
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(420f, 320f) * ImGuiHelpers.GlobalScale;
		DataService.OnUDFRemovalCompleted += delegate
		{
			DataService.RefreshCacheAsync();
		};
	}

	public override void Draw()
	{
		base.Draw();
	}

	protected override void DrawContent()
	{
		if (((Window)this).IsOpen)
		{
			DrawTabs();
			if (ActiveTab == Tab.Cache)
			{
				DrawCacheTab();
			}
			else
			{
				DrawCookiesTab();
			}
		}
	}

	private void DrawTabs()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		ImGui.GetWindowDrawList();
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		float x = (ImGui.GetContentRegionAvail().X - HorizontalPadding * 2f) / 2f;
		Vector2 min = cursorScreenPos + new Vector2(HorizontalPadding, 0f);
		Vector2 max = cursorScreenPos + new Vector2(x, TabHeight);
		if (DrawTab(min, max, "Cache", ActiveTab == Tab.Cache))
		{
			ActiveTab = Tab.Cache;
		}
		Vector2 vector = new Vector2(max.X + Spacing, min.Y);
		Vector2 max2 = vector + new Vector2(x, TabHeight);
		if (DrawTab(vector, max2, "Cookies", ActiveTab == Tab.Cookies))
		{
			ActiveTab = Tab.Cookies;
		}
		ImGui.SetCursorScreenPos(cursorScreenPos + new Vector2(0f, TabHeight + Spacing));
	}

	private bool DrawTab(Vector2 min, Vector2 max, string text, bool active)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		bool flag = UiUtil.IsRectHovered(min, max);
		bool flag2 = UiUtil.IsRectClicked(min, max, (ImGuiMouseButton)0);
		Vector4 vector = (active ? UIShared.TabBgActive : (flag2 ? UIShared.TabBgClicked : (flag ? UIShared.TabBgHovered : UIShared.TabBgNormal)));
		((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(min, max, ImGui.GetColorU32(vector), UIShared.TabRounding);
		Vector4 value = (active ? UIShared.TabTextActive : (flag2 ? UIShared.TabTextClicked : (flag ? UIShared.TabTextHovered : UIShared.TabTextNormal)));
		using (UIShared.NormalFont.Push())
		{
			Vector2 vector2 = ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f);
			ImGui.SetCursorScreenPos(new Vector2(min.X + (max.X - min.X - vector2.X) * 0.5f, min.Y + (max.Y - min.Y - vector2.Y) * 0.5f));
			ImU8String text2 = ImU8String.op_Implicit(text);
			Vector3? colorA = value.AsVector3();
			ImGuiEx.StyledText(text2, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
			return flag2;
		}
	}

	private void DrawCacheTab()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		float x = ImGui.GetContentRegionAvail().X;
		ImGui.SetCursorScreenPos(cursorScreenPos + new Vector2(HorizontalPadding, 0f));
		float num = 0f;
		using (UIShared.SubFont.Push())
		{
			string text = "Total Cache: " + GetTotalSizeString();
			num = ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f).X;
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			((ImDrawListPtr)(ref windowDrawList)).AddText(ImGui.GetFont(), ImGui.GetFontSize(), ImGui.GetCursorScreenPos(), ImGui.GetColorU32(UIShared.Muted), ImU8String.op_Implicit(text), 0f);
		}
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X + HorizontalPadding + num + Spacing, cursorScreenPos.Y));
		if (ImGuiEx.IconButton((FontAwesomeIcon)61473, "##refresh", disabled: false, "Refresh", null, IconSize))
		{
			DataService.RefreshCacheAsync();
		}
		ImGui.SetCursorScreenPos(cursorScreenPos + new Vector2(0f, IconSize + Spacing));
		ImGui.BeginChild(ImU8String.op_Implicit("##udfRows"), new Vector2(x, ImGui.GetContentRegionAvail().Y), false, (ImGuiWindowFlags)0);
		foreach (UDF item in DataService.GetUDFSnapshot())
		{
			DrawUDFRow(item);
		}
		ImGui.EndChild();
	}

	private void DrawUDFRow(UDF item)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0540: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b0: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushID(ImU8String.op_Implicit(item.PixId));
		IPix pix = PixService.GetPix(item.PixId);
		float x = ImGui.GetContentRegionAvail().X;
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		Vector2 vector = cursorScreenPos + new Vector2(x, RowHeight);
		_ = vector - cursorScreenPos;
		ImDrawListPtr windowDrawList;
		if (ImGui.IsWindowHovered((ImGuiHoveredFlags)3) && ImGui.IsMouseHoveringRect(cursorScreenPos, vector))
		{
			windowDrawList = ImGui.GetWindowDrawList();
			((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(cursorScreenPos, vector, ImGui.GetColorU32(UIShared.ItemBgHovered));
		}
		float num = HorizontalPadding + IconSize + HorizontalPadding;
		float num2 = HorizontalPadding + IconSize * 0.5f;
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X + num2, cursorScreenPos.Y + (RowHeight - IconSize) * 0.5f));
		bool value = item.PersistentCache;
		if (pix != null && ImGuiEx.Checkbox("##togglePersist", ref value, disabled: false, "Toggle Persistent Cache"))
		{
			DataService.SetPersistent(item.PixId, value);
		}
		float num3 = vector.X - HorizontalPadding - IconSize;
		Vector2 cursorScreenPos2 = new Vector2(num3, cursorScreenPos.Y + (RowHeight - IconSize) * 0.5f);
		ImGui.SetCursorScreenPos(cursorScreenPos2);
		bool flag = PixService.IsSpawned(pix);
		bool disabled = flag || item.IsRemoving;
		if (ImGuiEx.IconButton((FontAwesomeIcon)62189, "##remove", disabled, "Clear Cache", item.IsRemoving ? "Processing.." : (flag ? "Unable to clear cache while spawned." : null), IconSize))
		{
			DataService.RemoveUDF(item.PixId);
		}
		string text = ((item.SizeBytes >= 0) ? FormatBytes(item.SizeBytes) : "Calculating...");
		using (UIShared.SubFont.Push())
		{
			Vector2 vector2 = UiUtil.CalcTextSize(text, ImGui.GetFontSize(), globalScale: false);
			ImGui.SetCursorScreenPos(new Vector2(vector.X - HorizontalPadding - vector2.X, cursorScreenPos2.Y + IconSize + 4f));
			windowDrawList = ImGui.GetWindowDrawList();
			((ImDrawListPtr)(ref windowDrawList)).AddText(ImGui.GetFont(), ImGui.GetFontSize(), ImGui.GetCursorScreenPos(), ImGui.GetColorU32(UIShared.Muted), ImU8String.op_Implicit(text), 0f);
		}
		float x2 = cursorScreenPos.X + num + IconSize * 0.5f + HorizontalPadding;
		float x3 = num3 - Spacing;
		ImGui.PushClipRect(new Vector2(x2, cursorScreenPos.Y), new Vector2(x3, vector.Y), true);
		Vector2 cursorScreenPos3 = new Vector2(x2, cursorScreenPos.Y + VerticalPadding);
		ImGui.SetCursorScreenPos(cursorScreenPos3);
		if (!item.PixExists)
		{
			using (UIShared.NormalIconFont.Push())
			{
				string text2 = FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61553);
				float x4 = ImGui.CalcTextSize(ImU8String.op_Implicit(text2), false, -1f).X;
				ImU8String text3 = ImU8String.op_Implicit(text2);
				Vector3? colorA = UIShared.Error.AsVector3();
				ImGuiEx.StyledText(text3, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue, multiline: false, "Pix Not Found", "This data has no associated Pix, it can be safely removed.");
				cursorScreenPos3 = new Vector2(cursorScreenPos3.X + x4 + Spacing, cursorScreenPos3.Y);
			}
		}
		using (UIShared.NormalFont.Push())
		{
			ImGui.SetCursorScreenPos(cursorScreenPos3);
			ImU8String text4 = ImU8String.op_Implicit(item.PixId);
			Vector3? colorA = (item.PixExists ? UIShared.ItemHeader.AsVector3() : UIShared.Error.AsVector3());
			ImGuiEx.StyledText(text4, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
		}
		if (!string.IsNullOrWhiteSpace(item.PixName))
		{
			Vector2 cursorScreenPos4 = new Vector2(cursorScreenPos3.X, cursorScreenPos3.Y + ImGui.GetFontSize() + Spacing * 0.6f);
			using (UIShared.SubFont.Push())
			{
				ImGui.SetCursorScreenPos(cursorScreenPos4);
				ImU8String text5 = ImU8String.op_Implicit(item.PixName);
				Vector3? colorA = UIShared.Dimmed.AsVector3();
				ImGuiEx.StyledText(text5, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
			}
		}
		Vector2 cursorScreenPos5 = new Vector2(cursorScreenPos3.X, cursorScreenPos3.Y + ImGui.GetFontSize() * 2f + Spacing * 1.2f);
		using (UIShared.SubFont.Push())
		{
			string text6 = (item.LastWriteUtc.HasValue ? item.LastWriteUtc.Value.ToLocalTime().ToString("g", CultureInfo.CurrentCulture) : "Unknown");
			string text7 = (item.IsRemoving ? " (Removing...)" : "");
			string text8 = "Updated: " + text6 + text7;
			ImGui.SetCursorScreenPos(cursorScreenPos5);
			ImU8String text9 = ImU8String.op_Implicit(text8);
			Vector3? colorA = UIShared.Dimmed.AsVector3();
			ImGuiEx.StyledText(text9, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
		}
		ImGui.PopClipRect();
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X, vector.Y + Spacing));
		ImGui.PopID();
	}

	private string GetTotalSizeString()
	{
		return FormatBytes((from x in DataService.GetUDFSnapshot()
			where x.SizeBytes > 0
			select x).Sum((UDF x) => x.SizeBytes));
	}

	private static string FormatBytes(long bytes)
	{
		if (bytes < 0)
		{
			return "Unknown";
		}
		string[] array = new string[5] { "B", "KB", "MB", "GB", "TB" };
		double num = bytes;
		int num2 = 0;
		while (num >= 1024.0 && num2 < array.Length - 1)
		{
			num2++;
			num /= 1024.0;
		}
		return $"{num:0.##} {array[num2]}";
	}

	private void DrawCookiesTab()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X + HorizontalPadding, cursorScreenPos.Y));
		ImGuiEx.StyledText(ImU8String.op_Implicit("Nothing to see here for now :3"), null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, null, null, null, null, null, null, null, null, float.MaxValue);
	}
}
