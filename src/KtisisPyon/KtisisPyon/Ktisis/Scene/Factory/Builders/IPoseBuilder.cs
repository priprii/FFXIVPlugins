using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Scene.Factory.Types;

namespace Ktisis.Scene.Factory.Builders;

public interface IPoseBuilder : IEntityBuilder<EntityPose, IPoseBuilder>, IEntityBuilderBase<EntityPose, IPoseBuilder>
{
	IBoneTreeBuilder BuildBoneTree(int index, uint partialId, PartialSkeleton partial);
}
