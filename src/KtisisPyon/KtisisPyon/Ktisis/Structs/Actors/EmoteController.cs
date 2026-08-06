using System.Runtime.InteropServices;

namespace Ktisis.Structs.Actors;

[StructLayout(LayoutKind.Explicit)]
public struct EmoteController
{
	[FieldOffset(32)]
	public PoseModeEnum Mode;

	[FieldOffset(33)]
	public byte Pose;

	[FieldOffset(53)]
	public bool IsForceDefaultPose;

	[FieldOffset(55)]
	public bool IsDrawObjectOffset;
}
