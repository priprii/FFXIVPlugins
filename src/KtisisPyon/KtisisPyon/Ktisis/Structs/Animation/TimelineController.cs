using System.Runtime.InteropServices;

namespace Ktisis.Structs.Animation;

[StructLayout(LayoutKind.Explicit, Size = 128)]
public struct TimelineController
{
	[FieldOffset(0)]
	public SchedulerState SchedulerState;

	[FieldOffset(24)]
	public unsafe TrackController* TrackController;

	[FieldOffset(32)]
	public unsafe void* Child;

	[FieldOffset(40)]
	public unsafe byte* Data;

	[FieldOffset(80)]
	public uint QueuedClipCount;

	[FieldOffset(84)]
	public uint Flags;

	[FieldOffset(88)]
	public uint Unk1;

	[FieldOffset(92)]
	public uint Unk2;
}
