using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;
using Ktisis.Scene.Decor;

namespace Ktisis.Editor.Posing.Data;

public class SkeletonPoseData
{
	public unsafe Skeleton* Skeleton;

	public PartialSkeleton Partial;

	public unsafe hkaPose* Pose;

	public unsafe short TryResolveBone(IEnumerable<string> names)
	{
		return names.Select((string name) => HavokPosing.TryGetBoneNameIndex(Pose, name)).FirstOrDefault<short>((short index) => index != -1, -1);
	}

	public unsafe static SkeletonPoseData? TryGet(Skeleton* skeleton, int partialIndex, int poseIndex)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (skeleton == null || ((Skeleton)skeleton).PartialSkeletons == null || partialIndex > ((Skeleton)skeleton).PartialSkeletonCount)
		{
			return null;
		}
		PartialSkeleton val = ((PartialSkeleton*)((Skeleton)skeleton).PartialSkeletons)[partialIndex];
		if (((PartialSkeleton)(ref val)).HavokPoses.IsEmpty || val.SkeletonResourceHandle == null)
		{
			return null;
		}
		hkaPose* havokPose = ((PartialSkeleton)(ref val)).GetHavokPose(poseIndex);
		if (havokPose == null || ((hkaPose)havokPose).Skeleton == null)
		{
			return null;
		}
		return new SkeletonPoseData
		{
			Skeleton = skeleton,
			Partial = val,
			Pose = havokPose
		};
	}

	public unsafe static SkeletonPoseData? TryGet(ISkeleton skeleton, int partialIndex, int poseIndex)
	{
		Skeleton* skeleton2 = skeleton.GetSkeleton();
		if (skeleton2 == null)
		{
			return null;
		}
		return TryGet(skeleton2, partialIndex, poseIndex);
	}
}
