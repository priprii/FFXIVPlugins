using System.Numerics;

namespace PyonPix.Structs.Renderer;

public struct ShaderParams
{
	public Matrix4x4 CameraView;

	public Matrix4x4 CameraProjection;

	public Matrix4x4 ScreenTransform;

	public Vector4 ScreenTint;

	public Vector4 EdgeColour;

	public Vector4 BackColour;

	public Vector4 BorderColour;

	public float BorderWidthH;

	public float BorderWidthV;

	public int BorderMode;

	public float BorderFeather;

	public float EdgeFeather;

	public float DepthOffset;

	private readonly float _pad1;

	private readonly float _pad2;
}
