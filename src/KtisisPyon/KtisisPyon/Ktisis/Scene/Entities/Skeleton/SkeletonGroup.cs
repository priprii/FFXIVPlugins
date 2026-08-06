using System.Collections.Generic;
using System.Linq;
using Ktisis.Editor.Posing.Types;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Entities.Skeleton;

public abstract class SkeletonGroup : SkeletonNode, IVisibility
{
	public bool Visible
	{
		get
		{
			return RecurseVisible().All((IVisibility vis) => vis.Visible);
		}
		set
		{
			foreach (IVisibility item in RecurseVisible())
			{
				item.Visible = value;
			}
		}
	}

	protected SkeletonGroup(ISceneManager scene)
		: base(scene)
	{
	}

	protected IEnumerable<IVisibility> RecurseVisible()
	{
		return Children.Where((SceneEntity child) => child is IVisibility).Cast<IVisibility>();
	}

	protected void Clean(int pIndex, uint pId)
	{
		foreach (SceneEntity item in GetChildren().Where(delegate(SceneEntity item)
		{
			if (item is BoneNodeGroup boneNodeGroup)
			{
				boneNodeGroup.Clean(pIndex, pId);
				return boneNodeGroup.IsStale();
			}
			return item is BoneNode boneNode && ((boneNode.Info.PartialIndex == pIndex && boneNode.PartialId != pId) || !boneNode.IsValid);
		}).ToList())
		{
			item.Remove();
		}
	}

	public IEnumerable<BoneNode> GetAllBones()
	{
		HashSet<BoneNode> unique = new HashSet<BoneNode>();
		foreach (SceneEntity child in Children)
		{
			if (!(child is BoneNode boneNode))
			{
				if (!(child is SkeletonGroup skeletonGroup))
				{
					continue;
				}
				foreach (BoneNode allBone in skeletonGroup.GetAllBones())
				{
					yield return allBone;
				}
			}
			else if (unique.Add(boneNode))
			{
				yield return boneNode;
			}
		}
	}

	public IEnumerable<BoneNode> GetIndividualBones()
	{
		List<BoneNode> results = new List<BoneNode>();
		foreach (SceneEntity item2 in Recurse())
		{
			if (!(item2 is BoneNodeGroup boneNodeGroup))
			{
				if (item2 is BoneNode item)
				{
					results.Add(item);
				}
			}
			else
			{
				results.AddRange(boneNodeGroup.GetIndividualBones());
			}
		}
		EntityPose pose = base.Pose;
		results = results.Distinct().ToList();
		results.RemoveAll(delegate(BoneNode bone)
		{
			int boneIndex = bone.Info.BoneIndex;
			int partialIx = bone.Info.PartialIndex;
			PartialSkeletonInfo partialInfo = pose.GetPartialInfo(partialIx);
			if (partialInfo == null)
			{
				return false;
			}
			if (partialInfo.GetParentsOf(boneIndex).Any((short parentId) => results.Any((BoneNode x) => x.MatchesId(partialIx, parentId))))
			{
				return true;
			}
			if (partialIx == 0)
			{
				return false;
			}
			PartialSkeletonInfo partialInfo2 = pose.GetPartialInfo(0);
			if (partialInfo2 == null)
			{
				return false;
			}
			short connectedParentBoneIndex = partialInfo.ConnectedParentBoneIndex;
			return partialInfo2.GetParentsOf(connectedParentBoneIndex).Prepend(connectedParentBoneIndex).Any((short id) => results.Any((BoneNode x) => x.MatchesId(0, id)));
		});
		return results;
	}
}
