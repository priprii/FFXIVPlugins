using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using Ktisis.Structs.Attachment;

namespace Ktisis.Structs.Animation;

[StructLayout(LayoutKind.Explicit, Size = 256)]
public struct SkeletonEx
{
	[FieldOffset(0)]
	public Skeleton Skeleton;

	[FieldOffset(136)]
	public unsafe ElementParam* ElementParam;

	[FieldOffset(144)]
	public unsafe Matrix4x4* ElementMatrix;

	[FieldOffset(152)]
	public unsafe ushort* ElementBoneMap;

	[FieldOffset(160)]
	public uint ElementCount;

	public bool TryGetBoneIndexForElementId(uint id, out ushort index)
	{
		return TryGetBoneIndexForElementId((ElementId)id, out index);
	}

	public unsafe bool TryGetBoneIndexForElementId(ElementId id, out ushort index)
	{
		index = ushort.MaxValue;
		for (int i = 0; i < ElementCount; i++)
		{
			if (ElementParam[i].ElementId == id)
			{
				index = ElementBoneMap[i];
				return true;
			}
		}
		return false;
	}
}
