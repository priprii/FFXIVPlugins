using System.Runtime.InteropServices;
using Ktisis.Structs.Animation.Clips;

namespace Ktisis.Structs.Animation;

[StructLayout(LayoutKind.Explicit, Size = 128)]
public struct MotionControl
{
	[FieldOffset(68)]
	public uint FrameCount;

	[FieldOffset(76)]
	public float StartSpeed;

	[FieldOffset(84)]
	public float PlaySpeed;

	[FieldOffset(96)]
	public unsafe HavokAnimationClip* ParentClip;

	[FieldOffset(120)]
	public unsafe MotionAnimation* Animation;
}
