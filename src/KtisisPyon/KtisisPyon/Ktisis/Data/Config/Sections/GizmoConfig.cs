using System;
using System.Numerics;
using Dalamud.Bindings.ImGuizmo;
using Ktisis.Editor.Transforms;

namespace Ktisis.Data.Config.Sections;

public class GizmoConfig
{
	public bool Visible = true;

	public ImGuizmoMode Mode;

	public ImGuizmoOperation Operation = (ImGuizmoOperation)120;

	public MirrorMode MirrorRotation;

	public bool ParentBones = true;

	public bool RelativeBones = true;

	public bool AllowAxisFlip = true;

	public bool AllowRaySnap = true;

	public bool AllowHoldSnap = true;

	public float Gizmo2DScaleFactor = 0.5f;

	public Style Style = DefaultStyle;

	public static readonly Style DefaultStyle = new Style
	{
		TranslationLineThickness = 3f,
		TranslationLineArrowSize = 6f,
		RotationLineThickness = 2f,
		RotationOuterLineThickness = 3f,
		ScaleLineThickness = 3f,
		ScaleLineCircleSize = 6f,
		HatchedAxisLineThickness = 6f,
		CenterCircleSize = 6f,
		ColorDirectionX = new Vector4(0.666f, 0f, 0f, 1f),
		ColorDirectionY = new Vector4(0f, 0.666f, 0f, 1f),
		ColorDirectionZ = new Vector4(0f, 0f, 0.666f, 1f),
		ColorPlaneX = new Vector4(0.666f, 0f, 0f, 0.38f),
		ColorPlaneY = new Vector4(0f, 0.666f, 0f, 0.38f),
		ColorPlaneZ = new Vector4(0f, 0f, 0.666f, 0.38f),
		ColorSelection = new Vector4(1f, 0.5f, 0.062f, 0.541f),
		ColorInactive = new Vector4(0.6f, 0.6f, 0.6f, 0.6f),
		ColorTranslationLine = new Vector4(0.666f, 0.666f, 0.666f, 0.666f),
		ColorScaleLine = new Vector4(0.25f, 0.25f, 0.25f, 1f),
		ColorRotationUsingBorder = new Vector4(1f, 0.5f, 0.062f, 1f),
		ColorRotationUsingFill = new Vector4(1f, 0.5f, 0.062f, 0.5f),
		ColorHatchedAxisLines = new Vector4(0f, 0f, 0f, 0.5f),
		ColorText = new Vector4(1f, 1f, 1f, 1f),
		ColorTextShadow = new Vector4(0f, 0f, 0f, 1f)
	};

	public void SetNextMirrorRotation()
	{
		int num = Enum.GetNames(typeof(MirrorMode)).Length;
		MirrorRotation = (MirrorMode)((int)(MirrorRotation + 1) % num);
	}
}
