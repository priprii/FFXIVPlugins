using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using PyonPix.Config;
using PyonPix.Extensions;
using PyonPix.Services;
using PyonPix.Services.Core;
using PyonPix.Shared.Sync.Dto.Client;
using PyonPix.Structs.Browser;
using PyonPix.Structs.Ui;
using PyonPix.Utility;

namespace PyonPix.Ui.Windows;

public class ExtensionsWindow : BaseWindow
{
	private enum ExtensionTab
	{
		Extensions,
		Browse
	}

	private ExtensionTab ActiveTab;

	private string SearchText = string.Empty;

	private string[] SearchAutoCompleteResults = Array.Empty<string>();

	private List<ExtensionProductDetails> SearchResults = new List<ExtensionProductDetails>();

	private ExtensionsService ExtensionsService => Services.Get<ExtensionsService>();

	protected override WindowState State
	{
		get
		{
			if (!Config.UI.Extensions.Collapsed)
			{
				return WindowState.Expanded;
			}
			return WindowState.Collapsed;
		}
	}

	protected override Vector2 ExpandedSize => Config.UI.Extensions.ExpandedSize;

	protected override Vector2 ExpandedMinSize => new Vector2(300f, 150f);

	protected override Vector2 ExpandedMaxSize => UiUtil.GameResolution;

	private float TabHeight => 28f * ImGuiHelpers.GlobalScale;

	private float ResultRowHeight => 72f * ImGuiHelpers.GlobalScale;

	private float IconSize => 16f * ImGuiHelpers.GlobalScale;

	private float HorizontalPadding => 8f * ImGuiHelpers.GlobalScale;

	private float VerticalPadding => 8f * ImGuiHelpers.GlobalScale;

	private float Spacing => 6f * ImGuiHelpers.GlobalScale;

	public override void OnOpen()
	{
		((Window)this).OnOpen();
		ExtensionsService.ResolveUnknownExtensions();
		if (Config.Global.Browser.CheckUpdateExtensions)
		{
			ExtensionsService.CheckUpdateAllAsync(Config.Global.Browser.AutoUpdateExtensions);
		}
		Config.UI.Extensions.IsOpen = true;
		Config.Save();
	}

	public override void OnClose()
	{
		((Window)this).OnClose();
		Config.UI.Extensions.IsOpen = false;
		Config.Save();
	}

	protected override void OnCollapsed(Vector2 windowSize)
	{
		Config.UI.Extensions.ExpandedSize = windowSize;
		Config.Save();
	}

	protected override void SetState(WindowState newState)
	{
		if (State != newState)
		{
			Config.UI.Extensions.Collapsed = newState == WindowState.Collapsed;
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

	public ExtensionsWindow(Configuration config, IServiceContext services, IWindowContext windows)
		: base("PyonPix Extension Manager###PyonPixExtensions", config, services, windows, (ImGuiWindowFlags)0)
	{
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(420f, 320f) * ImGuiHelpers.GlobalScale;
		((Window)this).PositionCondition = (ImGuiCond)4;
		((Window)this).Position = UiUtil.CenterWindow(((Window)this).Size.Value);
		ExtensionsService.OnAutoCompleteResult += delegate(string[] result)
		{
			SearchAutoCompleteResults = result;
		};
		ExtensionsService.OnSearchResult += delegate(List<ExtensionProductDetails> result)
		{
			SearchResults = result;
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
			if (ActiveTab == ExtensionTab.Extensions)
			{
				DrawExtensionsTab();
			}
			else
			{
				DrawBrowseTab();
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
		if (DrawTab(min, max, "Extensions", ActiveTab == ExtensionTab.Extensions))
		{
			ActiveTab = ExtensionTab.Extensions;
		}
		Vector2 vector = new Vector2(max.X + Spacing, min.Y);
		Vector2 max2 = vector + new Vector2(x, TabHeight);
		if (DrawTab(vector, max2, "Browse", ActiveTab == ExtensionTab.Browse))
		{
			ActiveTab = ExtensionTab.Browse;
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

	private void DrawExtensionsTab()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		ImGui.GetWindowDrawList();
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		float x = ImGui.GetContentRegionAvail().X;
		ImGui.SetCursorScreenPos(cursorScreenPos + new Vector2(HorizontalPadding, 0f));
		if (ImGuiEx.Checkbox("Auto Check##autoCheck", ref Config.Global.Browser.CheckUpdateExtensions, disabled: false, "Automatically check for updates."))
		{
			Config.Save();
		}
		if (Config.Global.Browser.CheckUpdateExtensions)
		{
			ImGui.SameLine();
			if (ImGuiEx.Checkbox("Auto Update##autoUpdate", ref Config.Global.Browser.AutoUpdateExtensions, disabled: false, "Automatically install updates after checking."))
			{
				Config.Save();
			}
		}
		if (Config.Extensions.Count > 0)
		{
			ImGui.SameLine();
			if (ImGuiEx.IconButton((FontAwesomeIcon)58555, "##checkUpdate", ExtensionsService.IsOperating, "Check for updates now."))
			{
				Task.Run(async delegate
				{
					ExtensionsService.CheckUpdateAllAsync(Config.Global.Browser.AutoUpdateExtensions);
				});
			}
		}
		ImGui.SetCursorScreenPos(cursorScreenPos + new Vector2(0f, IconSize + Spacing));
		ImGui.BeginChild(ImU8String.op_Implicit("##extensionRows"), new Vector2(x, ImGui.GetContentRegionAvail().Y), false, (ImGuiWindowFlags)0);
		foreach (KeyValuePair<string, Extension> extension in Config.Extensions)
		{
			if (extension.Value.IsDownloaded)
			{
				DrawExtensionRow(extension.Value);
			}
		}
		ImGui.EndChild();
	}

	private void DrawExtensionRow(Extension item)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_0513: Unknown result type (might be due to invalid IL or missing references)
		//IL_061a: Unknown result type (might be due to invalid IL or missing references)
		if (item.CrxId == null)
		{
			return;
		}
		string crxId = item.CrxId;
		string text = item.Name ?? crxId;
		string text2 = item.Description ?? "";
		string text3 = item.Version ?? "";
		_ = item.Developer;
		bool isUpdateAvailable = item.IsUpdateAvailable;
		bool isInstalled = item.IsInstalled;
		bool isEnabled = item.IsEnabled;
		ImGui.PushID(ImU8String.op_Implicit(crxId));
		float x = ImGui.GetContentRegionAvail().X;
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		Vector2 vector = cursorScreenPos + new Vector2(x, ResultRowHeight);
		_ = vector - cursorScreenPos;
		ImDrawListPtr windowDrawList;
		if (ImGui.IsWindowHovered((ImGuiHoveredFlags)3) && ImGui.IsMouseHoveringRect(cursorScreenPos, vector))
		{
			windowDrawList = ImGui.GetWindowDrawList();
			((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(cursorScreenPos, vector, ImGui.GetColorU32(UIShared.ItemBgHovered));
		}
		float num = HorizontalPadding + IconSize * 0.5f;
		Vector2 cursorScreenPos2 = new Vector2(cursorScreenPos.X + num, cursorScreenPos.Y + (ResultRowHeight - IconSize) * 0.5f);
		if (isInstalled)
		{
			ImGui.SetCursorScreenPos(cursorScreenPos2);
			if (ImGuiEx.Checkbox("##toggle", ref isEnabled, ExtensionsService.IsOperating, "Toggle Extension"))
			{
				Task.Run(delegate
				{
					if (isEnabled)
					{
						ExtensionsService.EnableExtension(crxId);
					}
					else
					{
						ExtensionsService.DisableExtension(crxId);
					}
				});
			}
		}
		float num2 = vector.X - HorizontalPadding - IconSize;
		Vector2 cursorScreenPos3 = new Vector2(num2, cursorScreenPos.Y + (ResultRowHeight - IconSize) * 0.5f);
		ImGui.SetCursorScreenPos(cursorScreenPos3);
		if (!isInstalled && ImGuiEx.IconButton((FontAwesomeIcon)62189, "##remove", ExtensionsService.IsOperating, "Remove Extension", null, IconSize))
		{
			Task.Run(delegate
			{
				ExtensionsService.RemoveExtension(crxId);
			});
		}
		else if (isInstalled && ImGuiEx.IconButton((FontAwesomeIcon)61735, "##uninstall", ExtensionsService.IsOperating, "Uninstall Extension", null, IconSize))
		{
			Task.Run(delegate
			{
				ExtensionsService.UninstallExtension(crxId);
			});
		}
		num2 -= IconSize + HorizontalPadding;
		cursorScreenPos3 = new Vector2(num2, cursorScreenPos3.Y);
		ImGui.SetCursorScreenPos(cursorScreenPos3);
		if (isUpdateAvailable && ImGuiEx.IconButton((FontAwesomeIcon)62307, "##update", ExtensionsService.IsOperating, "Update Extension", null, IconSize))
		{
			Task.Run(async delegate
			{
				ExtensionsService.UpdateAsync(crxId).ConfigureAwait(continueOnCapturedContext: false);
			});
		}
		else if (!isUpdateAvailable && !isInstalled && ImGuiEx.IconButton((FontAwesomeIcon)61633, "##install", ExtensionsService.IsOperating, "Install Extension", null, IconSize))
		{
			Task.Run(delegate
			{
				ExtensionsService.InstallExtension(crxId);
			});
		}
		using (UIShared.SubFont.Push())
		{
			string text4 = "v" + text3;
			Vector2 vector2 = UiUtil.CalcTextSize(text4, ImGui.GetFontSize(), globalScale: false);
			ImGui.SetCursorScreenPos(new Vector2(vector.X - HorizontalPadding - vector2.X, cursorScreenPos3.Y + IconSize + 4f));
			windowDrawList = ImGui.GetWindowDrawList();
			((ImDrawListPtr)(ref windowDrawList)).AddText(ImGui.GetFont(), ImGui.GetFontSize(), ImGui.GetCursorScreenPos(), ImGui.GetColorU32(UIShared.Muted), ImU8String.op_Implicit(text4), 0f);
		}
		float x2 = cursorScreenPos.X + num * 2f + IconSize;
		float x3 = num2 - Spacing;
		ImGui.PushClipRect(new Vector2(x2, cursorScreenPos.Y), new Vector2(x3, vector.Y), true);
		Vector2 cursorScreenPos4 = new Vector2(x2, cursorScreenPos.Y + VerticalPadding);
		using (UIShared.NormalFont.Push())
		{
			ImGui.SetCursorScreenPos(cursorScreenPos4);
			ImU8String text5 = ImU8String.op_Implicit(text);
			Vector3? colorA = UIShared.ItemHeader.AsVector3();
			ImGuiEx.StyledText(text5, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
		}
		Vector2 cursorScreenPos5 = new Vector2(cursorScreenPos4.X, cursorScreenPos4.Y + ImGui.GetFontSize() + Spacing);
		using (UIShared.SubFont.Push())
		{
			ImGui.SetCursorScreenPos(cursorScreenPos5);
			ImU8String text6 = ImU8String.op_Implicit(text2);
			Vector3? colorA = UIShared.Dimmed.AsVector3();
			ImGuiEx.StyledText(text6, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
		}
		Vector2 cursorScreenPos6 = new Vector2(cursorScreenPos4.X, cursorScreenPos5.Y + ImGui.GetFontSize() + Spacing * 0.4f);
		using (UIShared.SubFont.Push())
		{
			ImGui.SetCursorScreenPos(cursorScreenPos6);
			ImU8String text7 = ImU8String.op_Implicit(crxId);
			Vector3? colorA = UIShared.Dimmed.AsVector3();
			ImGuiEx.StyledText(text7, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
		}
		ImGui.PopClipRect();
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X, vector.Y + Spacing));
		ImGui.PopID();
	}

	private void DrawBrowseTab()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		float x = ImGui.GetContentRegionAvail().X;
		float width = x - HorizontalPadding * 2f;
		ImGui.SetCursorScreenPos(cursorScreenPos + new Vector2(HorizontalPadding, 0f));
		if (ImGuiEx.StyledInput(ImU8String.op_Implicit("##search"), ref SearchText, "Search for Extension..", disabled: false, 256, width, (ImGuiInputTextFlags)16, null, null, delegate
		{
			SubmitSearchAndClearAsync(SearchText);
		}, (FontAwesomeIcon)0, null, (FontAwesomeIcon)0, SearchAutoCompleteResults, 3, delegate(string result)
		{
			SubmitSearchAndClearAsync(result);
		}) == UIState.Using)
		{
			ExtensionsService.AutoCompleteAsync(SearchText);
		}
		ImGui.BeginChild(ImU8String.op_Implicit("##searchRows"), new Vector2(x, ImGui.GetContentRegionAvail().Y), false, (ImGuiWindowFlags)0);
		foreach (ExtensionProductDetails searchResult in SearchResults)
		{
			DrawSearchResultRow(searchResult);
		}
		ImGui.EndChild();
	}

	private void DrawSearchResultRow(ExtensionProductDetails item)
	{
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Unknown result type (might be due to invalid IL or missing references)
		//IL_063d: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d9: Unknown result type (might be due to invalid IL or missing references)
		if (item.CrxId == null)
		{
			return;
		}
		string crxId = item.CrxId;
		string text = item.Name ?? crxId;
		string text2 = item.ShortDescription ?? "";
		string text3 = item.Version ?? "";
		_ = item.DeveloperName;
		float num = 0f;
		try
		{
			num = Convert.ToSingle(item.Rating.GetValueOrDefault());
		}
		catch
		{
			num = 0f;
		}
		long num2 = 0L;
		try
		{
			num2 = Convert.ToInt64(item.RatingCount.GetValueOrDefault());
		}
		catch
		{
			num2 = 0L;
		}
		long num3 = 0L;
		try
		{
			num3 = Convert.ToInt64(item.InstallCount.GetValueOrDefault());
		}
		catch
		{
			num3 = 0L;
		}
		Config.Extensions.TryGetValue(crxId, out Extension value);
		bool flag = value?.IsDownloaded ?? false;
		bool flag2 = value?.IsInstalled ?? false;
		bool flag3 = flag && !string.Equals(value?.Version, text3, StringComparison.OrdinalIgnoreCase);
		bool isEnabled = value?.IsEnabled ?? false;
		ImGui.PushID(ImU8String.op_Implicit(crxId));
		float x = ImGui.GetContentRegionAvail().X;
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		Vector2 vector = cursorScreenPos + new Vector2(x, ResultRowHeight);
		_ = vector - cursorScreenPos;
		ImDrawListPtr windowDrawList;
		if (ImGui.IsWindowHovered((ImGuiHoveredFlags)3) && ImGui.IsMouseHoveringRect(cursorScreenPos, vector))
		{
			windowDrawList = ImGui.GetWindowDrawList();
			((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(cursorScreenPos, vector, ImGui.GetColorU32(UIShared.ItemBgHovered));
		}
		float num4 = HorizontalPadding + IconSize * 0.5f;
		Vector2 cursorScreenPos2 = new Vector2(cursorScreenPos.X + num4, cursorScreenPos.Y + (ResultRowHeight - IconSize) * 0.5f);
		if (flag2)
		{
			ImGui.SetCursorScreenPos(cursorScreenPos2);
			if (ImGuiEx.Checkbox("##toggle", ref isEnabled, ExtensionsService.IsOperating, "Toggle Extension"))
			{
				Task.Run(delegate
				{
					if (isEnabled)
					{
						ExtensionsService.EnableExtension(crxId);
					}
					else
					{
						ExtensionsService.DisableExtension(crxId);
					}
				});
			}
		}
		float num5 = vector.X - HorizontalPadding - IconSize;
		Vector2 cursorScreenPos3 = new Vector2(num5, cursorScreenPos.Y + (ResultRowHeight - IconSize) * 0.5f);
		ImGui.SetCursorScreenPos(cursorScreenPos3);
		if (!flag && ImGuiEx.IconButton((FontAwesomeIcon)61465, "##download", ExtensionsService.IsOperating, "Download Extension", null, IconSize))
		{
			Task.Run(async delegate
			{
				ExtensionsService.DownloadOrUpdateAndInstallAsync(crxId).ConfigureAwait(continueOnCapturedContext: false);
			});
		}
		else if (flag && !flag2 && ImGuiEx.IconButton((FontAwesomeIcon)62189, "##remove", ExtensionsService.IsOperating, "Remove Extension", null, IconSize))
		{
			Task.Run(delegate
			{
				ExtensionsService.RemoveExtension(crxId);
			});
		}
		else if (flag2 && ImGuiEx.IconButton((FontAwesomeIcon)61735, "##uninstall", ExtensionsService.IsOperating, "Uninstall Extension", null, IconSize))
		{
			Task.Run(delegate
			{
				ExtensionsService.UninstallExtension(crxId);
			});
		}
		num5 -= IconSize + HorizontalPadding;
		cursorScreenPos3 = new Vector2(num5, cursorScreenPos3.Y);
		if (flag3 || (flag && !flag2))
		{
			ImGui.SetCursorScreenPos(cursorScreenPos3);
			if (flag3 && ImGuiEx.IconButton((FontAwesomeIcon)62307, "##update", ExtensionsService.IsOperating, "Update Extension", null, IconSize))
			{
				Task.Run(async delegate
				{
					ExtensionsService.UpdateAsync(crxId).ConfigureAwait(continueOnCapturedContext: false);
				});
			}
			else if (flag && !flag2 && ImGuiEx.IconButton((FontAwesomeIcon)61633, "##install", ExtensionsService.IsOperating, "Install Extension", null, IconSize))
			{
				Task.Run(delegate
				{
					ExtensionsService.InstallExtension(crxId);
				});
			}
		}
		using (UIShared.SubFont.Push())
		{
			string text4 = "v" + text3;
			Vector2 vector2 = UiUtil.CalcTextSize(text4, ImGui.GetFontSize(), globalScale: false);
			ImGui.SetCursorScreenPos(new Vector2(vector.X - HorizontalPadding - vector2.X, cursorScreenPos3.Y + IconSize + 4f));
			windowDrawList = ImGui.GetWindowDrawList();
			((ImDrawListPtr)(ref windowDrawList)).AddText(ImGui.GetFont(), ImGui.GetFontSize(), ImGui.GetCursorScreenPos(), ImGui.GetColorU32(UIShared.Muted), ImU8String.op_Implicit(text4), 0f);
		}
		float x2 = cursorScreenPos.X + num4 * 2f + IconSize;
		float x3 = num5 - Spacing;
		ImGui.PushClipRect(new Vector2(x2, cursorScreenPos.Y), new Vector2(x3, vector.Y), true);
		Vector2 cursorScreenPos4 = new Vector2(x2, cursorScreenPos.Y + VerticalPadding);
		using (UIShared.NormalFont.Push())
		{
			ImGui.SetCursorScreenPos(cursorScreenPos4);
			ImU8String text5 = ImU8String.op_Implicit(text);
			Vector3? colorA = UIShared.ItemHeader.AsVector3();
			ImGuiEx.StyledText(text5, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
		}
		Vector2 cursorScreenPos5 = new Vector2(cursorScreenPos4.X, cursorScreenPos4.Y + ImGui.GetFontSize() + Spacing * 0.6f);
		using (UIShared.SubFont.Push())
		{
			ImGui.SetCursorScreenPos(cursorScreenPos5);
			ImU8String text6 = ImU8String.op_Implicit(text2);
			Vector3? colorA = UIShared.Dimmed.AsVector3();
			ImGuiEx.StyledText(text6, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
		}
		Vector2 pos = new Vector2(cursorScreenPos4.X, cursorScreenPos5.Y + ImGui.GetFontSize() + Spacing * 0.6f);
		Vector2 vector3 = DrawRatingStars(pos, num);
		Vector2 cursorScreenPos6 = new Vector2(pos.X + vector3.X + 4f * ImGuiHelpers.GlobalScale, pos.Y);
		string text7 = "(" + num2.ToString("N0", CultureInfo.CurrentCulture) + ")";
		string text8 = "Users: " + num3.ToString("N0", CultureInfo.CurrentCulture);
		using (UIShared.SubFont.Push())
		{
			ImGui.SetCursorScreenPos(cursorScreenPos6);
			ImU8String val = new ImU8String(1, 2);
			((ImU8String)(ref val)).AppendFormatted<string>(text7);
			((ImU8String)(ref val)).AppendLiteral(" ");
			((ImU8String)(ref val)).AppendFormatted<string>(text8);
			ImU8String text9 = val;
			Vector3? colorA = UIShared.Muted.AsVector3();
			ImGuiEx.StyledText(text9, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
		}
		ImGui.PopClipRect();
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X, vector.Y + Spacing));
		ImGui.PopID();
	}

	private Vector2 DrawRatingStars(Vector2 pos, float rating)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		float num = 0f;
		float num2 = 12f * ImGuiHelpers.GlobalScale;
		float num3 = 2f * ImGuiHelpers.GlobalScale;
		int num4 = (int)Math.Floor(rating);
		float num5 = rating - (float)num4;
		using (UIShared.NormalIconFont.Push())
		{
			for (int i = 0; i < 5; i++)
			{
				Vector2 vector = new Vector2(pos.X + (float)i * (num2 + num3), pos.Y);
				if (i < num4)
				{
					((ImDrawListPtr)(ref windowDrawList)).AddText(ImGui.GetFont(), num2, vector, ImGui.GetColorU32(UiUtil.RGBA(255, 200, 40, 255f)), ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61445)), 0f);
				}
				else if (i == num4 && num5 >= 0.5f)
				{
					((ImDrawListPtr)(ref windowDrawList)).AddText(ImGui.GetFont(), num2, vector, ImGui.GetColorU32(UiUtil.RGBA(255, 200, 40, 255f)), ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)62912)), 0f);
				}
				else
				{
					((ImDrawListPtr)(ref windowDrawList)).AddText(ImGui.GetFont(), num2, vector, ImGui.GetColorU32(UIShared.Muted), ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61445)), 0f);
				}
				num = vector.X - pos.X;
			}
		}
		return new Vector2(num + num2, num2);
	}

	private async Task SubmitSearchAndClearAsync(string query)
	{
		if (!string.IsNullOrWhiteSpace(query))
		{
			SearchText = string.Empty;
			SearchAutoCompleteResults = Array.Empty<string>();
			ExtensionsService.SearchAsync(query).ConfigureAwait(continueOnCapturedContext: false);
		}
	}
}
