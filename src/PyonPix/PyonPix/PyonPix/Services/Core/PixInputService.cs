using System;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Common.Math;
using PyonPix.Config;
using PyonPix.Config.Global.Properties;
using PyonPix.Config.Pix;
using PyonPix.Extensions;
using PyonPix.Interop;
using PyonPix.Services.Game;
using PyonPix.Shared.Structs.Browser;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Structs.Browser;
using PyonPix.Structs.Renderer;
using PyonPix.Ui;
using PyonPix.Utility;

namespace PyonPix.Services.Core;

public unsafe class PixInputService(Configuration config, IServiceContext services, IWindowContext windows) : BaseService(config, services, windows)
{
	private bool IsMouseInImGuiPresentationRegion;

	private bool IsImGuiPresentationRegionFocused;

	private bool IsRendererRegionFocused;

	private bool WasGameFocused;

	private unsafe Cursor* FFXIVCursor = Cursor.Instance();

	private bool HWCursorInitialState;

	private bool SWCursorInitialState;

	private uint PreviousCursorId;

	private IPix? RendererHoveredPix;

	private IPix? RendererMouseCapturePix;

	private Vector2 RendererMousePos;

	private BrowserService? BrowserService => Services.Get<BrowserService>();

	private RendererService? RendererService => Services.Get<RendererService>();

	private PixService? PixService => Services.Get<PixService>();

	public unsafe override Task Initialize()
	{
		WindowSubclass.Initialize(Services.PluginInterface.UiBuilder.WindowHandlePtr, WndProcDetour);
		HWCursorInitialState = ((Cursor)FFXIVCursor).UseOsHardwareCursor;
		SWCursorInitialState = ((Cursor)FFXIVCursor).UseSoftwareCursor;
		return Task.CompletedTask;
	}

	public Vector2 TranslatePositionRelativeToImGuiPresentation(IPix p, Vector2 mousePos)
	{
		if (BrowserService == null)
		{
			return Vector2.Zero;
		}
		Vector2 presentationSize = BrowserService.PresentationSize;
		switch (p.Browser.ScaleMode)
		{
		case BrowserScaleMode.GameWindow:
			mousePos *= new Vector2((float)UiUtil.GameWidth / presentationSize.X, (float)UiUtil.GameHeight / presentationSize.Y);
			return mousePos;
		case BrowserScaleMode.CustomScale:
			mousePos *= new Vector2(p.Browser.CustomScale.X / presentationSize.X, p.Browser.CustomScale.Y / presentationSize.Y);
			return mousePos;
		default:
			return mousePos;
		}
	}

	public void HandleImGuiPresentationMouseInput()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		if (BrowserService?.FocusedTab == null || PixService == null || !PixService.SpawnedPixs.TryGetValue(BrowserService.FocusedTab.PixId, out IPix value))
		{
			return;
		}
		ImGuiIOPtr iO = ImGui.GetIO();
		MouseButton mouseButtonsState = BrowserUtil.GetMouseButtonsState(((ImGuiIOPtr)(ref iO)).MouseClicked);
		MouseButton mouseButtonsState2 = BrowserUtil.GetMouseButtonsState(((ImGuiIOPtr)(ref iO)).MouseReleased);
		MouseButton mouseButtonsState3 = BrowserUtil.GetMouseButtonsState(((ImGuiIOPtr)(ref iO)).MouseDoubleClicked);
		bool flag = mouseButtonsState != MouseButton.None || mouseButtonsState2 != MouseButton.None || mouseButtonsState3 != MouseButton.None;
		bool flag2 = ((ImGuiIOPtr)(ref iO)).MouseWheelH != 0f || ((ImGuiIOPtr)(ref iO)).MouseWheel != 0f;
		nint lParam = TranslatePositionRelativeToImGuiPresentation(value, ((ImGuiIOPtr)(ref iO)).MousePos - BrowserService.PresentationPosition).ToLParam();
		ImGui.SetCursorScreenPos(BrowserService.PresentationPosition);
		ImGui.InvisibleButton(ImU8String.op_Implicit("##browserInputHitTest"), BrowserService.PresentationSize, (ImGuiButtonFlags)0);
		if ((mouseButtonsState.HasFlag(MouseButton.Left) || mouseButtonsState.HasFlag(MouseButton.Right) || mouseButtonsState.HasFlag(MouseButton.Middle)) && IsMouseInImGuiPresentationRegion && !IsImGuiPresentationRegionFocused)
		{
			IsImGuiPresentationRegionFocused = true;
		}
		if (!WasGameFocused && Win32Interop.IsGameFocused && ImGui.IsItemHovered())
		{
			IsImGuiPresentationRegionFocused = true;
		}
		if ((!ImGui.IsWindowFocused() || !Win32Interop.IsGameFocused) && IsImGuiPresentationRegionFocused)
		{
			IsImGuiPresentationRegionFocused = false;
			BrowserService.LostFocus();
		}
		WasGameFocused = Win32Interop.IsGameFocused;
		if (!ImGui.IsItemHovered())
		{
			if (IsMouseInImGuiPresentationRegion)
			{
				IsMouseInImGuiPresentationRegion = false;
				BrowserService.SendMouseEvent(value.Id, 675u, 0, lParam);
			}
			return;
		}
		if (!Win32Interop.IsGameFocused)
		{
			IsMouseInImGuiPresentationRegion = false;
			return;
		}
		IsMouseInImGuiPresentationRegion = true;
		ImGui.SetMouseCursor(BrowserUtil.TranslateCursor(BrowserService.CursorId));
		if (!(((ImGuiIOPtr)(ref iO)).MouseDelta == Vector2.Zero) || flag || flag2)
		{
			if (!flag && !flag2)
			{
				BrowserService.SendMouseEvent(value.Id, 512u, 0, lParam);
			}
			if (mouseButtonsState.HasFlag(MouseButton.Left))
			{
				BrowserService.SendMouseEvent(value.Id, 513u, 1, lParam);
			}
			if (mouseButtonsState2.HasFlag(MouseButton.Left))
			{
				BrowserService.SendMouseEvent(value.Id, 514u, 0, lParam);
			}
			if (mouseButtonsState.HasFlag(MouseButton.Right))
			{
				BrowserService.SendMouseEvent(value.Id, 516u, 1, lParam);
			}
			if (mouseButtonsState2.HasFlag(MouseButton.Right))
			{
				BrowserService.SendMouseEvent(value.Id, 517u, 0, lParam);
			}
			if (mouseButtonsState.HasFlag(MouseButton.Middle))
			{
				BrowserService.SendMouseEvent(value.Id, 519u, 1, lParam);
			}
			if (mouseButtonsState2.HasFlag(MouseButton.Middle))
			{
				BrowserService.SendMouseEvent(value.Id, 520u, 0, lParam);
			}
			if (((ImGuiIOPtr)(ref iO)).MouseWheel != 0f)
			{
				BrowserService.SendMouseEvent(value.Id, 522u, (int)(((ImGuiIOPtr)(ref iO)).MouseWheel * 120f) << 16, lParam);
			}
		}
	}

	public void ClearImGuiPresentationFocus()
	{
		IsMouseInImGuiPresentationRegion = false;
		IsImGuiPresentationRegionFocused = false;
	}

	public Vector2 TranslatePositionRelativeToRenderer(IPix pix, Vector2 uv)
	{
		if (BrowserService == null || !BrowserService.TryGetRenderBounds(pix, out var _, out var size))
		{
			return Vector2.Zero;
		}
		return uv * size;
	}

	public unsafe void HandleRendererMouseInput(Renderer? renderer, Tab? tab)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		if (BrowserService == null || PixService == null || renderer == null || tab == null || !PixService.SpawnedPixs.TryGetValue(tab.PixId, out IPix value) || !renderer.ScreenTransform.HasValue || !Matrix4x4.Invert(renderer.ScreenTransform.Value, out var result))
		{
			return;
		}
		Camera* sceneCamera = CameraService.GetSceneCamera();
		if (sceneCamera == null)
		{
			return;
		}
		BrowserGlobalProperties browser = Config.Global.Browser;
		if (!browser.ScreenInteractionCaptureLButton && !browser.ScreenInteractionCaptureRButton && !browser.ScreenInteractionCaptureMButton && !browser.ScreenInteractionCaptureScroll)
		{
			return;
		}
		PixVariant variant = PixService.GetVariant(value);
		if (variant != null && !variant.ScreenInteraction)
		{
			return;
		}
		ImGuiIOPtr iO = ImGui.GetIO();
		Ray val = ((Camera)sceneCamera).ScreenPointToRay(new Vector2((float)(int)((ImGuiIOPtr)(ref iO)).MousePos.X, (float)(int)((ImGuiIOPtr)(ref iO)).MousePos.Y));
		Vector3 vector = Vector3.Transform(Vector3.op_Implicit(val.Origin), result);
		Vector3 vector2 = Vector3.Normalize(Vector3.TransformNormal(Vector3.op_Implicit(val.Direction), result));
		if (MathF.Abs(vector2.Z) < 0.0001f || (browser.ScreenInteractionFrontFace && Vector3.Dot(vector2, Vector3.UnitZ) >= 0f) || (browser.ScreenInteractionFrontFace && Vector3.Dot(vector, Vector3.UnitZ) <= 0f))
		{
			return;
		}
		float num = (0f - vector.Z) / vector2.Z;
		if (num < 0f)
		{
			return;
		}
		Vector3 vector3 = vector + vector2 * num;
		bool num2 = vector3.X >= -0.5f && vector3.X <= 0.5f && vector3.Y >= -0.5f && vector3.Y <= 0.5f;
		float x = Math.Clamp(vector3.X + 0.5f, 0f, 1f);
		Vector2 vector4 = TranslatePositionRelativeToRenderer(uv: new Vector2(x, Math.Clamp(0.5f - vector3.Y, 0f, 1f)), pix: value);
		nint lParam = vector4.ToLParam();
		if (num2)
		{
			ImGuiIOPtr iO2 = ImGui.GetIO();
			if (!((ImGuiIOPtr)(ref iO2)).WantCaptureMouse)
			{
				if (!Win32Interop.IsGameFocused)
				{
					if (RendererHoveredPix == value)
					{
						RendererHoveredPix = null;
						if (IsRendererRegionFocused)
						{
							IsRendererRegionFocused = false;
							BrowserService.LostFocus();
						}
						ResetCursor(force: true);
					}
				}
				else
				{
					RendererHoveredPix = value;
					RendererMousePos = vector4;
					ChangeCursor(BrowserService.CursorId);
				}
				return;
			}
		}
		if (RendererHoveredPix == value)
		{
			RendererHoveredPix = null;
			BrowserService.SendMouseEvent(value.Id, 675u, 0, lParam);
			ResetCursor();
		}
	}

	public override void Update()
	{
		if (PreviousCursorId != 0)
		{
			if (!Config.Global.Browser.ScreenInteractionCursorChanges || !IsRendererRegionFocused)
			{
				ResetCursor();
			}
			else
			{
				Win32Interop.SetOSCursor(PreviousCursorId);
			}
		}
	}

	private unsafe void ChangeCursor(uint cursorId)
	{
		if (Config.Global.Browser.ScreenInteractionCursorChanges && IsRendererRegionFocused)
		{
			((Cursor)FFXIVCursor).UseOsHardwareCursor = true;
			((Cursor)FFXIVCursor).UseSoftwareCursor = false;
			PreviousCursorId = cursorId;
		}
	}

	private unsafe void ResetCursor(bool force = false)
	{
		if (PreviousCursorId != 0 || force)
		{
			((Cursor)FFXIVCursor).UseOsHardwareCursor = HWCursorInitialState;
			((Cursor)FFXIVCursor).UseSoftwareCursor = SWCursorInitialState;
			PreviousCursorId = 0u;
		}
	}

	private bool HandleRendererMouseEvent(Win32Interop.WM msg, ulong wParam, long lParam)
	{
		if (BrowserService == null || PixService == null || !Win32Interop.IsGameFocused)
		{
			return false;
		}
		BrowserGlobalProperties browser = Config.Global.Browser;
		switch (msg)
		{
		case Win32Interop.WM.LBUTTONDOWN:
		case Win32Interop.WM.RBUTTONDOWN:
		case Win32Interop.WM.MBUTTONDOWN:
		{
			if (RendererHoveredPix == null)
			{
				if (IsRendererRegionFocused)
				{
					IsRendererRegionFocused = false;
					BrowserService.LostFocus();
					ResetCursor(force: true);
				}
				return false;
			}
			if (msg == Win32Interop.WM.LBUTTONDOWN && !browser.ScreenInteractionCaptureLButton)
			{
				return false;
			}
			if (msg == Win32Interop.WM.RBUTTONDOWN && !browser.ScreenInteractionCaptureRButton)
			{
				return false;
			}
			if (msg == Win32Interop.WM.MBUTTONDOWN && !browser.ScreenInteractionCaptureMButton)
			{
				return false;
			}
			if (!IsRendererRegionFocused)
			{
				if (browser.ScreenInteractionReqCtrl && !Win32Interop.IsCtrlDown())
				{
					return false;
				}
				if (browser.ScreenInteractionReqShift && !Win32Interop.IsShiftDown())
				{
					return false;
				}
			}
			PixVariant variant2 = PixService.GetVariant(RendererHoveredPix);
			if (variant2 != null && !variant2.ScreenInteraction)
			{
				return false;
			}
			RendererMouseCapturePix = RendererHoveredPix;
			IsRendererRegionFocused = true;
			ReleaseModifierKeys();
			BrowserService.FocusTab(RendererMouseCapturePix.Id);
			BrowserService.SendMouseEvent(RendererMouseCapturePix, (uint)msg, (nint)wParam, RendererMousePos.ToLParam());
			return true;
		}
		case Win32Interop.WM.LBUTTONUP:
		case Win32Interop.WM.RBUTTONUP:
		case Win32Interop.WM.MBUTTONUP:
			if (RendererMouseCapturePix == null)
			{
				return false;
			}
			if (msg == Win32Interop.WM.LBUTTONDOWN && !browser.ScreenInteractionCaptureLButton)
			{
				return false;
			}
			if (msg == Win32Interop.WM.RBUTTONDOWN && !browser.ScreenInteractionCaptureRButton)
			{
				return false;
			}
			if (msg == Win32Interop.WM.MBUTTONDOWN && !browser.ScreenInteractionCaptureMButton)
			{
				return false;
			}
			BrowserService.SendMouseEvent(RendererMouseCapturePix, (uint)msg, (nint)wParam, RendererMousePos.ToLParam());
			RendererMouseCapturePix = null;
			return true;
		case Win32Interop.WM.MOUSEWHEEL:
		{
			if (!browser.ScreenInteractionCaptureScroll)
			{
				return false;
			}
			if (RendererHoveredPix == null)
			{
				return false;
			}
			PixVariant variant3 = PixService.GetVariant(RendererHoveredPix);
			if (variant3 != null && !variant3.ScreenInteraction)
			{
				return false;
			}
			BrowserService.SendMouseEvent(RendererHoveredPix, (uint)msg, (nint)wParam, (nint)lParam);
			return true;
		}
		case Win32Interop.WM.MOUSEFIRST:
		{
			if (!browser.ScreenInteractionCaptureLButton && !browser.ScreenInteractionCaptureRButton && !browser.ScreenInteractionCaptureMButton && !browser.ScreenInteractionCaptureScroll)
			{
				return false;
			}
			if (RendererHoveredPix == null)
			{
				return false;
			}
			PixVariant variant = PixService.GetVariant(RendererHoveredPix);
			if (variant != null && !variant.ScreenInteraction)
			{
				return false;
			}
			BrowserService.SendMouseEvent(RendererHoveredPix, 512u, (nint)wParam, RendererMousePos.ToLParam());
			return true;
		}
		default:
			return false;
		}
	}

	private nint WndProcDetour(nint hwnd, uint msg, nuint wParam, nint lParam)
	{
		if (hwnd == WindowSubclass.Hwnd)
		{
			BrowserService? browserService = BrowserService;
			if (browserService != null && browserService.State == BrowserState.Running)
			{
				switch ((Win32Interop.WM)msg)
				{
				case Win32Interop.WM.ENTERSIZEMOVE:
					BrowserService.IsResizing = true;
					break;
				case Win32Interop.WM.EXITSIZEMOVE:
					BrowserService.IsResizing = false;
					break;
				case Win32Interop.WM.LBUTTONDOWN:
				case Win32Interop.WM.RBUTTONDOWN:
				case Win32Interop.WM.MBUTTONDOWN:
					if (HandleRendererMouseEvent((Win32Interop.WM)msg, wParam, lParam))
					{
						return 0;
					}
					break;
				case Win32Interop.WM.LBUTTONUP:
				case Win32Interop.WM.RBUTTONUP:
				case Win32Interop.WM.MBUTTONUP:
					if (HandleRendererMouseEvent((Win32Interop.WM)msg, wParam, lParam))
					{
						return 0;
					}
					break;
				case Win32Interop.WM.MOUSEWHEEL:
					if (HandleRendererMouseEvent((Win32Interop.WM)msg, wParam, lParam))
					{
						return 0;
					}
					break;
				case Win32Interop.WM.MOUSEFIRST:
					HandleRendererMouseEvent((Win32Interop.WM)msg, wParam, lParam);
					break;
				}
			}
		}
		return WindowSubclass.CallOriginal(hwnd, msg, wParam, lParam);
	}

	private void ReleaseModifierKeys()
	{
		if (Win32Interop.IsKeyDown(162) || Win32Interop.IsKeyDown(163))
		{
			WindowSubclass.CallOriginal(WindowSubclass.Hwnd, 257u, 17u, 0);
		}
		if (Win32Interop.IsKeyDown(160) || Win32Interop.IsKeyDown(161))
		{
			WindowSubclass.CallOriginal(WindowSubclass.Hwnd, 257u, 16u, 0);
		}
	}

	public override Task Dispose()
	{
		ResetCursor();
		WindowSubclass.Dispose();
		return Task.CompletedTask;
	}
}
