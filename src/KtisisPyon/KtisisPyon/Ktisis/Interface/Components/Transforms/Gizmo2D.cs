using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImGuizmo;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Data.Config.Sections;
using Ktisis.Interface.Overlay;

namespace Ktisis.Interface.Components.Transforms;

public class Gizmo2D
{
	private readonly GizmoConfig _cfg;

	private readonly IGizmo Gizmo;

	public ImGuizmoMode Mode
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Gizmo.Mode;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			Gizmo.Mode = value;
		}
	}

	public ImGuizmoOperation Operation
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Gizmo.Operation;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			Gizmo.Operation = value;
		}
	}

	public bool IsEnded => Gizmo.IsEnded;

	public Gizmo2D(GizmoConfig cfg, IGizmo gizmo)
	{
		_cfg = cfg;
		Gizmo = gizmo;
		Gizmo.Operation = (ImGuizmoOperation)120;
		Gizmo.ScaleFactor = _cfg.Gizmo2DScaleFactor;
		Gizmo.AllowAxisFlip = false;
	}

	public void SetLookAt(Vector3 cameraPos, Vector3 targetPos, float fov, float aspect = 1f)
	{
		Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView(fov, 1f, 0.1f, 100f);
		Matrix4x4 view = Matrix4x4.CreateLookAt(cameraPos, targetPos, Vector3.UnitY);
		Gizmo.SetMatrix(view, proj);
	}

	public void Begin(Vector2 rectSize, string? nameAppend = null)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		StyleDisposable val = ImRaii.PushStyle((ImGuiStyleVar)10, Vector2.Zero, true);
		try
		{
			StyleDisposable val2 = ImRaii.PushStyle((ImGuiStyleVar)7, 0f, true);
			try
			{
				rectSize.Y *= _cfg.Gizmo2DScaleFactor;
				Gizmo.ScaleFactor = _cfg.Gizmo2DScaleFactor;
				ImGui.BeginChildFrame((uint)(873568 + Gizmo.Id), rectSize, (ImGuiWindowFlags)24);
				Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
				Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
				ImGuiViewportPtr mainViewport = ImGui.GetMainViewport();
				ImGui.SetNextWindowPos(((ImGuiViewportPtr)(ref mainViewport)).Pos);
				mainViewport = ImGui.GetMainViewport();
				ImGui.SetNextWindowSize(((ImGuiViewportPtr)(ref mainViewport)).Size);
				ImU8String val3 = new ImU8String(9, 1);
				((ImU8String)(ref val3)).AppendLiteral("##Gizmo2D");
				((ImU8String)(ref val3)).AppendFormatted<string>(nameAppend);
				ImGui.Begin(val3, (ImGuiWindowFlags)16777263);
				float num = Math.Min(contentRegionAvail.X, contentRegionAvail.Y);
				Vector2 vector = new Vector2(num, num);
				Vector2 pos = cursorScreenPos + (contentRegionAvail - vector) / 2f;
				Gizmo.BeginFrame(pos, vector);
				Gizmo.PushDrawList();
				DrawGizmoCircle(pos, vector, vector.X, _cfg.Gizmo2DScaleFactor);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static void DrawGizmoCircle(Vector2 pos, Vector2 size, float width, float scaleFactor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		((ImDrawListPtr)(ref windowDrawList)).AddCircleFilled(pos + size / 2f, width * scaleFactor / 2.05f, 3474989088u);
	}

	public bool Manipulate(ref Matrix4x4 matrix, out Matrix4x4 delta)
	{
		return Gizmo.Manipulate(ref matrix, out delta);
	}

	public void End()
	{
		Gizmo.EndFrame();
		ImGui.EndChild();
		ImGui.EndChildFrame();
	}
}
