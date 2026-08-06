using System.Runtime.InteropServices;
using Ktisis.Structs.Animation;

namespace Ktisis.Structs.Actors;

[StructLayout(LayoutKind.Explicit, Size = 496)]
public struct AnimationTimeline
{
	[FieldOffset(0)]
	public unsafe nint** __vfTable;

	[FieldOffset(112)]
	public unsafe fixed ulong SchedulerTimelines[14];

	[FieldOffset(224)]
	public unsafe fixed ushort TimelineIds[14];

	[FieldOffset(252)]
	public unsafe fixed ushort CurrentTimelineIds[14];

	[FieldOffset(280)]
	public unsafe fixed ushort PreviousTimelineIds[14];

	[FieldOffset(340)]
	public unsafe fixed float TimelineSpeeds[14];

	[FieldOffset(396)]
	public unsafe fixed float TimelineWeights[14];

	[FieldOffset(720)]
	public ushort ActionTimelineId;

	public unsafe SchedulerTimeline* GetSchedulerTimeline(int slot)
	{
		ulong num = SchedulerTimelines[slot];
		if (SchedulerTimelines[slot] == 0L)
		{
			return null;
		}
		SchedulerTimeline.Handle* ptr = (SchedulerTimeline.Handle*)num;
		if (ptr->Flags == 0)
		{
			return null;
		}
		return ptr->Data;
	}
}
