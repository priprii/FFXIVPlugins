using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using PyonPix.Config;
using PyonPix.Config.UI.Properties;
using PyonPix.Events;
using PyonPix.Extensions;
using PyonPix.Services;
using PyonPix.Services.Core;
using PyonPix.Services.Game;
using PyonPix.Shared.Extensions;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Sync.Dto.Client;
using PyonPix.Shared.Sync.Dto.Syncable;
using PyonPix.Shared.Utility;
using PyonPix.Structs.PlayerState;
using PyonPix.Ui.Components;
using PyonPix.Utility;

namespace PyonPix.Ui.Windows;

public class SyncSearchWindow : BaseWindow
{
	private string Search = string.Empty;

	private string JoinPixId = string.Empty;

	private string JoinPixPass = string.Empty;

	private string? StatusMessage;

	private ContextMenu? FilterCategoryContextMenu;

	private ContextMenu? FilterWorldContextMenu;

	private SyncService SyncService => Services.Get<SyncService>();

	private PixService PixService => Services.Get<PixService>();

	private StateService StateService => Services.Get<StateService>();

	protected override WindowState State
	{
		get
		{
			if (!Config.UI.SyncSearch.Collapsed)
			{
				return WindowState.Expanded;
			}
			return WindowState.Collapsed;
		}
	}

	protected override Vector2 ExpandedSize => Config.UI.SyncSearch.ExpandedSize;

	protected override Vector2 ExpandedMinSize => new Vector2(460f, 220f);

	protected override Vector2 ExpandedMaxSize => UiUtil.GameResolution;

	protected override bool ShowTitleBarSettingsButton => false;

	private float RowHeight => 92f * ImGuiHelpers.GlobalScale;

	private float IconSize => 16f * ImGuiHelpers.GlobalScale;

	private float HorizontalPadding => 8f * ImGuiHelpers.GlobalScale;

	private float VerticalPadding => 8f * ImGuiHelpers.GlobalScale;

	private float Spacing => 6f * ImGuiHelpers.GlobalScale;

	protected override void OnCollapsed(Vector2 windowSize)
	{
		Config.UI.SyncSearch.ExpandedSize = windowSize;
		Config.Save();
	}

	protected override void SetState(WindowState newState)
	{
		if (State != newState)
		{
			Config.UI.SyncSearch.Collapsed = newState == WindowState.Collapsed;
			Config.Save();
		}
	}

	protected override void OnCloseClicked()
	{
		((Window)this).IsOpen = false;
	}

	public override void OnOpen()
	{
		((Window)this).OnOpen();
		Config.UI.SyncSearch.IsOpen = true;
		Config.Save();
		SyncService.QuerySyncablePixs();
	}

	public override void OnClose()
	{
		((Window)this).OnClose();
		Config.UI.SyncSearch.IsOpen = false;
		Config.Save();
	}

	public SyncSearchWindow(Configuration config, IServiceContext services, IWindowContext windows)
		: base("PyonPix Sync Search###PyonPixSyncSearch", config, services, windows, (ImGuiWindowFlags)0)
	{
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(520f, 440f) * ImGuiHelpers.GlobalScale;
		SyncService.SyncablePixsUpdated += delegate
		{
			StatusMessage = null;
		};
		SyncService.SubscriptionFailed += delegate(string reason)
		{
			StatusMessage = reason;
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
			DrawHeader();
			ImGuiEx.Separator(ImGui.GetContentRegionAvail().X);
			DrawRows();
			if (!SyncService.IsConnectedAuth)
			{
				string text = ((SyncService.State != ConnectionState.Connected) ? "Disconnected" : ((!SyncService.Client.IsAuthenticated) ? "Authentication Required" : "Unavailable"));
				StatusBar.Show("Sync Service: " + text, 100, overlay: false, StatusType.Error);
			}
			else if (!string.IsNullOrWhiteSpace(StatusMessage))
			{
				StatusBar.Show(StatusMessage, 4000, overlay: false, StatusType.Error);
				StatusMessage = null;
			}
		}
	}

	private void DrawHeader()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		SyncSearchUIProperties syncSearch = Config.UI.SyncSearch;
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		Vector2 vector = new Vector2(cursorScreenPos.X + WindowPadding.X, cursorScreenPos.Y) + ImGui.GetContentRegionAvail() - WindowPadding;
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X + WindowPadding.X, cursorScreenPos.Y));
		float x = ImGui.GetContentRegionAvail().X;
		ImGuiEx.StyledText(ImU8String.op_Implicit("Join via Pix Id/Password"), null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, null, null, null, null, null, null, null, null, float.MaxValue);
		float x2 = UiUtil.CalcIconTextSize((FontAwesomeIcon)61543, "Join").X;
		float width = (x - IndentWidth - ItemSpacing * 2f - x2) * 0.5f;
		cursorScreenPos = ImGui.GetCursorScreenPos();
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X + IndentWidth, cursorScreenPos.Y));
		ImGuiEx.StyledInput(ImU8String.op_Implicit("##joinId"), ref JoinPixId, "Pix Id", disabled: false, 12, width, (ImGuiInputTextFlags)16, null, null, null, (FontAwesomeIcon)0, null, (FontAwesomeIcon)0);
		ImGui.SameLine(0f, ItemSpacing);
		ImGuiEx.StyledInput(ImU8String.op_Implicit("##joinPass"), ref JoinPixPass, "Password", disabled: false, 32, width, (ImGuiInputTextFlags)16, null, null, null, (FontAwesomeIcon)0, null, (FontAwesomeIcon)0);
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X + x - x2, cursorScreenPos.Y));
		bool disabled = !SyncService.IsConnectedAuth;
		float? height = LineHeight;
		if (ImGuiEx.IconTextButton((FontAwesomeIcon)61543, "Join", "##joinButton", disabled, "Join Pix", "Attempt to subscribe to specified Synced Pix.\nA Synced Pix Id has 'PXS' prefix like: 'PXS?????????'\nPassword is only required if the Pix is private.", null, height))
		{
			StatusMessage = null;
			if (NameUtil.ValidateSyncedPixId(JoinPixId, out StatusMessage))
			{
				SyncService.SubscribePix(JoinPixId, JoinPixPass);
			}
		}
		ImGuiEx.Separator(ImGui.GetContentRegionAvail().X);
		cursorScreenPos = ImGui.GetCursorScreenPos();
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X + WindowPadding.X, cursorScreenPos.Y));
		float num = (LineHeight + ItemSpacing) * 5f + ItemSpacing;
		float x3 = UiUtil.CalcIconTextSize((FontAwesomeIcon)58555, "Refresh").X;
		ImGui.SetCursorScreenPos(cursorScreenPos + new Vector2(WindowPadding.X, 0f));
		ImGuiEx.StyledInput(ImU8String.op_Implicit("##mew"), ref Search, "Search..", disabled: false, 512, x - num - x3 - ItemSpacing - WindowPadding.X, (ImGuiInputTextFlags)16, null, null, null, (FontAwesomeIcon)0, null, (FontAwesomeIcon)61442);
		ImGui.SameLine(0f, ItemSpacing);
		cursorScreenPos = ImGui.GetCursorScreenPos();
		((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(cursorScreenPos, cursorScreenPos + new Vector2(num, LineHeight), UIShared.IconTextBgNormal.ToU32(), 6f);
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X + ItemSpacing, cursorScreenPos.Y));
		bool flag = syncSearch.TypeFilters.Count != 0 || syncSearch.WorldFilters.Count != 0 || syncSearch.SameTerritoryOnly;
		if (ImGuiEx.IconToggleButton((FontAwesomeIcon)61527, "##filterNone", value: false, !flag, "Clear Filters", null, LineHeight))
		{
			syncSearch.TypeFilters.Clear();
			syncSearch.WorldFilters.Clear();
			syncSearch.SameTerritoryOnly = false;
			Config.Save();
		}
		ImGui.SameLine(0f, ItemSpacing);
		bool isClicked = ImGuiEx.IconToggleButton((FontAwesomeIcon)61888, "##filterCategory", syncSearch.TypeFilters.Count != 0, disabled: false, "Filter Category", null, LineHeight);
		DrawFilterCategoryContextMenu(isClicked);
		ImGui.SameLine(0f, ItemSpacing);
		isClicked = ImGuiEx.IconToggleButton((FontAwesomeIcon)61612, "##filterWorld", syncSearch.WorldFilters.Count != 0, disabled: false, "Filter World", null, LineHeight);
		DrawFilterWorldContextMenu(isClicked);
		ImGui.SameLine(0f, ItemSpacing);
		if (ImGuiEx.IconToggleButton((FontAwesomeIcon)62405, "##filterTerritory", syncSearch.SameTerritoryOnly, disabled: false, "Show Current Territory Only", null, LineHeight))
		{
			syncSearch.SameTerritoryOnly = !syncSearch.SameTerritoryOnly;
			Config.Save();
		}
		ImGui.SameLine(0f, ItemSpacing);
		if (ImGuiEx.IconToggleButton((FontAwesomeIcon)61549, "##filterNsfw", syncSearch.ShowNsfw, disabled: false, "Show Nsfw", null, LineHeight))
		{
			syncSearch.ShowNsfw = !syncSearch.ShowNsfw;
			Config.Save();
		}
		ImGui.SameLine(0f, ItemSpacing);
		ImGui.SetCursorScreenPos(new Vector2(vector.X - x3, cursorScreenPos.Y));
		bool disabled2 = !SyncService.IsConnectedAuth;
		height = LineHeight;
		if (ImGuiEx.IconTextButton((FontAwesomeIcon)58555, "Refresh", "##refreshButton", disabled2, null, null, null, height))
		{
			SyncService.QuerySyncablePixs();
		}
	}

	private void DrawFilterCategoryContextMenu(bool isClicked)
	{
		SyncSearchUIProperties syncConfig = Config.UI.SyncSearch;
		bool flag = FilterCategoryContextMenu?.IsOpen() ?? false;
		if (isClicked)
		{
			List<ContextMenuItem> list = new List<ContextMenuItem>();
			list.Add(new ContextMenuCheckbox("Any", () => syncConfig.TypeFilters.Count == 0, delegate(bool x)
			{
				if (x)
				{
					syncConfig.TypeFilters.Clear();
					Config.Save();
				}
			}));
			list.Add(new ContextMenuSeparator());
			PixType[] values = Enum.GetValues<PixType>();
			foreach (PixType type in values)
			{
				list.Add(new ContextMenuCheckbox($"{type}", () => syncConfig.TypeFilters.Contains(type), delegate(bool x)
				{
					if (x)
					{
						syncConfig.TypeFilters.Add(type);
					}
					else
					{
						syncConfig.TypeFilters.Remove(type);
					}
					Config.Save();
				}));
			}
			FilterCategoryContextMenu = new ContextMenu("##filterCategory", list, 100f, 26f);
			FilterCategoryContextMenu.Open();
		}
		if (flag)
		{
			FilterCategoryContextMenu?.Draw();
		}
	}

	private void DrawFilterWorldContextMenu(bool isClicked)
	{
		SyncSearchUIProperties syncConfig = Config.UI.SyncSearch;
		bool flag = FilterWorldContextMenu?.IsOpen() ?? false;
		if (isClicked)
		{
			List<ContextMenuTab> list = new List<ContextMenuTab>();
			Region[] values = Enum.GetValues<Region>();
			foreach (Region region in values)
			{
				List<WorldInfo> regionWorlds = StateService.Worlds[region];
				List<ContextMenuItem> list2 = new List<ContextMenuItem>
				{
					new ContextMenuCheckbox($"Any {region} World", () => syncConfig.WorldFilters.Count == 0 || syncConfig.WorldFilters.Count((ushort x) => regionWorlds.Any((WorldInfo w) => w.Id == x)) == regionWorlds.Count, delegate(bool x)
					{
						if (x)
						{
							foreach (WorldInfo item in regionWorlds)
							{
								syncConfig.WorldFilters.Add(item.Id);
							}
						}
						else
						{
							foreach (WorldInfo item2 in regionWorlds)
							{
								syncConfig.WorldFilters.Remove(item2.Id);
							}
						}
						Config.Save();
					}),
					new ContextMenuSeparator()
				};
				foreach (WorldInfo world in regionWorlds)
				{
					list2.Add(new ContextMenuCheckbox(world.Name, () => syncConfig.WorldFilters.Contains(world.Id), delegate(bool x)
					{
						if (x)
						{
							syncConfig.WorldFilters.Add(world.Id);
						}
						else
						{
							syncConfig.WorldFilters.Remove(world.Id);
						}
						Config.Save();
					}));
				}
				list.Add(new ContextMenuTab($"{region}", $"{region}", list2));
			}
			FilterWorldContextMenu = new ContextMenu("##filterWorld", list, syncConfig.RegionActiveTabIndex, 240f, 26f, 12, delegate(int regionActiveTabIndex)
			{
				syncConfig.RegionActiveTabIndex = regionActiveTabIndex;
				Config.Save();
			});
			FilterWorldContextMenu.Open();
		}
		if (flag)
		{
			FilterWorldContextMenu?.Draw();
		}
	}

	private void DrawRows()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		List<SyncablePixQueryItemDto> syncablePixs = SyncService.SyncablePixs;
		List<SyncablePixQueryItemDto> list = syncablePixs.Where(MatchesFilter).ToList();
		using (UIShared.SubFont.Push())
		{
			ImU8String val = new ImU8String(18, 2);
			((ImU8String)(ref val)).AppendLiteral(" Listing ");
			((ImU8String)(ref val)).AppendFormatted<int>(list.Count);
			((ImU8String)(ref val)).AppendLiteral("/");
			((ImU8String)(ref val)).AppendFormatted<int>(syncablePixs.Count);
			((ImU8String)(ref val)).AppendLiteral(" Results");
			ImU8String text = val;
			Vector3? colorA = UIShared.Muted.AsVector3();
			ImGuiEx.StyledText(text, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
		}
		ImGui.BeginChild(ImU8String.op_Implicit("##syncRows"), ImGui.GetContentRegionAvail(), false, (ImGuiWindowFlags)0);
		foreach (SyncablePixQueryItemDto item in list)
		{
			DrawRow(item);
		}
		ImGui.EndChild();
	}

	private bool MatchesFilter(SyncablePixQueryItemDto item)
	{
		SyncSearchUIProperties syncSearch = Config.UI.SyncSearch;
		if (StateService.CurrentTerritory == null)
		{
			return false;
		}
		if (!syncSearch.ShowNsfw && item.Nsfw)
		{
			return false;
		}
		if (syncSearch.TypeFilters.Count > 0 && !syncSearch.TypeFilters.Contains(item.PixType))
		{
			return false;
		}
		if (syncSearch.SameTerritoryOnly)
		{
			if (item.Territory.WorldId != (short)StateService.CurrentTerritory.WorldId || item.Territory.TerritoryId != (short)StateService.CurrentTerritory.TerritoryId)
			{
				return false;
			}
		}
		else if (syncSearch.WorldFilters.Count > 0 && !syncSearch.WorldFilters.Contains((ushort)item.Territory.WorldId))
		{
			return false;
		}
		if (!string.IsNullOrWhiteSpace(Search))
		{
			string value = Search.Trim();
			if (!item.Name.Contains(value, StringComparison.OrdinalIgnoreCase) && !item.OwnerAlias.Contains(value, StringComparison.OrdinalIgnoreCase) && !item.Description.Contains(value, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
		}
		return true;
	}

	private void DrawRow(SyncablePixQueryItemDto item)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0889: Unknown result type (might be due to invalid IL or missing references)
		//IL_098f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b18: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushID(ImU8String.op_Implicit(item.PixId));
		float x = ImGui.GetContentRegionAvail().X;
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		Vector2 vector = cursorScreenPos + new Vector2(x, RowHeight);
		if (ImGui.IsWindowHovered((ImGuiHoveredFlags)3) && ImGui.IsMouseHoveringRect(cursorScreenPos, vector))
		{
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(cursorScreenPos, vector, ImGui.GetColorU32(UIShared.ItemBgHovered));
		}
		bool flag = PixService.IsSubscribed(item.PixId);
		float num = vector.X - HorizontalPadding - IconSize;
		float y = cursorScreenPos.Y + (RowHeight - IconSize) * 0.5f;
		ImGui.SetCursorScreenPos(new Vector2(num, y));
		if (ImGuiEx.IconToggleButton((FontAwesomeIcon)61633, "##toggleSubscribe", flag, !SyncService.IsConnectedAuth, flag ? "Unsubscribe" : "Subscribe", null, IconSize, 1f, (FontAwesomeIcon)61735))
		{
			StatusMessage = null;
			if (flag)
			{
				SyncService.UnsubscribePix(item.PixId);
			}
			else
			{
				SyncService.SubscribePix(item.PixId, null);
			}
		}
		using (UIShared.SubFont.Push())
		{
			string text = item.Privacy.ToString().ToUpperInvariant();
			ImGui.SetCursorScreenPos(new Vector2(vector.X - HorizontalPadding - ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f).X, cursorScreenPos.Y + VerticalPadding));
			ImU8String text2 = ImU8String.op_Implicit(text);
			Vector3? colorA = UIShared.Muted.AsVector3();
			ImGuiEx.StyledText(text2, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
			if (item.Nsfw)
			{
				ImGui.SetCursorScreenPos(new Vector2(vector.X - HorizontalPadding - ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f).X, vector.Y - VerticalPadding - ImGui.GetFontSize()));
				ImU8String text3 = ImU8String.op_Implicit("NSFW");
				colorA = UIShared.Muted.AsVector3();
				ImGuiEx.StyledText(text3, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
			}
		}
		float num2 = cursorScreenPos.X + HorizontalPadding;
		float x2 = num - Spacing;
		ImGui.PushClipRect(new Vector2(num2, cursorScreenPos.Y), new Vector2(x2, vector.Y), true);
		string text4 = (string.IsNullOrWhiteSpace(item.Name) ? item.PixId : item.Name);
		if (!flag)
		{
			_ = UIShared.ItemHeader;
		}
		else
		{
			_ = UIShared.AccentActive;
		}
		using (UIShared.NormalFont.Push())
		{
			ImGui.SetCursorScreenPos(new Vector2(num2, cursorScreenPos.Y + VerticalPadding));
			ImGuiEx.IconLabel(UiUtil.GetIconForPixType(item.PixType), $"##pixType{item.OwnerId}", null, null, color: UIShared.PixTypeSynced, size: UIShared.NormalFontSize, iconScale: 0.7f);
			ImGui.SetCursorScreenPos(new Vector2(num2 + UIShared.NormalFontSize, cursorScreenPos.Y + VerticalPadding));
			ImU8String text5 = ImU8String.op_Implicit(text4);
			AnimationType animationType = item.OwnerPixStyle?.AnimationType ?? AnimationType.Static;
			Vector3? colorA = item.OwnerPixStyle?.ColourA?.ToVector3();
			Vector3? colorB = item.OwnerPixStyle?.ColourB?.ToVector3();
			Vector3? glowA = item.OwnerPixStyle?.GlowA?.ToVector3();
			Vector3? glowB = item.OwnerPixStyle?.GlowB?.ToVector3();
			ImGuiEx.StyledText(text5, null, 0.8f, 0f, 4f, 0.2f, animationType, colorA, colorB, glowA, glowB, null, null, null, null, float.MaxValue);
		}
		using (UIShared.SubFont.Push())
		{
			ImGui.SetCursorScreenPos(new Vector2(num2, cursorScreenPos.Y + VerticalPadding + UIShared.SubFontSize));
			ImGuiEx.IconLabel(id: $"##rank{item.OwnerId}", tooltip: "Owner", tooltipSub: null, color: UIShared.PixRankOwner, icon: (FontAwesomeIcon)62753, size: UIShared.SubFontSize, iconScale: 0.7f);
			ImGui.SetCursorScreenPos(new Vector2(num2 + UIShared.NormalFontSize, cursorScreenPos.Y + VerticalPadding + UIShared.SubFontSize));
			ImU8String text6 = ImU8String.op_Implicit(item.OwnerAlias);
			AnimationType animationType = item.OwnerAliasStyle?.AnimationType ?? AnimationType.Static;
			Vector3? glowB = item.OwnerAliasStyle?.ColourA?.ToVector3();
			Vector3? glowA = item.OwnerAliasStyle?.ColourB?.ToVector3();
			Vector3? colorB = item.OwnerAliasStyle?.GlowA?.ToVector3();
			Vector3? colorA = item.OwnerAliasStyle?.GlowB?.ToVector3();
			ImGuiEx.StyledText(text6, null, 0.8f, 0f, 4f, 0.2f, animationType, glowB, glowA, colorB, colorA, null, null, null, null, float.MaxValue);
		}
		using (UIShared.SubFont.Push())
		{
			string text7 = (string.IsNullOrWhiteSpace(item.Description) ? "No description" : item.Description);
			ImGui.SetCursorScreenPos(new Vector2(num2, cursorScreenPos.Y + VerticalPadding + ImGui.GetFontSize() * 2f + 2f));
			ImU8String text8 = ImU8String.op_Implicit(text7);
			string tooltip = text7;
			Vector3? colorA = UIShared.Dimmed.AsVector3();
			ImGuiEx.StyledText(text8, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue, multiline: false, tooltip);
			string text9 = (string.IsNullOrWhiteSpace(item.Uri) ? "about:blank" : item.Uri);
			ImGui.SetCursorScreenPos(new Vector2(num2, cursorScreenPos.Y + VerticalPadding + ImGui.GetFontSize() * 3f + 5f));
			ImU8String text10 = ImU8String.op_Implicit(text9);
			tooltip = text9;
			colorA = UIShared.Muted.AsVector3();
			ImGuiEx.StyledText(text10, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue, multiline: false, tooltip);
			string worldName = StateService.GetWorldName((uint)item.Territory.WorldId);
			string territoryName = StateService.GetTerritoryName((ushort)item.Territory.TerritoryId);
			string value = BuildResidence(item.Territory.Ward, item.Territory.Plot, item.Territory.Room);
			string text11 = $"{worldName} - {territoryName} {value}".Trim();
			ImGui.SetCursorScreenPos(new Vector2(num2, cursorScreenPos.Y + VerticalPadding + ImGui.GetFontSize() * 4f + 8f));
			ImU8String text12 = ImU8String.op_Implicit(text11);
			colorA = UIShared.Normal.AsVector3();
			ImGuiEx.StyledText(text12, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
		}
		ImGui.PopClipRect();
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X, vector.Y + Spacing));
		ImGui.PopID();
	}

	private static string BuildResidence(short ward, short plot, short room)
	{
		string text = string.Empty;
		if (ward > 0)
		{
			text += $"W{ward}";
		}
		if (plot > 0)
		{
			text += $" P{plot}";
		}
		if (room > 0)
		{
			text += $" R{room}";
		}
		return text;
	}
}
