using System.Numerics;

namespace Ktisis.Data.Config.Sections;

public struct Style
{
	public float TranslationLineThickness;

	public float TranslationLineArrowSize;

	public float RotationLineThickness;

	public float RotationOuterLineThickness;

	public float ScaleLineThickness;

	public float ScaleLineCircleSize;

	public float HatchedAxisLineThickness;

	public float CenterCircleSize;

	public Vector4 ColorDirectionX;

	public Vector4 ColorDirectionY;

	public Vector4 ColorDirectionZ;

	public Vector4 ColorPlaneX;

	public Vector4 ColorPlaneY;

	public Vector4 ColorPlaneZ;

	public Vector4 ColorSelection;

	public Vector4 ColorInactive;

	public Vector4 ColorTranslationLine;

	public Vector4 ColorScaleLine;

	public Vector4 ColorRotationUsingBorder;

	public Vector4 ColorRotationUsingFill;

	public Vector4 ColorHatchedAxisLines;

	public Vector4 ColorText;

	public Vector4 ColorTextShadow;
}
