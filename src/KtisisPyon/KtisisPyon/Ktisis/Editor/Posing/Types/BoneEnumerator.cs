using System.Collections.Generic;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Container.Array;
using FFXIVClientStructs.Havok.Common.Base.Container.String;

namespace Ktisis.Editor.Posing.Types;

public class BoneEnumerator
{
	protected readonly int Index;

	protected PartialSkeleton Partial;

	public BoneEnumerator(int index, PartialSkeleton partial)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Index = index;
		Partial = partial;
	}

	protected unsafe hkaSkeleton* GetSkeleton()
	{
		hkaPose* havokPose = ((PartialSkeleton)(ref Partial)).GetHavokPose(0);
		if (havokPose == null)
		{
			return null;
		}
		return ((hkaPose)havokPose).Skeleton;
	}

	public unsafe IEnumerable<PartialBoneInfo> EnumerateBones()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		hkaSkeleton* skeleton = GetSkeleton();
		hkArray<hkaBone> bones = ((hkaSkeleton)skeleton).Bones;
		hkArray<short> parentIndices = ((hkaSkeleton)skeleton).ParentIndices;
		return EnumerateBones(bones, parentIndices);
	}

	private IEnumerable<PartialBoneInfo> EnumerateBones(hkArray<hkaBone> bones, hkArray<short> parents)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 1; i < bones.Length; i++)
		{
			hkaBone val = bones[i];
			string text = ((hkStringPtr)(ref val.Name)).String;
			if (!StringExtensions.IsNullOrEmpty(text) && parents[i] != -1)
			{
				yield return new PartialBoneInfo
				{
					Name = text,
					BoneIndex = i,
					ParentIndex = parents[i],
					PartialIndex = Index
				};
			}
		}
	}
}
