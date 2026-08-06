using System.Numerics;
using Dalamud.Bindings.ImGuizmo;

namespace Ktisis.Interface.Overlay;

public interface IGizmo
{
	GizmoId Id { get; }

	bool IsUsedPrev { get; }

	float ScaleFactor { get; set; }

	ImGuizmoMode Mode { get; set; }

	ImGuizmoOperation Operation { get; set; }

	bool AllowAxisFlip { get; set; }

	bool IsEnded { get; }

	void SetMatrix(Matrix4x4 view, Matrix4x4 proj);

	void BeginFrame(Vector2 pos, Vector2 size);

	void PushDrawList();

	bool Manipulate(ref Matrix4x4 mx, out Matrix4x4 delta);

	void EndFrame();

	void Reset();
}
