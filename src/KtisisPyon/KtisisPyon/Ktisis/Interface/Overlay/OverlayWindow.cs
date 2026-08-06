using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImGuizmo;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Ktisis.Common.Utility;
using Ktisis.Data.Config.Sections;
using Ktisis.Editor.Actions;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Transforms.Types;
using Ktisis.Interface.Types;
using Ktisis.Services.Game;

namespace Ktisis.Interface.Overlay;

public class OverlayWindow : KtisisWindow
{
	private const ImGuiWindowFlags WindowFlags = (ImGuiWindowFlags)795563;

	private readonly IGameGui _gui;

	private readonly IEditorContext _ctx;

	private readonly IGizmo _gizmo;

	private readonly IGizmo _gizmoGaze;

	public Vector3? GazeTarget;

	public bool GazeManipulated;

	private readonly SceneDraw _sceneDraw;

	private ITransformMemento? Transform;

	public OverlayWindow(IGameGui gui, IEditorContext ctx, IGizmo gizmo, IGizmo gizmoGaze, SceneDraw draw)
		: base("##KtisisOverlay", (ImGuiWindowFlags)795563)
	{
		_gui = gui;
		_ctx = ctx;
		_gizmo = gizmo;
		_gizmoGaze = gizmoGaze;
		_sceneDraw = draw;
		_sceneDraw.SetContext(ctx);
		((Window)this).PositionCondition = (ImGuiCond)1;
	}

	public override void PreOpenCheck()
	{
		if (!_ctx.IsValid)
		{
			Ktisis.Log.Verbose("Context for overlay window is stale, closing...");
			Close();
		}
	}

	public override void PreDraw()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		ImGuiViewportPtr mainViewport = ImGui.GetMainViewport();
		((Window)this).Size = ((ImGuiViewportPtr)(ref mainViewport)).Size;
		mainViewport = ImGui.GetMainViewport();
		((Window)this).Position = ((ImGuiViewportPtr)(ref mainViewport)).Pos;
	}

	public override void Draw()
	{
		_sceneDraw.DrawRefOverlay();
		if (!_ctx.Config.Overlay.Visible)
		{
			CheckResetGizmo();
			return;
		}
		bool gizmo = false;
		if (GazeTarget.HasValue)
		{
			GazeManipulated = DrawGazeGizmo();
		}
		else
		{
			GazeManipulated = false;
			gizmo = DrawGizmo();
		}
		_sceneDraw.DrawScene(gizmo, _gizmo.IsEnded);
	}

	private bool DrawGizmo()
	{
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		if (!_ctx.Config.Gizmo.Visible)
		{
			CheckResetGizmo();
			return false;
		}
		ITransformTarget target = _ctx.Transform.Target;
		Transform transform = target?.GetTransform();
		if (target == null || transform == null)
		{
			return false;
		}
		Matrix4x4? viewMatrix = CameraService.GetViewMatrix();
		Matrix4x4? projectionMatrix = CameraService.GetProjectionMatrix();
		if (!viewMatrix.HasValue || !projectionMatrix.HasValue || !((Window)this).Size.HasValue)
		{
			return false;
		}
		Vector2 value = ((Window)this).Size.Value;
		_gizmo.SetMatrix(viewMatrix.Value, projectionMatrix.Value);
		_gizmo.BeginFrame(((Window)this).Position.Value, value);
		GizmoConfig gizmo = _ctx.Config.Gizmo;
		_gizmo.Mode = gizmo.Mode;
		_gizmo.Operation = gizmo.Operation;
		_gizmo.AllowAxisFlip = gizmo.AllowAxisFlip;
		Matrix4x4 mx = transform.ComposeMatrix();
		Matrix4x4 delta;
		bool num = _gizmo.Manipulate(ref mx, out delta);
		bool flag = HandleShiftRaycast(ref mx);
		if (num || flag)
		{
			if (Transform == null)
			{
				Transform = _ctx.Transform.Begin(target);
			}
			Transform.SetTransform(new Transform(mx, transform));
		}
		_gizmo.EndFrame();
		if (_gizmo.IsEnded)
		{
			Transform?.Dispatch();
			Transform = null;
		}
		else if (_gizmo.IsUsedPrev && Transform != null && !ImGui.IsMouseDown((ImGuiMouseButton)0) && !ImGui.IsWindowHovered())
		{
			Transform?.Dispatch();
			Transform = null;
			_gizmo.Reset();
		}
		return true;
	}

	private bool DrawGazeGizmo()
	{
		if (!_ctx.Config.Overlay.Visible)
		{
			return false;
		}
		if (!GazeTarget.HasValue)
		{
			return false;
		}
		Matrix4x4? viewMatrix = CameraService.GetViewMatrix();
		Matrix4x4? projectionMatrix = CameraService.GetProjectionMatrix();
		if (!viewMatrix.HasValue || !projectionMatrix.HasValue || !((Window)this).Size.HasValue)
		{
			return false;
		}
		Transform transform = new Transform(GazeTarget.Value);
		Matrix4x4 mx = transform.ComposeMatrix();
		GizmoConfig gizmo = _ctx.Config.Gizmo;
		_gizmoGaze.Mode = (ImGuizmoMode)1;
		_gizmoGaze.Operation = (ImGuizmoOperation)7;
		_gizmoGaze.AllowAxisFlip = gizmo.AllowAxisFlip;
		_gizmoGaze.ScaleFactor = 0.075f;
		Vector2 value = ((Window)this).Size.Value;
		_gizmoGaze.SetMatrix(viewMatrix.Value, projectionMatrix.Value);
		_gizmoGaze.BeginFrame(((Window)this).Position.Value, value);
		Matrix4x4 delta;
		bool result = _gizmoGaze.Manipulate(ref mx, out delta);
		transform.DecomposeMatrixPrecise(mx, transform);
		GazeTarget = transform.Position;
		_gizmoGaze.EndFrame();
		return result;
	}

	private bool HandleShiftRaycast(ref Matrix4x4 matrix)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		if (!_ctx.Config.Gizmo.AllowRaySnap)
		{
			return false;
		}
		if (!ImGui.IsKeyDown((ImGuiKey)642) || !ImGuizmo.IsUsing() || (int)_gizmo.Operation != 7)
		{
			return false;
		}
		Vector3 translation = default(Vector3);
		if (!_gui.ScreenToWorld(ImGui.GetMousePos(), ref translation, 100000f))
		{
			return false;
		}
		matrix.Translation = translation;
		return true;
	}

	private void CheckResetGizmo()
	{
		if (_gizmo.IsUsedPrev)
		{
			Transform?.Dispatch();
			Transform = null;
			_gizmo.Reset();
		}
	}

	private void DrawDebugOverlay(Stopwatch? t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SetCursorPosY(((ImGuiStylePtr)(ref style)).WindowPadding.Y);
		for (int i = 0; i < 5; i++)
		{
			ImGui.Spacing();
		}
		DrawDebug(t);
	}

	public void DrawDebug(Stopwatch? t)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(12, 2);
		((ImU8String)(ref val)).AppendLiteral("Context: ");
		((ImU8String)(ref val)).AppendFormatted<int>(_ctx.GetHashCode(), "X");
		((ImU8String)(ref val)).AppendLiteral(" (");
		((ImU8String)(ref val)).AppendFormatted<bool>(_ctx.IsValid);
		((ImU8String)(ref val)).AppendLiteral(")");
		ImGui.Text(val);
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(10, 2);
		((ImU8String)(ref val2)).AppendLiteral("Scene: ");
		((ImU8String)(ref val2)).AppendFormatted<int>(_ctx.Scene.GetHashCode(), "X");
		((ImU8String)(ref val2)).AppendLiteral(" ");
		((ImU8String)(ref val2)).AppendFormatted<double>(_ctx.Scene.UpdateTime, "00.00");
		((ImU8String)(ref val2)).AppendLiteral("ms");
		ImGui.Text(val2);
		if (t != null)
		{
			ImU8String val3 = default(ImU8String);
			((ImU8String)(ref val3))._002Ector(12, 2);
			((ImU8String)(ref val3)).AppendLiteral("Overlay: ");
			((ImU8String)(ref val3)).AppendFormatted<int>(((object)this).GetHashCode());
			((ImU8String)(ref val3)).AppendLiteral(" ");
			((ImU8String)(ref val3)).AppendFormatted<double>(t.Elapsed.TotalMilliseconds, "00.00");
			((ImU8String)(ref val3)).AppendLiteral("ms");
			ImGui.Text(val3);
		}
		ImU8String val4 = default(ImU8String);
		((ImU8String)(ref val4))._002Ector(13, 4);
		((ImU8String)(ref val4)).AppendLiteral("Gizmo: ");
		((ImU8String)(ref val4)).AppendFormatted<int>(_gizmo.GetHashCode(), "X");
		((ImU8String)(ref val4)).AppendLiteral(" ");
		((ImU8String)(ref val4)).AppendFormatted<GizmoId>(_gizmo.Id);
		((ImU8String)(ref val4)).AppendLiteral(" (");
		((ImU8String)(ref val4)).AppendFormatted<ImGuizmoOperation>(_gizmo.Operation);
		((ImU8String)(ref val4)).AppendLiteral(", ");
		((ImU8String)(ref val4)).AppendFormatted<bool>(ImGuizmo.IsUsing());
		((ImU8String)(ref val4)).AppendLiteral(")");
		ImGui.Text(val4);
		ImU8String val5 = default(ImU8String);
		((ImU8String)(ref val5))._002Ector(19, 4);
		((ImU8String)(ref val5)).AppendLiteral("Gaze Gizmo?: ");
		((ImU8String)(ref val5)).AppendFormatted<int>(_gizmoGaze.GetHashCode(), "X");
		((ImU8String)(ref val5)).AppendLiteral(" ");
		((ImU8String)(ref val5)).AppendFormatted<GizmoId>(_gizmoGaze.Id);
		((ImU8String)(ref val5)).AppendLiteral(" (");
		((ImU8String)(ref val5)).AppendFormatted<ImGuizmoOperation>(_gizmoGaze.Operation);
		((ImU8String)(ref val5)).AppendLiteral(", ");
		((ImU8String)(ref val5)).AppendFormatted<bool>(ImGuizmo.IsUsing());
		((ImU8String)(ref val5)).AppendLiteral(")");
		ImGui.Text(val5);
		ITransformTarget target = _ctx.Transform.Target;
		ImU8String val6 = default(ImU8String);
		((ImU8String)(ref val6))._002Ector(14, 4);
		((ImU8String)(ref val6)).AppendLiteral("Target: ");
		((ImU8String)(ref val6)).AppendFormatted<int>(target?.GetHashCode() ?? 0, "X7");
		((ImU8String)(ref val6)).AppendLiteral(" ");
		((ImU8String)(ref val6)).AppendFormatted<string>(target?.GetType().Name ?? "NULL");
		((ImU8String)(ref val6)).AppendLiteral(" (");
		((ImU8String)(ref val6)).AppendFormatted<int>((target?.Targets?.Count()).GetValueOrDefault());
		((ImU8String)(ref val6)).AppendLiteral(", ");
		((ImU8String)(ref val6)).AppendFormatted<string>(target?.Primary?.Name ?? "NULL");
		((ImU8String)(ref val6)).AppendLiteral(")");
		ImGui.Text(val6);
		IHistoryManager history = _ctx.Actions.History;
		ImU8String val7 = default(ImU8String);
		((ImU8String)(ref val7))._002Ector(14, 3);
		((ImU8String)(ref val7)).AppendLiteral("History: ");
		((ImU8String)(ref val7)).AppendFormatted<int>(history.Count);
		((ImU8String)(ref val7)).AppendLiteral(" (");
		((ImU8String)(ref val7)).AppendFormatted<bool>(history.CanUndo);
		((ImU8String)(ref val7)).AppendLiteral(", ");
		((ImU8String)(ref val7)).AppendFormatted<bool>(history.CanRedo);
		((ImU8String)(ref val7)).AppendLiteral(")");
		ImGui.Text(val7);
	}
}
