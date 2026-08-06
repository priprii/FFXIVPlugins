using System.Numerics;
using System.Runtime.InteropServices;

namespace Ktisis.Structs.Attachment;

[StructLayout(LayoutKind.Explicit, Size = 64)]
public struct ElementParam
{
	[FieldOffset(0)]
	public unsafe fixed char NameBytes[28];

	[FieldOffset(32)]
	public ElementId ElementId;

	[FieldOffset(36)]
	public Vector3 Position;

	[FieldOffset(48)]
	public Vector3 Rotation;
}
