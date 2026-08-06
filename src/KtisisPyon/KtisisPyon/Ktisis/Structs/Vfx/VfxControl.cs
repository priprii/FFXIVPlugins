using System.Runtime.InteropServices;
using Ktisis.Structs.Animation;

namespace Ktisis.Structs.Vfx;

[StructLayout(LayoutKind.Explicit, Size = 240)]
public struct VfxControl
{
	[FieldOffset(0)]
	public SchedulerState State;

	[FieldOffset(40)]
	public unsafe SchedulerVfx* SchedulerVfx;
}
