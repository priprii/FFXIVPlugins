using System.Runtime.InteropServices;

namespace Ktisis.Structs.Animation.Clips;

[StructLayout(LayoutKind.Explicit, Size = 208)]
public struct HavokAnimationClip
{
	[FieldOffset(0)]
	public BaseClip Clip;

	[FieldOffset(152)]
	public unsafe MotionControl* MotionControl;

	[FieldOffset(160)]
	public unsafe char* AnimationName;
}
