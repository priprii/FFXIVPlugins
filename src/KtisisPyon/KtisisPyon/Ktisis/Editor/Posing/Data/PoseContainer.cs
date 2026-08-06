using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Container.String;
using Ktisis.Common.Extensions;
using Ktisis.Common.Utility;
using Ktisis.Editor.Posing.Types;

namespace Ktisis.Editor.Posing.Data;

[Serializable]
public class PoseContainer : Dictionary<string, Transform>
{
	public unsafe void Store(Skeleton* modelSkeleton, PoseContainer? filter = null)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		if (modelSkeleton == null)
		{
			return;
		}
		Clear();
		ushort partialSkeletonCount = ((Skeleton)modelSkeleton).PartialSkeletonCount;
		PartialSkeleton* partialSkeletons = ((Skeleton)modelSkeleton).PartialSkeletons;
		for (int i = 0; i < partialSkeletonCount; i++)
		{
			PartialSkeleton val = ((PartialSkeleton*)partialSkeletons)[i];
			hkaPose* havokPose = ((PartialSkeleton)(ref val)).GetHavokPose(0);
			if (havokPose == null || ((hkaPose)havokPose).Skeleton == null)
			{
				continue;
			}
			hkaSkeleton* skeleton = ((hkaPose)havokPose).Skeleton;
			for (int j = 0; j < ((hkaSkeleton)skeleton).Bones.Length; j++)
			{
				if (j != val.ConnectedBoneIndex)
				{
					hkaBone val2 = ((hkaSkeleton)skeleton).Bones[j];
					string text = ((hkStringPtr)(ref val2.Name)).String;
					if (!StringExtensions.IsNullOrEmpty(text) && (filter == null || (filter.ContainsKey(text) && i != 4)))
					{
						base[text] = new Transform(((hkaPose)havokPose).ModelPose[j]);
					}
				}
			}
		}
	}

	public unsafe void Apply(Skeleton* modelSkeleton, PoseMode modes = PoseMode.All, PoseTransforms transforms = PoseTransforms.Rotation)
	{
		if (modelSkeleton == null || (!modes.HasFlag(PoseMode.Face) && !modes.HasFlag(PoseMode.Body)))
		{
			return;
		}
		for (int i = 0; i < ((Skeleton)modelSkeleton).PartialSkeletonCount; i++)
		{
			if ((uint)(i - 1) <= 1u)
			{
				if (!modes.HasFlag(PoseMode.Face))
				{
					continue;
				}
			}
			else if (!modes.HasFlag(PoseMode.Body))
			{
				continue;
			}
			ApplyToPartial(modelSkeleton, i, transforms, modes);
		}
	}

	public unsafe void ApplyToBones(Skeleton* modelSkeleton, IEnumerable<PartialBoneInfo> bones, PoseTransforms transforms = PoseTransforms.Rotation, PoseMode modes = PoseMode.All)
	{
		Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
		foreach (PartialBoneInfo bone in bones)
		{
			int partialIndex = bone.PartialIndex;
			if (!dictionary.TryGetValue(partialIndex, out var value))
			{
				value = new List<int>();
				dictionary.Add(partialIndex, value);
			}
			value.Add(bone.BoneIndex);
		}
		for (int i = 0; i < ((Skeleton)modelSkeleton).PartialSkeletonCount; i++)
		{
			if (dictionary.TryGetValue(i, out var value2))
			{
				ApplyToPartialBones(modelSkeleton, i, value2, transforms, modes, isSelective: true);
			}
		}
	}

	public unsafe void ApplyToPartial(Skeleton* modelSkeleton, int partialIndex, PoseTransforms transforms = PoseTransforms.Rotation, PoseMode modes = PoseMode.All)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		PartialSkeleton val = ((PartialSkeleton*)((Skeleton)modelSkeleton).PartialSkeletons)[partialIndex];
		hkaPose* havokPose = ((PartialSkeleton)(ref val)).GetHavokPose(0);
		if (havokPose != null && ((hkaPose)havokPose).Skeleton != null)
		{
			ApplyToPartialBones(modelSkeleton, partialIndex, Enumerable.Range(1, ((hkaSkeleton)((hkaPose)havokPose).Skeleton).Bones.Length - 1), transforms, modes);
		}
	}

	public unsafe void ApplyToPartialBones(Skeleton* modelSkeleton, int partialIndex, IEnumerable<int> bones, PoseTransforms transforms = PoseTransforms.Rotation, PoseMode modes = PoseMode.All, bool isSelective = false)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		if (modelSkeleton == null)
		{
			return;
		}
		PartialSkeleton val = ((PartialSkeleton*)((Skeleton)modelSkeleton).PartialSkeletons)[partialIndex];
		hkaPose* havokPose = ((PartialSkeleton)(ref val)).GetHavokPose(0);
		if (havokPose == null || ((hkaPose)havokPose).Skeleton == null)
		{
			return;
		}
		hkaSkeleton* skeleton = ((hkaPose)havokPose).Skeleton;
		Quaternion offset = Quaternion.Identity;
		if (partialIndex > 0)
		{
			Quaternion quaternion = HavokPosing.ParentSkeleton(modelSkeleton, partialIndex);
			short connectedBoneIndex = val.ConnectedBoneIndex;
			Quaternion quaternion2 = ((hkaPose)havokPose).ModelPose[(int)connectedBoneIndex].Rotation.ToQuaternion();
			hkaBone val2 = ((hkaSkeleton)skeleton).Bones[(int)connectedBoneIndex];
			string text = ((hkStringPtr)(ref val2.Name)).String;
			if (!StringExtensions.IsNullOrEmpty(text) && TryGetValue(text, out Transform value))
			{
				offset = quaternion2 / value.Rotation / quaternion;
			}
			else
			{
				Ktisis.Log.Warning($"Failed to find parent bone '{text}' for partial {partialIndex}!");
			}
		}
		foreach (int item in Enumerable.Range(1, ((hkaSkeleton)skeleton).Bones.Length - 1).Intersect(bones))
		{
			ApplyToBone(modelSkeleton, havokPose, partialIndex, item, offset, transforms, modes, isSelective);
		}
	}

	public unsafe void ApplyToBone(Skeleton* modelSkeleton, hkaPose* pose, int partialIndex, int boneIndex, Quaternion offset, PoseTransforms transforms = PoseTransforms.Rotation, PoseMode modes = PoseMode.All, bool isSelective = false)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		hkaBone val = ((hkaSkeleton)((hkaPose)pose).Skeleton).Bones[boneIndex];
		string text = ((hkStringPtr)(ref val.Name)).String;
		if (StringExtensions.IsNullOrEmpty(text) || !TryGetValue(text, out Transform value))
		{
			return;
		}
		Transform modelTransform = HavokPosing.GetModelTransform(pose, boneIndex);
		Transform transform = new Transform(modelTransform.Position, modelTransform.Rotation, modelTransform.Scale);
		bool flag = transforms.HasFlag(PoseTransforms.Rotation);
		bool flag2 = transforms.HasFlag(PoseTransforms.Position);
		bool num = transforms.HasFlag(PoseTransforms.Scale);
		bool flag3 = partialIndex == 0 && boneIndex == 1 && transforms.HasFlag(PoseTransforms.PositionRoot);
		if (partialIndex == 0 && boneIndex == 1)
		{
			value.Rotation = Quaternion.Identity;
		}
		if (flag3)
		{
			modelTransform.Rotation = Quaternion.Normalize(offset * value.Rotation);
		}
		if (num)
		{
			transform.Scale = value.Scale;
		}
		Transform modelParent = new Transform();
		Transform currentParent = new Transform();
		bool num2;
		if (!isSelective)
		{
			if (!flag2 || !modes.HasFlag(PoseMode.Face))
			{
				goto IL_0148;
			}
			num2 = !modes.HasFlag(PoseMode.Body);
		}
		else
		{
			num2 = flag2 || flag;
		}
		if (!num2)
		{
			goto IL_0148;
		}
		int num3 = (TryGetRelativeParent(pose, partialIndex, boneIndex, out modelParent, out currentParent) ? 1 : 0);
		goto IL_0149;
		IL_0148:
		num3 = 0;
		goto IL_0149;
		IL_0149:
		bool flag4 = (byte)num3 != 0;
		if (flag2 || flag3)
		{
			if (flag4)
			{
				Vector3 value2 = value.Position - modelParent.Position;
				Quaternion rotation = Quaternion.Normalize(currentParent.Rotation * Quaternion.Inverse(modelParent.Rotation));
				transform.Position = currentParent.Position + Vector3.Transform(value2, rotation);
			}
			else
			{
				transform.Position = value.Position;
			}
		}
		if (flag)
		{
			if (isSelective && flag4)
			{
				Quaternion quaternion = Quaternion.Normalize(Quaternion.Inverse(modelParent.Rotation) * value.Rotation);
				transform.Rotation = Quaternion.Normalize(currentParent.Rotation * quaternion);
			}
			else
			{
				transform.Rotation = Quaternion.Normalize(offset * value.Rotation);
			}
		}
		HavokPosing.SetModelTransform(pose, boneIndex, transform);
		HavokPosing.Propagate(modelSkeleton, partialIndex, boneIndex, transform, modelTransform);
	}

	private unsafe bool TryGetRelativeParent(hkaPose* pose, int partialIndex, int boneIndex, out Transform modelParent, out Transform currentParent)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		modelParent = new Transform();
		currentParent = new Transform();
		int num = -1;
		if ((uint)(partialIndex - 1) <= 1u)
		{
			num = 0;
		}
		else
		{
			num = ((hkaSkeleton)((hkaPose)pose).Skeleton).ParentIndices[boneIndex];
			if (num == -1)
			{
				return false;
			}
		}
		hkaBone val = ((hkaSkeleton)((hkaPose)pose).Skeleton).Bones[num];
		string text = ((hkStringPtr)(ref val.Name)).String;
		if (StringExtensions.IsNullOrEmpty(text) || !TryGetValue(text, out modelParent))
		{
			return false;
		}
		currentParent = HavokPosing.GetModelTransform(pose, num);
		return true;
	}
}
