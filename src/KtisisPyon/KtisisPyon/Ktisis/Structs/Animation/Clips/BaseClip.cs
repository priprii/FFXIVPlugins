using System.Runtime.InteropServices;

namespace Ktisis.Structs.Animation.Clips;

[StructLayout(LayoutKind.Explicit, Size = 152)]
public struct BaseClip
{
	[FieldOffset(0)]
	public unsafe nint* __vfTable;

	[FieldOffset(0)]
	public SchedulerState SchedulerState;

	[FieldOffset(40)]
	public unsafe TrackController* TrackController;

	[FieldOffset(48)]
	public unsafe TimelineController* ParentTimeline;

	[FieldOffset(56)]
	public unsafe TimelineController* RootTimeline;

	[FieldOffset(72)]
	public unsafe byte* Data;

	[FieldOffset(80)]
	public float TrackStartFrame;

	[FieldOffset(84)]
	public float TrackTotalFrames;

	[FieldOffset(92)]
	public float DeltaFrames;

	[FieldOffset(100)]
	public float ClipStartFrame;

	[FieldOffset(104)]
	public float ClipTotalFrames;

	[FieldOffset(132)]
	public ClipType ClipType;
}
