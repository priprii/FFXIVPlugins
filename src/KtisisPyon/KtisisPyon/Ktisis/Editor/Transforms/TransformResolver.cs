using System.Collections.Generic;
using System.Linq;
using Ktisis.Editor.Posing.Types;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Skeleton;

namespace Ktisis.Editor.Transforms;

public static class TransformResolver
{
	public static SceneEntity? GetPoseTarget(IEnumerable<SceneEntity> entities)
	{
		BoneNode boneNode = null;
		foreach (BoneNode item in entities.Where((SceneEntity item) => item is BoneNode).Cast<BoneNode>())
		{
			if (boneNode == null)
			{
				boneNode = item;
				continue;
			}
			EntityPose pose = item.Pose;
			if (pose != boneNode.Pose)
			{
				continue;
			}
			int partialIndex = item.Info.PartialIndex;
			PartialSkeletonInfo partialInfo = pose.GetPartialInfo(partialIndex);
			if (partialInfo != null)
			{
				int num = partialIndex;
				int partialIndex2 = boneNode.Info.PartialIndex;
				int? num2;
				if (num == partialIndex2)
				{
					num2 = boneNode.Info.BoneIndex;
				}
				else
				{
					int num3 = partialIndex2;
					num2 = ((num >= num3) ? ((int?)null) : pose.GetPartialInfo(num3)?.ConnectedParentBoneIndex);
				}
				int? num4 = num2;
				if (num4.HasValue && (partialInfo.IsBoneDescendantOf(num4.Value, item.Info.BoneIndex) || (boneNode.Info.ParentIndex == item.Info.ParentIndex && item.Info.BoneIndex < boneNode.Info.BoneIndex)))
				{
					boneNode = item;
				}
			}
		}
		return boneNode;
	}

	public static IEnumerable<SceneEntity> GetCorrelatingBones(IEnumerable<SceneEntity> entities, bool yieldDefault = false)
	{
		HashSet<BoneNode> unique = new HashSet<BoneNode>();
		foreach (SceneEntity entity in entities)
		{
			if (!(entity is BoneNode boneNode))
			{
				if (entity is SkeletonGroup skeletonGroup)
				{
					foreach (BoneNode item in from bone in skeletonGroup.GetIndividualBones()
						where unique.Add(bone)
						select bone)
					{
						yield return item;
					}
				}
				else if (yieldDefault)
				{
					yield return entity;
				}
			}
			else if (unique.Add(boneNode))
			{
				yield return boneNode;
			}
		}
	}

	public static Dictionary<EntityPose, Dictionary<int, List<BoneNode>>> BuildPoseMap(SceneEntity? target, IEnumerable<SceneEntity> entities)
	{
		Dictionary<EntityPose, Dictionary<int, List<BoneNode>>> dictionary = new Dictionary<EntityPose, Dictionary<int, List<BoneNode>>>();
		foreach (BoneNode item in GetCorrelatingBones(entities).Cast<BoneNode>())
		{
			EntityPose pose = item.Pose;
			if (pose != target)
			{
				Dictionary<int, List<BoneNode>> value;
				bool num = dictionary.TryGetValue(pose, out value);
				if (value == null)
				{
					value = new Dictionary<int, List<BoneNode>>();
				}
				int partialIndex = item.Info.PartialIndex;
				List<BoneNode> value2;
				bool num2 = value.TryGetValue(partialIndex, out value2);
				if (value2 == null)
				{
					value2 = new List<BoneNode>();
				}
				value2.Add(item);
				if (!num2)
				{
					value.Add(partialIndex, value2);
				}
				if (!num)
				{
					dictionary.Add(pose, value);
				}
			}
		}
		return dictionary;
	}
}
