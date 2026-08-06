using System.Runtime.InteropServices;

namespace Ktisis.Structs.Vfx.Apricot;

[StructLayout(LayoutKind.Explicit, Size = 1192)]
public struct ApricotInstance
{
	[FieldOffset(396)]
	public float F1;

	[FieldOffset(444)]
	public float F2;

	[FieldOffset(468)]
	public float F3;

	[FieldOffset(500)]
	public float F4;

	[FieldOffset(1181)]
	public byte State;
}
