using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
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
using PyonPix.Shared.Extensions;
using PyonPix.Shared.Structs;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Structs.Territory;
using PyonPix.Shared.Sync.Dto.Client;
using PyonPix.Structs.Browser;
using PyonPix.Ui.Components;
using PyonPix.Utility;

namespace PyonPix.Ui.Windows;

public class MainWindow : BaseWindow
{
	private ContextMenu? PixContextMenu;

	private PixService PixService => Services.Get<PixService>();

	private SyncService SyncService => Services.Get<SyncService>();

	private StateService StateService => Services.Get<StateService>();

	private BrowserService BrowserService => Services.Get<BrowserService>();

	protected override WindowState State
	{
		get
		{
			if (!Config.UI.Main.Collapsed)
			{
				return WindowState.Expanded;
			}
			return WindowState.Collapsed;
		}
	}

	protected override Vector2 ExpandedSize => Config.UI.Main.ExpandedSize;

	protected override Vector2 ExpandedMinSize => new Vector2(300f, 250f);

	protected override Vector2 ExpandedMaxSize => new Vector2(300f, UiUtil.GameHeight);

	public override void OnOpen()
	{
		((Window)this).OnOpen();
		Config.UI.Main.IsOpen = true;
		Config.Save();
	}

	public override void OnClose()
	{
		((Window)this).OnClose();
		Config.UI.Main.IsOpen = false;
		Config.Save();
	}

	protected override void OnCollapsed(Vector2 windowSize)
	{
		Config.UI.Main.ExpandedSize = windowSize;
		Config.Save();
	}

	protected override void SetState(WindowState newState)
	{
		Config.UI.Main.Collapsed = newState == WindowState.Collapsed;
		Config.Save();
	}

	protected override void OnConfigClicked()
	{
		((Window)Windows.Get<ConfigWindow>()).Toggle();
	}

	protected override void OnCloseClicked()
	{
		((Window)this).IsOpen = false;
	}

	protected override float DrawControlExtras(float rightCursor)
	{
		if (!((Window)this).IsOpen)
		{
			return rightCursor;
		}
		rightCursor -= base.TitleBarFrameHeight;
		ImGui.SetCursorScreenPos(new Vector2(rightCursor, base.HeaderMin.Y));
		UpdatesWindow updatesWindow = Windows.Get<UpdatesWindow>();
		string tooltip = (((Window)updatesWindow).IsOpen ? "Close PyonPix Changelog" : "Open PyonPix Changelog");
		bool isOpen = ((Window)updatesWindow).IsOpen;
		float? size = base.TitleBarFrameHeight;
		if (ImGuiEx.IconToggleButton((FontAwesomeIcon)61530, "##updates", isOpen, disabled: false, tooltip, null, size, 0.8f))
		{
			((Window)updatesWindow).Toggle();
		}
		rightCursor -= base.TitleBarFrameHeight;
		ImGui.SetCursorScreenPos(new Vector2(rightCursor, base.HeaderMin.Y));
		if (ImGuiEx.IconToggleButton((FontAwesomeIcon)61444, "##kofi", value: true, disabled: false, "Support me on Ko-fi!", null, base.TitleBarFrameHeight, 0.8f))
		{
			UiUtil.OpenKofi();
		}
		rightCursor -= base.TitleBarFrameHeight;
		ImGui.SetCursorScreenPos(new Vector2(rightCursor, base.HeaderMin.Y));
		if (ImGuiEx.IconToggleButton((FontAwesomeIcon)61445, "##discord", value: true, disabled: false, "Join the Pyon Discord!", null, base.TitleBarFrameHeight, 0.8f))
		{
			UiUtil.OpenDiscord();
		}
		return rightCursor;
	}

	public MainWindow(Configuration config, IServiceContext services, IWindowContext windows)
		: base($"{"PyonPix"} {Plugin.Version}###{"PyonPix"}Main", config, services, windows, (ImGuiWindowFlags)0)
	{
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(300f, 450f) * ImGuiHelpers.GlobalScale;
		((Window)this).PositionCondition = (ImGuiCond)4;
		((Window)this).Position = UiUtil.CenterWindow(((Window)this).Size.Value);
		SyncService.StateChanged += delegate(ConnectionState connectionState, string? statusMessage, StatusType statusType)
		{
			switch (statusType)
			{
			case StatusType.None:
				break;
			case StatusType.Hide:
				StatusBar.Hide();
				break;
			default:
				StatusBar.Show(statusMessage ?? "", 8000, overlay: true, statusType);
				break;
			}
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
			ImGuiEx.Separator(ImGui.GetContentRegionAvail().X, 0f);
			DrawPixTree();
		}
	}

	private void DrawHeader()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b90: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c8f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0837: Unknown result type (might be due to invalid IL or missing references)
		//IL_0852: Unknown result type (might be due to invalid IL or missing references)
		//IL_0958: Unknown result type (might be due to invalid IL or missing references)
		//IL_0995: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a9a: Unknown result type (might be due to invalid IL or missing references)
		ImGui.GetWindowDrawList();
		float globalScale = ImGuiHelpers.GlobalScale;
		float num = 4f * globalScale;
		float num2 = 4f * globalScale;
		float num3 = 6f * globalScale;
		float num4 = 18f * globalScale;
		string text = SyncService.State switch
		{
			ConnectionState.Disconnected => "offline", 
			ConnectionState.Connecting => "syncing", 
			ConnectionState.Connected => "online", 
			_ => string.Empty, 
		};
		Vector2 vector = UiUtil.CalcTextSize(UIShared.SubFont, text) + UIShared.TextBgPadding * 2f;
		float num5 = num4 + vector.Y + num * 2f + num2;
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		Vector2 vector2 = cursorScreenPos + new Vector2(ImGui.GetContentRegionAvail().X, num5);
		Vector2 vector3 = vector2 - cursorScreenPos;
		float num6 = cursorScreenPos.Y + vector3.Y * 0.5f;
		float num7 = num4 * 2f + num3 * 2f + num2;
		float num8 = cursorScreenPos.X + num7 * 0.5f;
		float num9 = num4 * 3f + num2 * 2f + num3 * 2f;
		float num10 = vector2.X - num9 + num3;
		float num11 = cursorScreenPos.X + num7;
		float num12 = vector2.X - num9;
		float num13 = num11 + (num12 - num11) * 0.5f;
		float num14 = cursorScreenPos.Y + num;
		float num15 = cursorScreenPos.X + num3;
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X + num3, num14));
		bool localPlayerExists = StateService.LocalPlayerExists;
		bool flag = SyncService.State == ConnectionState.Connected;
		bool flag2 = SyncService.State == ConnectionState.Connecting;
		Vector3 value = (flag ? new Vector3(0f, 0.8f, 0f) : (flag2 ? new Vector3(0.8f, 0.8f, 0f) : new Vector3(0.8f, 0f, 0f)));
		if (ImGuiEx.IconToggleButton(label: "##syncConnect", value: flag || flag2, disabled: !localPlayerExists && !flag, toggledIcon: (FontAwesomeIcon)61735, icon: (FontAwesomeIcon)61633, tooltip: flag ? "Disconnect from Sync Service" : "Connect to Sync Service", tooltipSub: null, size: num4))
		{
			if (flag)
			{
				SyncService.Disconnect();
				Config.Sync.AutoConnect = false;
			}
			else if (flag2)
			{
				SyncService.AbortConnection();
				Config.Sync.AutoConnect = false;
			}
			else
			{
				SyncService.Connect();
				Config.Sync.AutoConnect = true;
			}
			Config.Save();
		}
		if (Windows.TryGet<SyncSearchWindow>(out SyncSearchWindow window))
		{
			ImGui.SetCursorScreenPos(new Vector2(num15 + num4 + num2, num14));
			if (ImGuiEx.IconToggleButton((FontAwesomeIcon)61442, "##openSyncWindow", ((Window)window).IsOpen, !StateService.LocalPlayerExists || !flag, ((Window)window).IsOpen ? "Close Sync Search" : "Open Sync Search", null, num4))
			{
				((Window)window).Toggle();
			}
		}
		using (UIShared.SubFont.Push())
		{
			ImGui.SetCursorScreenPos(new Vector2(num8 - vector.X * 0.5f, num14 + num4 + num2));
			ImU8String text2 = ImU8String.op_Implicit(text);
			Vector3? colorA = value;
			ImGuiEx.StyledText(text2, null, 0.8f, 0.3f, 4f, 0.1f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
		}
		ImGui.SetCursorScreenPos(new Vector2(num11, cursorScreenPos.Y + num5 * 0.1f));
		if (SyncService.State == ConnectionState.Connected && !SyncService.Client.IsAuthenticated && !string.IsNullOrEmpty(SyncService.Client.AuthKey))
		{
			using (UIShared.SubFont.Push())
			{
				ImGui.SetCursorScreenPos(new Vector2(num11 + num3, cursorScreenPos.Y + num3));
				float x = ImGui.CalcTextSize(ImU8String.op_Implicit("AuthKey:"), false, -1f).X;
				ImGuiEx.StyledText(ImU8String.op_Implicit("AuthKey:"), null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, null, null, null, null, null, null, null, null, float.MaxValue);
				ImGui.SetCursorScreenPos(new Vector2(num11 + num3 + x + num2, cursorScreenPos.Y + num3));
				ImGuiEx.StyledText(ImU8String.op_Implicit(SyncService.Client.AuthKey), action: delegate
				{
					//IL_0027: Unknown result type (might be due to invalid IL or missing references)
					if (!string.IsNullOrEmpty(SyncService.Client.AuthKey))
					{
						ImGui.SetClipboardText(ImU8String.op_Implicit(SyncService.Client.AuthKey));
					}
				}, fontSize: null, opacity: 0.8f, bgOpacity: 0f, bgRounding: 4f, glowStrength: 0.2f, animationType: AnimationType.RainbowWave, colorA: null, colorB: null, glowA: null, glowB: null, bgColor: null, xPadding: null, yPadding: null, width: null, wrapWidth: float.MaxValue, multiline: false, tooltip: "PyonPix Sync Service Registration", tooltipSub: "- Click this key to copy it.\n- Go to the Pyon Discord server (if you have not yet joined, click the star above)\n- Check #pyonpix channel for registration form.");
				string text3 = "Expires in " + SyncService.Client.GetAuthExpirationTime();
				ImGui.SetCursorScreenPos(new Vector2(num11 + num3, num14 + num4 + num2));
				ImU8String text4 = ImU8String.op_Implicit(text3);
				Vector3? colorA = new Vector3(0.8f, 0f, 0f);
				ImGuiEx.StyledText(text4, null, 0.8f, 0.3f, 4f, 0.1f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
			}
		}
		else if (SyncService.State == ConnectionState.Connected)
		{
			using (UIShared.SubFont.Push())
			{
				ImGui.SetCursorScreenPos(new Vector2(num11 + num3, cursorScreenPos.Y + num3));
				UserWindow userWindow = Windows.Get<UserWindow>();
				if (ImGuiEx.IconToggleButton((FontAwesomeIcon)62719, "##userWindow", ((Window)userWindow).IsOpen, disabled: false, ((Window)userWindow).IsOpen ? "Close User Config" : "Open User Config", null, null, 0.7f))
				{
					((Window)userWindow).Toggle();
				}
				ImGui.SameLine(0f, 0f);
				CharacterProperties style = SyncService.Client.Style;
				string alias = style.Alias;
				ImGui.CalcTextSize(ImU8String.op_Implicit(alias), false, -1f);
				float value2 = num10 - num11 - num4;
				ImU8String text5 = ImU8String.op_Implicit(alias);
				float? wrapWidth = value2;
				AnimationType aliasAnimationType = style.AliasAnimationType;
				Vector3? colorA = style.AliasColourA;
				Vector3? colorB = style.AliasColourB;
				Vector3? glowA = style.AliasGlowA;
				Vector3? glowB = style.AliasGlowB;
				ImGuiEx.StyledText(text5, null, 0.8f, 0f, 4f, 0.2f, aliasAnimationType, colorA, colorB, glowA, glowB, null, null, null, null, wrapWidth);
				string obj = $"{SyncService.Server.UserCount} users";
				float num16 = ImGui.CalcTextSize(ImU8String.op_Implicit(obj), false, -1f).X + UIShared.TextBgPadding.X * 2f;
				ImGui.SetCursorScreenPos(new Vector2(num11 + num3, num14 + num4 + num2));
				ImU8String text6 = ImU8String.op_Implicit(obj);
				glowB = UIShared.AccentActive.AsVector3();
				ImGuiEx.StyledText(text6, null, 0.8f, 0.3f, 4f, 0.1f, AnimationType.Static, glowB, null, null, null, null, null, null, null, float.MaxValue);
				string obj2 = $"{SyncService.Server.PixCount} pixs";
				ImGui.SetCursorScreenPos(new Vector2(num11 + num3 + num16 + num2, num14 + num4 + num2));
				ImU8String text7 = ImU8String.op_Implicit(obj2);
				glowB = UIShared.AccentActive.AsVector3();
				ImGuiEx.StyledText(text7, null, 0.8f, 0.3f, 4f, 0.1f, AnimationType.Static, glowB, null, null, null, null, null, null, null, float.MaxValue);
			}
		}
		else if (!string.IsNullOrEmpty(SyncService.StatusMessage))
		{
			using (UIShared.SubFont.Push())
			{
				string? statusMessage = SyncService.StatusMessage;
				Vector2 vector4 = ImGui.CalcTextSize(ImU8String.op_Implicit(statusMessage), false, -1f);
				if (SyncService.Client.IsSecretKeyInvalid)
				{
					float num17 = ImGui.CalcTextSize(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)62719)), false, -1f).X * 0.7f;
					ImGui.SetCursorScreenPos(new Vector2(num13 - vector4.X * 0.5f - num3 - num17, num6 - vector4.Y * 0.5f));
					UserWindow userWindow2 = Windows.Get<UserWindow>();
					if (ImGuiEx.IconToggleButton((FontAwesomeIcon)62719, "##userWindow", ((Window)userWindow2).IsOpen, disabled: false, ((Window)userWindow2).IsOpen ? "Close User Config" : "Open User Config", null, null, 0.7f))
					{
						((Window)userWindow2).Toggle();
					}
				}
				ImGui.SetCursorScreenPos(new Vector2(num13 - vector4.X * 0.5f, num6 - vector4.Y * 0.5f));
				ImU8String text8 = ImU8String.op_Implicit(statusMessage);
				Vector3? glowB = new Vector3(value.X - 0.2f, value.Y - 0.2f, 0f);
				Vector3? glowA = new Vector3(value.X + 0.2f, value.Y + 0.2f, 0f);
				ImGuiEx.StyledText(text8, null, 0.8f, 0.3f, 4f, 0.1f, AnimationType.Pulse, glowB, glowA, null, null, null, null, null, null, float.MaxValue);
			}
		}
		float y = cursorScreenPos.Y + (num5 - num4) * 0.5f;
		float num18 = vector2.X - num3;
		ImGui.SetCursorScreenPos(new Vector2(num18 - num4, y));
		if (ImGuiEx.IconButton((FontAwesomeIcon)61543, "##addHeader", disabled: false, "Create Pix", null, num4))
		{
			IPix pix = PixService.CreateLocalPix();
			Windows.Get<PixConfigWindow>().Toggle(pix);
			string item = pix.Territory.ToString();
			if (!Config.UI.Main.ExpandedTerritories.Contains(item))
			{
				Config.UI.Main.ExpandedTerritories.Add(item);
				Config.Save();
			}
		}
		ImGui.SetCursorScreenPos(new Vector2(num18 - num4 * 2f - num2, y));
		if (ImGuiEx.IconButton((FontAwesomeIcon)61674, "##pasteHeader", disabled: false, "Paste Pix", null, num4))
		{
			IPix pix2 = PixService.PastePixFromClipboard();
			Windows.Get<PixConfigWindow>().Toggle(pix2);
		}
		ImGui.SetCursorScreenPos(new Vector2(num18 - num4 * 3f - num2 * 2f, y));
		BrowserWindow browserWindow = Windows.Get<BrowserWindow>();
		if (ImGuiEx.IconToggleButton(label: "##browserHeader", value: ((Window)browserWindow).IsOpen, disabled: false, size: num4, icon: (FontAwesomeIcon)61612, tooltip: ((Window)browserWindow).IsOpen ? "Hide Browser" : "Show Browser"))
		{
			((Window)browserWindow).Toggle();
		}
		ImGui.SetCursorScreenPos(cursorScreenPos + new Vector2(0f, vector3.Y));
	}

	private void DrawPixTree()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(7, 0);
		((ImU8String)(ref val)).AppendLiteral("PixTree");
		ImGui.BeginChild(val, ImGui.GetContentRegionAvail(), false, (ImGuiWindowFlags)0);
		TerritoryData currentTerritory = StateService.CurrentTerritory;
		if (currentTerritory != null)
		{
			DrawTerritoryRow(currentTerritory, isCurrentTerritory: true);
		}
		IReadOnlyList<TerritoryData> pixTerritories = PixService.GetPixTerritories();
		if (pixTerritories != null)
		{
			foreach (TerritoryData item in pixTerritories)
			{
				if (!item.MatchesWTWP(currentTerritory))
				{
					DrawTerritoryRow(item, isCurrentTerritory: false);
				}
			}
		}
		ImGui.EndChild();
	}

	private void DrawTerritoryRow(TerritoryData t, bool isCurrentTerritory)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		string text = t.ToString();
		ImGui.PushID(ImU8String.op_Implicit(text));
		float globalScale = ImGuiHelpers.GlobalScale;
		float num = 8f * globalScale;
		float num2 = 6f * globalScale;
		float num3 = 4f * globalScale;
		float num4 = 18f * globalScale;
		string text2 = (t.WorldName + " " + StateService.GetResidenceFormatted(t)).Trim();
		string territoryName = t.TerritoryName;
		string territorySubName = t.TerritorySubName;
		string text3 = (string.IsNullOrEmpty(territorySubName) ? territoryName : (territoryName + " - " + territorySubName));
		Vector2 vector;
		using (UIShared.NormalFont.Push())
		{
			vector = ImGui.CalcTextSize(ImU8String.op_Implicit(text2), false, -1f);
		}
		Vector2 vector2;
		using (UIShared.SubFont.Push())
		{
			vector2 = ImGui.CalcTextSize(ImU8String.op_Implicit(text3), false, -1f);
		}
		float num5 = vector.Y + num3 + vector2.Y;
		float x = num5 + num2 * 2f;
		x = MathF.Max(x, num4 + num2 * 2f);
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		float x2 = ImGui.GetContentRegionAvail().X;
		Vector2 vector3 = cursorScreenPos;
		Vector2 vector4 = cursorScreenPos + new Vector2(x2, x);
		float y = vector3.Y + (x - num4) * 0.5f;
		Vector2 cursorScreenPos2 = new Vector2(vector3.X + num, y);
		bool flag = ImGui.IsWindowHovered((ImGuiHoveredFlags)3) && ImGui.IsMouseHoveringRect(vector3, vector4);
		bool flag2 = ImGui.IsMouseHoveringRect(new Vector2(cursorScreenPos2.X + num4, vector3.Y), new Vector2(vector4.X - num3, vector4.Y));
		bool num6 = flag && flag2 && ImGui.IsMouseReleased((ImGuiMouseButton)0);
		bool flag3 = Config.UI.Main.ExpandedTerritories.Contains(text);
		Vector4 vector5 = (num6 ? UIShared.PixTerritoryBgActive : ((flag3 && flag) ? UIShared.PixTerritoryBgExpandedHovered : (flag3 ? UIShared.PixTerritoryBgExpanded : (flag ? UIShared.PixTerritoryBgHovered : UIShared.PixTerritoryBgNormal))));
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(vector3, vector4, ImGui.GetColorU32(vector5));
		ImGui.SetCursorScreenPos(cursorScreenPos2);
		if (num6 || ImGuiEx.IconButton((FontAwesomeIcon)(flag3 ? 61655 : 61658), "##expand", disabled: false, null, null, num4))
		{
			if (flag3)
			{
				Config.UI.Main.ExpandedTerritories.Remove(text);
			}
			else
			{
				Config.UI.Main.ExpandedTerritories.Add(text);
			}
			Config.Save();
		}
		float x3 = vector3.X + num + num4 + num3;
		float x4 = vector4.X - num3;
		ImGui.PushClipRect(new Vector2(x3, vector3.Y), new Vector2(x4, vector4.Y), true);
		float num7 = vector3.Y + (x - num5) * 0.5f;
		using (UIShared.NormalFont.Push())
		{
			Vector4 vector6 = (isCurrentTerritory ? UIShared.AccentActive : UIShared.ItemSubText);
			windowDrawList = ImGui.GetWindowDrawList();
			((ImDrawListPtr)(ref windowDrawList)).AddText(ImGui.GetFont(), ImGui.GetFontSize(), new Vector2(x3, num7), ImGui.GetColorU32(vector6), ImU8String.op_Implicit(text2), 0f);
		}
		using (UIShared.SubFont.Push())
		{
			windowDrawList = ImGui.GetWindowDrawList();
			((ImDrawListPtr)(ref windowDrawList)).AddText(ImGui.GetFont(), ImGui.GetFontSize(), new Vector2(x3, num7 + vector.Y + num3), ImGui.GetColorU32(UIShared.ItemSubText), ImU8String.op_Implicit(text3), 0f);
		}
		ImGui.PopClipRect();
		ImGui.PopID();
		if (flag3)
		{
			float indent = 20f * globalScale;
			ImGui.SetCursorScreenPos(new Vector2(vector3.X, vector4.Y));
			{
				foreach (IPix item in PixService.GetOrderedPixsForTerritory(t, persistent: true))
				{
					DrawPixRow(item, indent, isCurrentTerritory);
				}
				return;
			}
		}
		ImGui.SetCursorScreenPos(new Vector2(vector3.X, vector4.Y));
	}

	private void DrawPixRow(IPix pix, float indent, bool isCurrentTerritory)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0524: Unknown result type (might be due to invalid IL or missing references)
		//IL_046e: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushID(ImU8String.op_Implicit(pix.Id));
		BrowserService.Tabs.TryGetValue(pix.Id, out Tab _);
		SyncedPix syncedPix = pix as SyncedPix;
		float globalScale = ImGuiHelpers.GlobalScale;
		float num = 8f * globalScale;
		float num2 = 4f * globalScale;
		float num3 = 4f * globalScale;
		float num4 = 16f * globalScale;
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		float x = ImGui.GetContentRegionAvail().X;
		float num5 = num4 + num2 * 2f;
		Vector2 vector = cursorScreenPos;
		Vector2 vector2 = cursorScreenPos + new Vector2(x, num5);
		if (ImGui.IsWindowHovered((ImGuiHoveredFlags)3) && ImGui.IsMouseHoveringRect(vector, vector2))
		{
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(vector, vector2, ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.05f)));
		}
		FontAwesomeIcon iconForPixType = UiUtil.GetIconForPixType(pix.Info.Type);
		float y = vector.Y + (num5 - num4) * 0.5f;
		Vector2 cursorScreenPos2 = new Vector2(vector.X + num + indent, y);
		ImGui.SetCursorScreenPos(cursorScreenPos2);
		if (syncedPix != null)
		{
			ImGuiEx.IconLabel(iconForPixType, "##icon", size: num4, color: UIShared.PixTypeSynced, tooltip: $"Synced {pix.Info.Type} ({pix.Id})", tooltipSub: $"Owner: {syncedPix.OwnerAlias}\nPrivacy: {syncedPix.Sync.Privacy}", iconScale: 1f, hover: false);
		}
		else
		{
			ImGuiEx.IconLabel(iconForPixType, "##icon", size: num4, tooltip: $"Local {pix.Info.Type} ({pix.Id})", tooltipSub: null, iconScale: 1f, color: null, hover: false);
		}
		float num6 = vector2.X - num - num4;
		float num7 = num6 - num3 - num4;
		Vector2 cursorScreenPos3 = new Vector2(num7, y);
		Vector2 vector3 = new Vector2(num6, y);
		bool flag = PixService.IsSpawned(pix);
		bool flag2 = PixService.IsActive(pix);
		ImGui.SetCursorScreenPos(cursorScreenPos3);
		int num8 = (flag2 ? 61516 : 61515);
		if (ImGuiEx.IconToggleButton(label: "##pixToggle", value: flag2 && flag, disabled: false, size: num4, icon: (FontAwesomeIcon)num8, tooltip: (!isCurrentTerritory) ? ((flag2 ? "Disable Pix" : "Enable Pix") ?? "") : ((flag ? "Despawn Pix" : "Spawn Pix") ?? ""), tooltipSub: isCurrentTerritory ? string.Empty : "Toggle whether to spawn this Pix when you're in the same territory."))
		{
			PixService.Toggle(pix);
		}
		bool flag3 = PixContextMenu?.IsOpen() ?? false;
		ImGui.SetCursorScreenPos(vector3);
		if (ImGuiEx.IconToggleButton((FontAwesomeIcon)61762, "##pixMenu", flag3 || Windows.Get<PixConfigWindow>().SelectedPix == pix, disabled: false, "Manage Pix", null, num4))
		{
			PixContextMenu = BuildContextMenu(pix);
			PixContextMenu.Open();
		}
		if (flag3)
		{
			PixContextMenu?.Draw(vector3 + new Vector2(num4, 0f));
		}
		float x2 = cursorScreenPos2.X + num4 + num3;
		float x3 = num7 - num3;
		ImGui.PushClipRect(new Vector2(x2, vector.Y), new Vector2(x3, vector2.Y), true);
		string displayName = pix.GetDisplayName();
		Vector2 vector4 = ImGui.CalcTextSize(ImU8String.op_Implicit(displayName), false, -1f);
		float y2 = vector.Y + (num5 - vector4.Y) * 0.5f;
		ImGui.SetCursorScreenPos(new Vector2(x2, y2));
		if (syncedPix == null)
		{
			if (!isCurrentTerritory)
			{
				_ = UIShared.ItemInactive;
			}
			else
			{
				_ = UIShared.ItemHeader;
			}
			ImGuiEx.StyledText(ImU8String.op_Implicit(displayName), UIShared.NormalFontSize, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, null, null, null, null, null, null, null, null, float.MaxValue);
		}
		else
		{
			ImGuiEx.StyledText(ImU8String.op_Implicit(displayName), UIShared.NormalFontSize, 0.8f, 0f, 4f, 0.2f, syncedPix.OwnerPixStyle?.AnimationType ?? AnimationType.Static, syncedPix.OwnerPixStyle?.ColourA?.ToVector3(), syncedPix.OwnerPixStyle?.ColourB?.ToVector3(), syncedPix.OwnerPixStyle?.GlowA?.ToVector3(), syncedPix.OwnerPixStyle?.GlowB?.ToVector3(), null, null, null, null, float.MaxValue);
		}
		ImGui.PopClipRect();
		ImGui.SetCursorScreenPos(new Vector2(vector.X, vector2.Y));
		ImGui.PopID();
	}

	private ContextMenu BuildContextMenu(IPix? pix)
	{
		List<ContextMenuItem> list = new List<ContextMenuItem>
		{
			new ContextMenuButton("Pix Config", delegate
			{
				Windows.Get<PixConfigWindow>().Toggle(pix);
			}, closeOnClick: true, (FontAwesomeIcon)61459)
		};
		SyncedPix syncedPix = pix as SyncedPix;
		if (syncedPix == null)
		{
			list.Add(new ContextMenuButton("Copy Pix", delegate
			{
				PixService.CopyPixToClipboard(pix);
			}, closeOnClick: true, (FontAwesomeIcon)61637, null, null, ContextMenuTint.Both, ContextMenuTint.Both, () => ("Copy Pix", "Copy this Pix to your clipboard to share with others.\nFor manual syncing, the receiver can copy the text & use the 'Paste Pix' button.\nNote: This does not copy any private browser data, only the Uri.")));
			list.Add(new ContextMenuButton("Remove Pix", delegate
			{
				if (ImGui.IsKeyDown((ImGuiKey)641) && !PixService.IsSpawned(pix))
				{
					Windows.Get<PixConfigWindow>().Toggle(null);
					PixService.DeleteLocalPix(pix);
				}
			}, closeOnClick: true, (FontAwesomeIcon)61944, null, () => PixService.IsSpawned(pix) || !ImGui.IsKeyDown((ImGuiKey)641), ContextMenuTint.Both, ContextMenuTint.Both, () => (!ImGui.IsKeyDown((ImGuiKey)641) || PixService.IsSpawned(pix)) ? new(string, string)?(("Remove Pix", "Hold the Control key to confirm.\nThe Pix must also not be currently spawned.")) : new(string, string)?(("Remove Pix", null))));
		}
		else
		{
			list.Add(new ContextMenuButton("Copy PixId", delegate
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				ImGui.SetClipboardText(ImU8String.op_Implicit(syncedPix.Id));
			}, closeOnClick: true, (FontAwesomeIcon)61637, null, null, ContextMenuTint.Both, ContextMenuTint.Both, () => ("Copy PixId", "Copy the Id of this synced Pix to your clipboard to share with others.\nThe receiver can copy the Id & join via the Sync Search window.")));
			if (SyncService.IsConnectedAuth)
			{
				list.Add(new ContextMenuButton("Members", delegate
				{
					Windows.Get<PixMembersWindow>().Toggle(syncedPix, syncedPix.SelfRank == PixRank.Owner);
				}, closeOnClick: true, (FontAwesomeIcon)61632));
				if (syncedPix.SelfRank == PixRank.Owner)
				{
					list.Add(new ContextMenuButton("Unsync Pix", delegate
					{
						if (ImGui.IsKeyDown((ImGuiKey)641))
						{
							SyncService.DeleteSyncedPix(syncedPix.Id);
						}
					}, closeOnClick: true, (FontAwesomeIcon)61735, null, () => !ImGui.IsKeyDown((ImGuiKey)641), ContextMenuTint.Both, ContextMenuTint.Both, () => (!ImGui.IsKeyDown((ImGuiKey)641)) ? new(string, string)?(("Unsync Pix", "Remove the Pix from the Sync Service & restore it as a local Pix.\n\nHold the Control key to confirm.")) : new(string, string)?(("Unsync Pix", null))));
				}
				else
				{
					list.Add(new ContextMenuButton("Report", delegate
					{
						if (ImGui.IsKeyDown((ImGuiKey)642))
						{
							SyncService.ReportPix(syncedPix.Id);
						}
					}, closeOnClick: true, (FontAwesomeIcon)61553, null, () => !ImGui.IsKeyDown((ImGuiKey)642), ContextMenuTint.Both, ContextMenuTint.Both, () => (!ImGui.IsKeyDown((ImGuiKey)642)) ? new(string, string)?(("Report Pix", "Report this Pix for service violation.\nFalse reports may have consequences.\n\nHold the Shift key to confirm.")) : new(string, string)?(("Report Pix", null))));
					list.Add(new ContextMenuButton("Leave Pix", delegate
					{
						if (ImGui.IsKeyDown((ImGuiKey)641))
						{
							SyncService.UnsubscribePix(syncedPix.Id);
						}
					}, closeOnClick: true, (FontAwesomeIcon)61735, null, () => !ImGui.IsKeyDown((ImGuiKey)641), ContextMenuTint.Both, ContextMenuTint.Both, () => (!ImGui.IsKeyDown((ImGuiKey)641)) ? new(string, string)?(("Leave Pix", "Hold the Control key to confirm.")) : new(string, string)?(("Leave Pix", null))));
				}
			}
		}
		return new ContextMenu("pixContext", list, 120f, 26f);
	}
}
