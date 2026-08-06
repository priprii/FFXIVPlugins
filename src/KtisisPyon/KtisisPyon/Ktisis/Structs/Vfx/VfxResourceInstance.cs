using System.Runtime.InteropServices;

namespace Ktisis.Structs.Vfx;

[StructLayout(LayoutKind.Explicit, Size = 192)]
public struct VfxResourceInstance
{
	[FieldOffset(0)]
	public unsafe nint* __vfTable;

	[FieldOffset(96)]
	public VfxResourceHandle Handle;
}
