using System;
using System.Numerics;
using PyonPix.Shared.Structs.Renderer;
using SharpDX.Direct3D11;

namespace PyonPix.Structs.Renderer;

public sealed class Renderer(string id) : IDisposable
{
	public readonly string PixId = id;

	public Matrix4x4? ScreenTransform;

	public Vector4 ScreenTint = new Vector4(1f, 1f, 1f, 1f);

	public Vector4 EdgeColour = new Vector4(0.01f, 0.01f, 0.01f, 1f);

	public Vector4 BackColour = new Vector4(0.01f, 0.01f, 0.01f, 1f);

	public Vector4 BorderColour = new Vector4(0f, 0f, 0f, 1f);

	public float BorderWidthH = 0.05f;

	public float BorderWidthV = 0.05f;

	public BorderMode BorderMode;

	public float BorderFeather;

	public float EdgeFeather;

	public RasterizerState? RasterizerState;

	public DepthStencilState? DepthState;

	public float DepthOffset;

	public void Dispose()
	{
		RasterizerState?.Dispose();
		DepthState?.Dispose();
	}
}
