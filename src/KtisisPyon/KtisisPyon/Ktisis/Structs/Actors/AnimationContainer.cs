using System.Runtime.InteropServices;

namespace Ktisis.Structs.Actors;

[StructLayout(LayoutKind.Explicit, Size = 832)]
public struct AnimationContainer
{
	public const int TimelineOffset = 16;

	[FieldOffset(0)]
	public unsafe nint** __vfTable;

	[FieldOffset(8)]
	public unsafe CharacterEx* Character;

	[FieldOffset(16)]
	public AnimationTimeline Timeline;
}
