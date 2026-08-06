using Ktisis.Scene.Entities.Skeleton;

namespace Ktisis.Scene.Factory.Builders;

public interface IBoneTreeBuilder
{
	IBoneTreeBuilder BuildBoneList();

	IBoneTreeBuilder BuildCategoryMap();

	void BindTo(EntityPose pose);
}
