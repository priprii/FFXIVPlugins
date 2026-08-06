using Ktisis.Scene.Types;

namespace Ktisis.Scene.Entities.Skeleton;

public abstract class SkeletonNode : SceneEntity
{
	public EntityPose Pose { get; protected init; }

	public int SortPriority { get; set; }

	protected SkeletonNode(ISceneManager scene)
		: base(scene)
	{
	}

	public void OrderByPriority()
	{
		GetChildren().Sort(delegate(SceneEntity _a, SceneEntity _b)
		{
			SkeletonNode skeletonNode;
			SkeletonNode skeletonNode2;
			if (!(_a is SkeletonGroup))
			{
				if (_b is SkeletonGroup)
				{
					return 1;
				}
				skeletonNode = _a as SkeletonNode;
				if (skeletonNode != null)
				{
					skeletonNode2 = _b as SkeletonNode;
					if (skeletonNode2 != null)
					{
						goto IL_0048;
					}
				}
				return 0;
			}
			if (!(_b is SkeletonGroup))
			{
				return -1;
			}
			skeletonNode = (SkeletonNode)_a;
			skeletonNode2 = (SkeletonNode)_b;
			goto IL_0048;
			IL_0048:
			return skeletonNode.SortPriority - skeletonNode2.SortPriority;
		});
	}
}
