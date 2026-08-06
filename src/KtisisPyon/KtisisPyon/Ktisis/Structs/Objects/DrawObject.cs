using System.Runtime.InteropServices;

namespace Ktisis.Structs.Objects;

[StructLayout(LayoutKind.Explicit, Size = 144)]
public struct DrawObject
{
	[FieldOffset(137)]
	public OutlineChoice OutlineFlags;
}
