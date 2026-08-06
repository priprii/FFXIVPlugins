using System.Linq;
using Ktisis.Data.Config.Bones;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Entities.Skeleton;

public class BoneNodeGroup : SkeletonGroup, IAttachTarget
{
	public BoneCategory? Category { get; set; }

	public BoneNodeGroup(ISceneManager scene, EntityPose pose)
		: base(scene)
	{
		base.Type = EntityType.BoneGroup;
		base.Pose = pose;
	}

	public bool IsStale()
	{
		if (IsValid && GetChildren().Count != 0)
		{
			return IsDisabledNsfw();
		}
		return true;
	}

	private bool IsDisabledNsfw()
	{
		BoneCategory category = Category;
		if (category != null && category.IsNsfw)
		{
			return !Scene.Context.Config.Categories.ShowNsfwBones;
		}
		return false;
	}

	public bool TryAcceptAttach(IAttachable child)
	{
		return (from bone in GetIndividualBones()
			where bone.Info.PartialIndex == 0
			select bone).MinBy((BoneNode bone) => bone.Info.BoneIndex)?.TryAcceptAttach(child) ?? false;
	}
}
