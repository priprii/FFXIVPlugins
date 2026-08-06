using System.Runtime.InteropServices;

namespace Ktisis.Structs.Animation.Clips;

[StructLayout(LayoutKind.Explicit, Size = 352)]
public struct ChildTimelineClip
{
	[FieldOffset(0)]
	public BaseClip Clip;

	[FieldOffset(204)]
	public float ChildFrame;

	[FieldOffset(208)]
	public float PrevChildFrame;

	[FieldOffset(296)]
	public unsafe SchedulerTimeline* ParentTimeline;

	[FieldOffset(304)]
	public unsafe TimelineController* ChildTimeline;

	[FieldOffset(320)]
	public unsafe byte* Data;
}
