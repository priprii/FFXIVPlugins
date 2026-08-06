using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;
using Ktisis.Common.Extensions;

namespace Ktisis.Editor.Posing.Types;

public class PartialSkeletonInfo
{
	public uint Id;

	public string? Name;

	public short ConnectedBoneIndex;

	public short ConnectedParentBoneIndex;

	public short[] ParentIds = Array.Empty<short>();

	public PartialSkeletonInfo(uint id)
	{
		Id = id;
	}

	public PartialSkeletonInfo(uint id, string name)
	{
		Id = id;
		Name = name;
	}

	public unsafe void CopyPartial(uint id, PartialSkeleton partial)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Id = id;
		ConnectedBoneIndex = partial.ConnectedBoneIndex;
		ConnectedParentBoneIndex = partial.ConnectedParentBoneIndex;
		hkaPose* havokPose = ((PartialSkeleton)(ref partial)).GetHavokPose(0);
		if (havokPose != null && ((hkaPose)havokPose).Skeleton != null)
		{
			ParentIds = ((hkaSkeleton)((hkaPose)havokPose).Skeleton).ParentIndices.Copy<short>();
		}
		else
		{
			ParentIds = Array.Empty<short>();
		}
	}

	public IEnumerable<short> GetParentsOf(int id)
	{
		for (short parent = ParentIds[id]; parent != -1; parent = ParentIds[parent])
		{
			yield return parent;
		}
	}

	public bool IsBoneDescendantOf(int bone, int descOf)
	{
		for (short num = ParentIds[bone]; num != -1; num = ParentIds[num])
		{
			if (num == descOf)
			{
				return true;
			}
		}
		return false;
	}
}
