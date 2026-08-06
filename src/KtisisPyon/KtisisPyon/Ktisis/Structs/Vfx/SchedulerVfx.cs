using System.Runtime.InteropServices;

namespace Ktisis.Structs.Vfx;

[StructLayout(LayoutKind.Explicit)]
public struct SchedulerVfx
{
	[FieldOffset(0)]
	public unsafe nint* __vfTable;

	[FieldOffset(128)]
	public unsafe VfxObject* Instance;
}
