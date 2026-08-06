using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Container.String;
using Ktisis.Common.Utility;
using Ktisis.Data.Files;
using Ktisis.Editor.Posing.Types;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Skeleton;

namespace Ktisis.Editor.Posing.Data;

public class EntityPoseConverter(EntityPose target)
{
	public bool IsPoseValid => target.IsValid;

	public unsafe PoseContainer Save(PoseContainer? filter = null)
	{
		PoseContainer poseContainer = new PoseContainer();
		Skeleton* skeleton = target.GetSkeleton();
		if (skeleton != null)
		{
			poseContainer.Store(skeleton, filter);
		}
		return poseContainer;
	}

	public PoseFile SaveFile()
	{
		return new PoseFile
		{
			Bones = Save()
		};
	}

	public unsafe void Load(PoseContainer pose, PoseMode mode, PoseTransforms transforms)
	{
		Skeleton* skeleton = target.GetSkeleton();
		if (skeleton != null)
		{
			pose.Apply(skeleton, mode, transforms);
		}
	}

	public unsafe void LoadPartial(PoseContainer pose, int partialIndex, PoseTransforms transforms)
	{
		Skeleton* skeleton = target.GetSkeleton();
		if (skeleton != null)
		{
			pose.ApplyToPartial(skeleton, partialIndex, transforms);
		}
	}

	public unsafe void LoadBones(PoseContainer pose, IEnumerable<PartialBoneInfo> bones, PoseTransforms transforms, PoseMode modes = PoseMode.All)
	{
		Skeleton* skeleton = target.GetSkeleton();
		if (skeleton != null)
		{
			pose.ApplyToBones(skeleton, bones, transforms, modes);
		}
	}

	public void LoadSelectedBones(PoseContainer pose, PoseTransforms transforms, PoseMode modes, bool includeDescendants)
	{
		IEnumerable<PartialBoneInfo> bones = GetSelectedBones();
		if (includeDescendants)
		{
			bones = ExpandToDescendants(bones, modes);
		}
		LoadBones(pose, bones, transforms, modes);
	}

	public unsafe void LoadReferencePose()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Skeleton* skeleton = target.GetSkeleton();
		if (skeleton == null)
		{
			return;
		}
		for (int i = 0; i < ((Skeleton)skeleton).PartialSkeletonCount; i++)
		{
			PartialSkeleton val = ((PartialSkeleton*)((Skeleton)skeleton).PartialSkeletons)[i];
			hkaPose* havokPose = ((PartialSkeleton)(ref val)).GetHavokPose(0);
			if (havokPose != null)
			{
				((hkaPose)havokPose).SetToReferencePose();
				HavokPosing.SyncModelSpace(skeleton, i);
			}
		}
	}

	public unsafe void LoadReferencePose(int partialIndex)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Skeleton* skeleton = target.GetSkeleton();
		if (skeleton == null)
		{
			return;
		}
		PartialSkeleton val = ((PartialSkeleton*)((Skeleton)skeleton).PartialSkeletons)[partialIndex];
		hkaPose* havokPose = ((PartialSkeleton)(ref val)).GetHavokPose(0);
		if (havokPose == null)
		{
			return;
		}
		((hkaPose)havokPose).SetToReferencePose();
		HavokPosing.SyncModelSpace(skeleton, partialIndex);
		if (partialIndex <= 0)
		{
			for (int i = 1; i < ((Skeleton)skeleton).PartialSkeletonCount; i++)
			{
				HavokPosing.ParentSkeleton(skeleton, i);
			}
		}
	}

	public unsafe PoseContainer FilterSelectedBones(PoseContainer pose, bool all = true, bool includeDescendants = false, PoseMode modes = PoseMode.All)
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		PoseContainer poseContainer = new PoseContainer();
		List<PartialBoneInfo> list = GetSelectedBones(all, includeDescendants, modes).ToList();
		if (list.Count == 0)
		{
			return poseContainer;
		}
		foreach (PartialBoneInfo item in list)
		{
			if (pose.TryGetValue(item.Name, out Transform value))
			{
				poseContainer[item.Name] = value;
			}
		}
		if (list.All((PartialBoneInfo bone) => bone.PartialIndex == 0))
		{
			return poseContainer;
		}
		Skeleton* skeleton = target.GetSkeleton();
		if (skeleton == null || ((Skeleton)skeleton).PartialSkeletons == null)
		{
			return poseContainer;
		}
		for (int num = 1; num < ((Skeleton)skeleton).PartialSkeletonCount; num++)
		{
			bool flag = (uint)(num - 1) <= 1u;
			if (flag && !modes.HasFlag(PoseMode.Face))
			{
				continue;
			}
			flag = (uint)(num - 1) <= 1u;
			if (!flag && !modes.HasFlag(PoseMode.Body))
			{
				continue;
			}
			PartialSkeleton val = ((PartialSkeleton*)((Skeleton)skeleton).PartialSkeletons)[num];
			hkaPose* havokPose = ((PartialSkeleton)(ref val)).GetHavokPose(0);
			if (havokPose != null && ((hkaPose)havokPose).Skeleton != null)
			{
				hkaBone val2 = ((hkaSkeleton)((hkaPose)havokPose).Skeleton).Bones[(int)val.ConnectedBoneIndex];
				string text = ((hkStringPtr)(ref val2.Name)).String;
				if (!StringExtensions.IsNullOrEmpty(text) && !poseContainer.ContainsKey(text) && pose.TryGetValue(text, out Transform value2))
				{
					poseContainer[text] = value2;
				}
			}
		}
		return poseContainer;
	}

	public PoseContainer FilterExcludeBones(PoseContainer pose, string[] excludes)
	{
		PoseContainer poseContainer = new PoseContainer();
		foreach (PartialBoneInfo item in GetBones().ToList())
		{
			if (!excludes.Contains(item.Name) && pose.TryGetValue(item.Name, out Transform value))
			{
				poseContainer[item.Name] = value;
			}
		}
		return poseContainer;
	}

	public IEnumerable<PartialBoneInfo> FilterBonesByModes(IEnumerable<PartialBoneInfo> bones, PoseMode modes)
	{
		return bones.Where(delegate(PartialBoneInfo b)
		{
			int partialIndex = b.PartialIndex;
			return ((uint)(partialIndex - 1) <= 1u) ? modes.HasFlag(PoseMode.Face) : modes.HasFlag(PoseMode.Body);
		});
	}

	public IEnumerable<PartialBoneInfo> IntersectBonesByName(IEnumerable<PartialBoneInfo> second)
	{
		return GetBones().IntersectBy(second.Select((PartialBoneInfo bone) => bone.Name), (PartialBoneInfo bone) => bone.Name);
	}

	private unsafe IEnumerable<PartialBoneInfo> GetBones()
	{
		Skeleton* skeleton = target.GetSkeleton();
		if (skeleton == null || ((Skeleton)skeleton).PartialSkeletons == null)
		{
			return Array.Empty<PartialBoneInfo>();
		}
		List<PartialBoneInfo> list = new List<PartialBoneInfo>();
		for (int i = 0; i < ((Skeleton)skeleton).PartialSkeletonCount; i++)
		{
			list.AddRange(GetPartialBones(i));
		}
		return list;
	}

	private unsafe IEnumerable<PartialBoneInfo> GetPartialBones(int index)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		Skeleton* skeleton = target.GetSkeleton();
		if (skeleton == null || ((Skeleton)skeleton).PartialSkeletons == null)
		{
			return Array.Empty<PartialBoneInfo>();
		}
		PartialSkeleton partial = ((PartialSkeleton*)((Skeleton)skeleton).PartialSkeletons)[index];
		if (((PartialSkeleton)(ref partial)).HavokPoses.IsEmpty || ((PartialSkeleton)(ref partial)).HavokPoses[0] == 0L)
		{
			return Array.Empty<PartialBoneInfo>();
		}
		return new BoneEnumerator(index, partial).EnumerateBones();
	}

	public IEnumerable<PartialBoneInfo> GetSelectedBones(bool all = true, bool includeDescendants = false, PoseMode modes = PoseMode.All)
	{
		IEnumerable<SkeletonNode> nodes = (from entity in target.Recurse().Prepend(target)
			where entity is SkeletonNode && entity.IsSelected
			select entity).Cast<SkeletonNode>();
		IEnumerable<PartialBoneInfo> enumerable = GetBoneSelectionFrom(nodes, all).Distinct();
		if (!includeDescendants)
		{
			return enumerable;
		}
		return ExpandToDescendants(enumerable, modes);
	}

	private IEnumerable<PartialBoneInfo> GetBoneSelectionFrom(IEnumerable<SkeletonNode> nodes, bool all = true)
	{
		foreach (SkeletonNode node in nodes)
		{
			if (!(node is BoneNode boneNode))
			{
				if (!(node is SkeletonGroup skeletonGroup))
				{
					continue;
				}
				foreach (PartialBoneInfo item in GetBoneSelectionFrom(all ? skeletonGroup.GetAllBones() : skeletonGroup.GetIndividualBones()))
				{
					yield return item;
				}
			}
			else
			{
				yield return boneNode.Info;
			}
		}
	}

	public IEnumerable<PartialBoneInfo> ExpandToDescendants(IEnumerable<PartialBoneInfo> bones, PoseMode modes)
	{
		if (bones == null)
		{
			return Array.Empty<PartialBoneInfo>();
		}
		IEnumerable<PartialBoneInfo> bones2 = target.ExpandToDescendants(bones);
		return FilterBonesByModes(bones2, modes);
	}

	public unsafe void FlipPose()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		List<PartialBoneInfo> list = GetBones().ToList();
		if (list.Count == 0)
		{
			return;
		}
		Skeleton* skeleton = target.GetSkeleton();
		PartialBoneInfo partialBoneInfo = list[0];
		hkaPose* havokPose = ((PartialSkeleton)((byte*)((Skeleton)skeleton).PartialSkeletons + (nint)partialBoneInfo.PartialIndex * (nint)Unsafe.SizeOf<PartialSkeleton>())).GetHavokPose(0);
		Transform modelTransform = HavokPosing.GetModelTransform(havokPose, partialBoneInfo.BoneIndex);
		for (int i = 0; i < ((Skeleton)skeleton).PartialSkeletonCount; i++)
		{
			if (((uint)(i - 1) <= 1u || i == 4) ? true : false)
			{
				continue;
			}
			PartialSkeleton val = ((PartialSkeleton*)((Skeleton)skeleton).PartialSkeletons)[i];
			hkaPose* havokPose2 = ((PartialSkeleton)(ref val)).GetHavokPose(0);
			if (havokPose2 == null || ((hkaPose)havokPose2).Skeleton == null)
			{
				continue;
			}
			Dictionary<string, Quaternion> dictionary = new Dictionary<string, Quaternion>();
			hkaBone val2;
			for (int j = 1; j < ((hkaSkeleton)((hkaPose)havokPose2).Skeleton).Bones.Length; j++)
			{
				val2 = ((hkaSkeleton)((hkaPose)havokPose2).Skeleton).Bones[j];
				string text = ((hkStringPtr)(ref val2.Name)).String;
				if (StringExtensions.IsNullOrEmpty(text) || text.StartsWith("iv_") || text.StartsWith("ya_"))
				{
					continue;
				}
				if (text.EndsWith("_l") || text.EndsWith("_r"))
				{
					string text3;
					if (!text.EndsWith("_l"))
					{
						string text2 = text;
						text3 = text2.Substring(0, text2.Length - 1) + "l";
					}
					else
					{
						string text2 = text;
						text3 = text2.Substring(0, text2.Length - 1) + "r";
					}
					string text4 = text3;
					if (HavokPosing.TryGetBoneNameIndex(havokPose2, text4) != -1)
					{
						text = text4;
					}
				}
				Transform modelTransform2 = HavokPosing.GetModelTransform(havokPose2, j);
				dictionary[text] = new Quaternion(0f - modelTransform2.Rotation.X, 0f - modelTransform2.Rotation.Y, modelTransform2.Rotation.Z, modelTransform2.Rotation.W);
			}
			for (int k = 1; k < ((hkaSkeleton)((hkaPose)havokPose2).Skeleton).Bones.Length; k++)
			{
				val2 = ((hkaSkeleton)((hkaPose)havokPose2).Skeleton).Bones[k];
				string text5 = ((hkStringPtr)(ref val2.Name)).String;
				if (!StringExtensions.IsNullOrEmpty(text5) && dictionary.TryGetValue(text5, out var value))
				{
					Transform modelTransform3 = HavokPosing.GetModelTransform(havokPose2, k);
					Transform trans = new Transform(modelTransform3.Position, value, modelTransform3.Scale);
					HavokPosing.SetModelTransform(havokPose2, k, trans);
					HavokPosing.Propagate(skeleton, i, k, trans, modelTransform3);
				}
			}
		}
		Transform modelTransform4 = HavokPosing.GetModelTransform(havokPose, partialBoneInfo.BoneIndex);
		Transform transform = new Transform(modelTransform4.Position, modelTransform4.Rotation, modelTransform4.Scale);
		float yaw = HkaEulerAngles.GetYaw(modelTransform.Rotation);
		float yaw2 = HkaEulerAngles.GetYaw(modelTransform4.Rotation);
		float angle = yaw - yaw2;
		Quaternion quaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle);
		transform.Rotation = Quaternion.Normalize(quaternion * modelTransform4.Rotation);
		Quaternion quaternion2 = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI);
		transform.Rotation = Quaternion.Normalize(quaternion2 * transform.Rotation);
		HavokPosing.SetModelTransform(havokPose, partialBoneInfo.BoneIndex, transform);
		HavokPosing.Propagate(skeleton, partialBoneInfo.PartialIndex, partialBoneInfo.BoneIndex, transform, modelTransform4);
	}
}
