using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics;

namespace Ktisis.Structs.Attachment;

[StructLayout(LayoutKind.Explicit, Size = 64)]
public struct AttachParam
{
	[FieldOffset(0)]
	public ushort ChildId;

	[FieldOffset(2)]
	public ushort ParentId;

	[FieldOffset(16)]
	public Transform Transform;
}
