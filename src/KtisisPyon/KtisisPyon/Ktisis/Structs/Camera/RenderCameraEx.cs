using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;

namespace Ktisis.Structs.Camera;

[StructLayout(LayoutKind.Explicit)]
public struct RenderCameraEx
{
	[FieldOffset(0)]
	public Camera RenderCamera;

	[FieldOffset(492)]
	public float FoV;

	[FieldOffset(496)]
	public float AspectRatio;

	[FieldOffset(508)]
	public float OrthographicZoom;

	[FieldOffset(512)]
	public bool OrthographicEnabled;
}
