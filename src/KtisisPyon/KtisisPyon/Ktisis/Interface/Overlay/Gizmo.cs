using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImGuizmo;
using Ktisis.Data.Config.Sections;

namespace Ktisis.Interface.Overlay;

public class Gizmo : IGizmo
{
	private readonly GizmoConfig _cfg;

	private bool HasDrawn;

	private Matrix4x4 ViewMatrix = Matrix4x4.Identity;

	private Matrix4x4 ProjMatrix = Matrix4x4.Identity;

	public GizmoId Id { get; }

	public float ScaleFactor { get; set; } = 0.1f;

	public bool IsUsedPrev { get; private set; }

	public ImGuizmoMode Mode { get; set; }

	public ImGuizmoOperation Operation { get; set; } = (ImGuizmoOperation)14463;

	public bool AllowAxisFlip { get; set; } = true;

	public bool IsEnded { get; private set; }

	public Gizmo(GizmoConfig cfg, GizmoId id)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		_cfg = cfg;
		Id = id;
	}

	public void SetMatrix(Matrix4x4 view, Matrix4x4 proj)
	{
		ViewMatrix = view;
		ProjMatrix = proj;
	}

	public void BeginFrame(Vector2 pos, Vector2 size)
	{
		HasDrawn = false;
		ImGuizmo.SetRect(pos.X, pos.Y, size.X, size.Y);
		ImGuizmo.SetID((int)Id);
		ImGuizmo.SetGizmoSizeClipSpace(ScaleFactor);
		ImGuizmo.AllowAxisFlip(AllowAxisFlip);
		ImGuizmo.BeginFrame();
		IsUsedPrev = ImGuizmo.IsUsing();
	}

	public unsafe void PushDrawList()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		ImGuizmo.SetDrawlist(ImDrawListPtr.op_Implicit(ImGui.GetWindowDrawList().Handle));
	}

	public unsafe bool Manipulate(ref Matrix4x4 mx, out Matrix4x4 delta)
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		delta = Matrix4x4.Identity;
		if (HasDrawn)
		{
			return false;
		}
		bool flag = false;
		if (_cfg.AllowHoldSnap && ImGui.IsKeyDown((ImGuiKey)641))
		{
			Vector3 one = Vector3.One;
			ImGuizmoOperation operation = Operation;
			if (((int)operation == 56 || (int)operation == 120) ? true : false)
			{
				one *= 5f;
			}
			else
			{
				one /= 10f;
			}
			if (ImGui.IsKeyDown((ImGuiKey)642))
			{
				one /= 10f;
			}
			flag = ImGuizmo.Manipulate(ref ViewMatrix.M11, ref ProjMatrix.M11, Operation, Mode, ref mx.M11, ref delta.M11, &one.X);
		}
		else
		{
			flag = ImGuizmo.Manipulate(ref ViewMatrix, ref ProjMatrix, Operation, Mode, ref mx, ref delta);
		}
		HasDrawn = true;
		return flag;
	}

	public void EndFrame()
	{
		IsEnded = !ImGuizmo.IsUsing() && IsUsedPrev;
		ImGuizmo.SetGizmoSizeClipSpace(0.1f);
	}

	public void Reset()
	{
		ImGuizmo.Enable(false);
		ImGuizmo.Enable(true);
		IsEnded = true;
	}
}
