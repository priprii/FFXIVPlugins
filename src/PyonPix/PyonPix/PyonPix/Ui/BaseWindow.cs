using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using PyonPix.Config;
using PyonPix.Extensions;
using PyonPix.Services;
using PyonPix.Shared.Sync.Dto.Client;
using PyonPix.Ui.Components;
using PyonPix.Utility;

namespace PyonPix.Ui;

public abstract class BaseWindow : Window, IDisposable
{
	protected enum WindowState
	{
		Expanded,
		Collapsed
	}

	protected readonly Configuration Config;

	protected readonly IServiceContext Services;

	protected readonly IWindowContext Windows;

	private WindowState _lastState;

	private bool _initialized;

	protected StatusBar StatusBar = new StatusBar();

	protected virtual bool ShowTitleBar => true;

	protected virtual bool ShowTitleBarCollapseButton => true;

	protected virtual bool ShowTitleBarTitleText => true;

	protected virtual bool ShowTitleBarSettingsButton => true;

	protected virtual bool ShowTitleBarCloseButton => true;

	protected virtual bool NoResize => false;

	protected virtual float TitleBarHeight => 24f;

	protected virtual float TitleBarXPadding => 2f;

	protected virtual float BorderThickness => 1.5f;

	protected virtual float BorderInset => 0.5f;

	public float BorderSize => BorderThickness + BorderInset;

	protected virtual Vector2 WindowPadding => UIShared.WindowPadding;

	protected virtual float LineHeight => UIShared.LineHeight;

	protected virtual float ItemSpacing => UIShared.ItemSpacing;

	protected virtual float IndentWidth => UIShared.IndentWidth;

	protected virtual ImGuiWindowFlags BaseFlags => (ImGuiWindowFlags)2097321;

	protected abstract WindowState State { get; }

	public bool IsCollapsed
	{
		get
		{
			if (State == WindowState.Collapsed && ShowTitleBar)
			{
				return ShowTitleBarCollapseButton;
			}
			return false;
		}
	}

	public bool IsHidden
	{
		get
		{
			if (((Window)this).IsOpen)
			{
				return IsCollapsed;
			}
			return true;
		}
	}

	protected virtual Vector2 ExpandedSize => ExpandedMinSize;

	protected abstract Vector2 ExpandedMinSize { get; }

	protected abstract Vector2 ExpandedMaxSize { get; }

	public float CollapsedHeight => TitleBarHeight + BorderSize * 2f;

	public Vector2 BoundsMin => ImGui.GetWindowPos() + new Vector2(BorderSize * ImGuiHelpers.GlobalScale);

	public Vector2 BoundsMax => ImGui.GetWindowPos() + ImGui.GetWindowSize() - new Vector2(BorderSize * ImGuiHelpers.GlobalScale);

	public Vector2 HeaderMin => BoundsMin;

	public Vector2 HeaderMax => new Vector2(BoundsMax.X, BoundsMin.Y + TitleBarHeight * ImGuiHelpers.GlobalScale);

	public Vector2 ContentMin
	{
		get
		{
			if (!ShowTitleBar)
			{
				return BoundsMin;
			}
			return new Vector2(BoundsMin.X, HeaderMax.Y + BorderSize * ImGuiHelpers.GlobalScale);
		}
	}

	public Vector2 ContentMax => BoundsMax;

	public Vector2 ContentSize => ContentMax - ContentMin;

	public float TitleBarFrameHeight => HeaderMax.Y - HeaderMin.Y;

	public BaseWindow(string name, Configuration config, IServiceContext services, IWindowContext windows, ImGuiWindowFlags flags = (ImGuiWindowFlags)0)
		: base(name)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Config = config;
		Services = services;
		Windows = windows;
		((Window)this).Flags = (ImGuiWindowFlags)(((int)flags == 0) ? BaseFlags : (BaseFlags | flags));
	}

	public override void PreDraw()
	{
		ImGui.PushStyleVar((ImGuiStyleVar)4, new Vector2(1f));
		ImGui.PushStyleVar((ImGuiStyleVar)1, Vector2.Zero);
		ImGui.PushStyleVar((ImGuiStyleVar)3, 0f);
		ImGui.PushStyleVar((ImGuiStyleVar)2, 0f);
		ImGui.PushStyleVar((ImGuiStyleVar)18, 4f * ImGuiHelpers.GlobalScale);
		ImGui.PushStyleVar((ImGuiStyleVar)17, 10f * ImGuiHelpers.GlobalScale);
		ImGui.PushStyleColor((ImGuiCol)30, Vector4.Zero);
		ImGui.PushStyleColor((ImGuiCol)31, Vector4.Zero);
		ImGui.PushStyleColor((ImGuiCol)32, Vector4.Zero);
		((Window)this).PreDraw();
	}

	public override void PostDraw()
	{
		ImGui.PopStyleColor(3);
		ImGui.PopStyleVar(6);
		((Window)this).PostDraw();
	}

	public override void Draw()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		if (!((Window)this).IsOpen)
		{
			return;
		}
		Vector2 windowPos = ImGui.GetWindowPos();
		if (windowPos.Y < 0f)
		{
			ImGui.SetWindowPos(new Vector2(windowPos.X, 0f), (ImGuiCond)0);
		}
		UpdateWindowState();
		((Window)this).SizeConstraints = GetConstraints();
		DrawWindowBackground();
		if (ShowTitleBar)
		{
			DrawTitleBarBackground();
			float leftCursor = DrawTitleBarCollapse();
			float rightCursor = DrawTitleBarControls();
			rightCursor = DrawControlExtras(rightCursor);
			if (ShowTitleBarTitleText)
			{
				DrawTitleBarText(leftCursor, rightCursor);
			}
		}
		ImGui.SetCursorScreenPos(ContentMin);
		bool num = !IsHidden && (StatusBar?.IsVisible ?? false);
		Vector2 contentSize = ContentSize;
		if (num && !StatusBar.IsOverlay)
		{
			contentSize -= new Vector2(0f, StatusBar.Height);
		}
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(13, 0);
		((ImU8String)(ref val)).AppendLiteral("##rootContent");
		ImGui.BeginChild(val, contentSize, false, (ImGuiWindowFlags)24);
		DrawContent();
		ImGui.EndChild();
		if (num)
		{
			StatusBar.Draw(BoundsMin, BoundsMax);
		}
	}

	protected virtual void DrawWindowBackground()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		float num = BorderThickness * ImGuiHelpers.GlobalScale;
		float num2 = BorderThickness * BorderInset * ImGuiHelpers.GlobalScale;
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 windowPos = ImGui.GetWindowPos();
		Vector2 windowSize = ImGui.GetWindowSize();
		Vector2 vector = windowPos + new Vector2(num2, num2);
		Vector2 vector2 = windowPos + windowSize - new Vector2(num2, num2);
		((ImDrawListPtr)(ref windowDrawList)).AddImageRounded(UIShared.GradientTexture.Handle, windowPos, windowPos + windowSize, new Vector2(0f, 0f), new Vector2(1f, 1f), ImGui.GetColorU32(UIShared.WindowBgTint), UIShared.WindowRounding);
		((ImDrawListPtr)(ref windowDrawList)).AddRect(vector, vector2, ImGui.GetColorU32(UIShared.WindowBorder), UIShared.WindowRounding - BorderThickness * BorderInset, (ImDrawFlags)0, num);
	}

	protected virtual void DrawTitleBarBackground()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(HeaderMin, HeaderMax, ImGui.GetColorU32(UIShared.TitleBarBg), UIShared.WindowRounding * 0.5f, (ImDrawFlags)(IsCollapsed ? 240 : 48));
	}

	protected virtual float DrawTitleBarCollapse()
	{
		float num = HeaderMin.X + TitleBarXPadding * ImGuiHelpers.GlobalScale;
		if (ShowTitleBarCollapseButton)
		{
			ImGui.SetCursorScreenPos(new Vector2(num, HeaderMin.Y));
			if (ImGuiEx.IconButton((FontAwesomeIcon)(IsCollapsed ? 61658 : 61655), "##collapse", disabled: false, null, null, TitleBarFrameHeight))
			{
				SetState((!IsCollapsed) ? WindowState.Collapsed : WindowState.Expanded);
			}
			num += TitleBarFrameHeight;
		}
		return num;
	}

	protected virtual float DrawTitleBarControls()
	{
		float num = HeaderMax.X - TitleBarXPadding * ImGuiHelpers.GlobalScale;
		if (ShowTitleBarCloseButton)
		{
			num -= TitleBarFrameHeight;
			ImGui.SetCursorScreenPos(new Vector2(num, HeaderMin.Y));
			if (ImGuiEx.IconButton((FontAwesomeIcon)61453, "##close", disabled: false, null, null, TitleBarFrameHeight, 0.8f))
			{
				OnCloseClicked();
			}
		}
		if (ShowTitleBarSettingsButton)
		{
			num -= TitleBarFrameHeight;
			ImGui.SetCursorScreenPos(new Vector2(num, HeaderMin.Y));
			if (ImGuiEx.IconButton((FontAwesomeIcon)61459, "##settings", disabled: false, null, null, TitleBarFrameHeight, 0.8f))
			{
				OnConfigClicked();
			}
		}
		return num;
	}

	protected virtual void DrawTitleBarText(float leftCursor, float rightCursor)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		using (UIShared.NormalFont.Push())
		{
			ImGui.PushClipRect(new Vector2(leftCursor, HeaderMin.Y), new Vector2(rightCursor, HeaderMax.Y), true);
			float fontSize = 16f;
			Vector2 vector = UiUtil.CalcTextSize(((Window)this).WindowName, fontSize);
			float y = HeaderMin.Y + (TitleBarFrameHeight - vector.Y) * 0.5f;
			ImGui.SetCursorScreenPos(new Vector2(leftCursor, y));
			ImU8String text = ImU8String.op_Implicit(((Window)this).WindowName);
			Vector3? colorA = UIShared.WindowTitle.AsVector3();
			float? wrapWidth = vector.X;
			ImGuiEx.StyledText(text, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, wrapWidth);
			ImGui.PopClipRect();
		}
	}

	protected virtual float DrawControlExtras(float rightCursor)
	{
		return rightCursor;
	}

	protected virtual void DrawContent()
	{
	}

	protected void UpdateWindowState()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		((Window)this).Flags = BaseFlags;
		if (!_initialized)
		{
			_lastState = State;
			_initialized = true;
			return;
		}
		if (IsCollapsed || NoResize)
		{
			((Window)this).Flags = (ImGuiWindowFlags)(((Window)this).Flags | 2);
		}
		if (_lastState != State)
		{
			if (IsCollapsed)
			{
				Vector2 windowSize = ImGui.GetWindowSize();
				ImGui.SetWindowSize(new Vector2(windowSize.X, CollapsedHeight), (ImGuiCond)0);
				OnCollapsed(windowSize);
			}
			else
			{
				ImGui.SetWindowSize(ExpandedSize, (ImGuiCond)0);
			}
			_lastState = State;
		}
	}

	protected virtual void OnCollapsed(Vector2 windowSize)
	{
	}

	protected virtual void SetState(WindowState newState)
	{
	}

	protected virtual void OnConfigClicked()
	{
	}

	protected virtual void OnCloseClicked()
	{
	}

	protected WindowSizeConstraints GetConstraints()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		WindowSizeConstraints result = default(WindowSizeConstraints);
		if (IsCollapsed)
		{
			((WindowSizeConstraints)(ref result))._002Ector();
			((WindowSizeConstraints)(ref result)).MinimumSize = new Vector2(ExpandedMinSize.X, CollapsedHeight);
			((WindowSizeConstraints)(ref result)).MaximumSize = new Vector2(ExpandedMaxSize.X, CollapsedHeight);
			return result;
		}
		((WindowSizeConstraints)(ref result))._002Ector();
		((WindowSizeConstraints)(ref result)).MinimumSize = ExpandedMinSize;
		float x = ExpandedMaxSize.X;
		ImGuiViewportPtr mainViewport = ImGui.GetMainViewport();
		float x2;
		if (!(x < ((ImGuiViewportPtr)(ref mainViewport)).Size.X))
		{
			mainViewport = ImGui.GetMainViewport();
			x2 = ((ImGuiViewportPtr)(ref mainViewport)).Size.X;
		}
		else
		{
			x2 = ExpandedMaxSize.X;
		}
		float y = ExpandedMaxSize.Y;
		mainViewport = ImGui.GetMainViewport();
		float y2;
		if (!(y < ((ImGuiViewportPtr)(ref mainViewport)).Size.Y))
		{
			mainViewport = ImGui.GetMainViewport();
			y2 = ((ImGuiViewportPtr)(ref mainViewport)).Size.Y;
		}
		else
		{
			y2 = ExpandedMaxSize.Y;
		}
		((WindowSizeConstraints)(ref result)).MaximumSize = new Vector2(x2, y2);
		return result;
	}

	public virtual void Dispose()
	{
	}
}
