using System.Runtime.InteropServices;
using Ktisis.Structs.Common;

namespace Ktisis.Structs.Animation;

[StructLayout(LayoutKind.Explicit, Size = 2680)]
public struct TimelineGroup
{
	[FieldOffset(0)]
	public unsafe nint* __vfTable;

	[FieldOffset(24)]
	public unsafe SchedulerTimeline* SchedulerTimeline;

	[FieldOffset(32)]
	public unsafe void* Controller;

	[FieldOffset(40)]
	public ObjectUnion Object;

	[FieldOffset(2668)]
	public uint GroupType;
}
