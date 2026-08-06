using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.System.Scheduler.Resource;
using Ktisis.Structs.Common;

namespace Ktisis.Structs.Animation;

[StructLayout(LayoutKind.Explicit, Size = 628)]
public struct SchedulerTimeline
{
	[StructLayout(LayoutKind.Sequential, Size = 16)]
	public struct Handle
	{
		public unsafe SchedulerTimeline* Data;

		public uint Flags;
	}

	[FieldOffset(0)]
	public TimelineController Controller;

	[FieldOffset(144)]
	public unsafe TimelineGroup* TimelineGroup;

	[FieldOffset(152)]
	public unsafe SchedulerResource* SchedulerResource;

	[FieldOffset(168)]
	public unsafe char* FilePath1;

	[FieldOffset(176)]
	public unsafe char* FilePath2;

	[FieldOffset(216)]
	public ObjectUnion UnkObject1;

	[FieldOffset(240)]
	public ObjectUnion UnkObject2;

	[FieldOffset(368)]
	public unsafe byte* UnkData;

	[FieldOffset(384)]
	public unsafe Handle* TimelineHandle;

	[FieldOffset(396)]
	public uint ObjectIndex;

	[FieldOffset(400)]
	public uint TargetIndex;

	[FieldOffset(548)]
	public unsafe fixed char FilePathBuffer[40];
}
