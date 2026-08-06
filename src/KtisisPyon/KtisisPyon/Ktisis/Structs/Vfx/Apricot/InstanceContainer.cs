using System.Runtime.InteropServices;

namespace Ktisis.Structs.Vfx.Apricot;

[StructLayout(LayoutKind.Explicit, Size = 136)]
public struct InstanceContainer
{
	public const int Size = 136;

	[FieldOffset(0)]
	public unsafe ApricotInstance* Instance;
}
