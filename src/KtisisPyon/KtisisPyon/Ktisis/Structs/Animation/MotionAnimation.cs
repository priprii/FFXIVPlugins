using System.Runtime.InteropServices;
using Ktisis.Structs.Animation.Clips;

namespace Ktisis.Structs.Animation;

[StructLayout(LayoutKind.Explicit, Size = 96)]
public struct MotionAnimation
{
	[FieldOffset(0)]
	public unsafe nint* __vfTable;

	[FieldOffset(32)]
	public unsafe AnimationControl.Handle* AnimationControls;

	[FieldOffset(40)]
	public unsafe MotionControl* ParentControl;

	[FieldOffset(48)]
	public unsafe HavokAnimationClip* ParentClip;
}
