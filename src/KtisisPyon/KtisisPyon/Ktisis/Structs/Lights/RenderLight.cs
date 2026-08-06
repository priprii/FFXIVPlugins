using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using Ktisis.Structs.Common;

namespace Ktisis.Structs.Lights;

[StructLayout(LayoutKind.Explicit, Size = 304)]
public struct RenderLight
{
	[FieldOffset(24)]
	public LightFlags Flags;

	[FieldOffset(28)]
	public LightType LightType;

	[FieldOffset(32)]
	public unsafe Transform* Transform;

	[FieldOffset(40)]
	public ColorHDR Color;

	[FieldOffset(56)]
	public Vector3 _unkVec0;

	[FieldOffset(68)]
	public Vector3 _unkVec1;

	[FieldOffset(80)]
	public Vector4 _unkVec2;

	[FieldOffset(96)]
	public float ShadowNear;

	[FieldOffset(100)]
	public float ShadowFar;

	[FieldOffset(104)]
	public FalloffType FalloffType;

	[FieldOffset(112)]
	public Vector2 AreaAngle;

	[FieldOffset(120)]
	public float _unk0;

	[FieldOffset(128)]
	public float Falloff;

	[FieldOffset(132)]
	public float LightAngle;

	[FieldOffset(136)]
	public float FalloffAngle;

	[FieldOffset(140)]
	public float Range;

	[FieldOffset(144)]
	public float CharaShadowRange;

	[FieldOffset(288)]
	public unsafe void* Texture;
}
