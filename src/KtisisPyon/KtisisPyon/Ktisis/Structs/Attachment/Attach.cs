using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;

namespace Ktisis.Structs.Attachment;

[StructLayout(LayoutKind.Explicit)]
public struct Attach
{
	[FieldOffset(80)]
	public AttachType Type;

	[FieldOffset(84)]
	public uint Capacity;

	[FieldOffset(88)]
	public unsafe Skeleton* Child;

	[FieldOffset(96)]
	public unsafe void* Parent;

	[FieldOffset(104)]
	public uint Count;

	[FieldOffset(112)]
	public unsafe AttachParam* Param;

	public bool IsActive()
	{
		if (IsValid() && Type != AttachType.None)
		{
			return Count != 0;
		}
		return false;
	}

	public unsafe bool IsValid()
	{
		if (Param != null && Child != null)
		{
			return Parent != null;
		}
		return false;
	}

	public unsafe Skeleton* GetParentSkeleton()
	{
		return Type switch
		{
			AttachType.ElementId => ((CharacterBase)Parent).Skeleton, 
			AttachType.BoneIndex => (Skeleton*)Parent, 
			_ => null, 
		};
	}
}
