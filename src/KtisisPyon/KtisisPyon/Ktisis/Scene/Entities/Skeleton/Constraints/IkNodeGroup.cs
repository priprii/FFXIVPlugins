using Ktisis.Editor.Posing.Ik.Types;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Entities.Skeleton.Constraints;

public class IkNodeGroup<T> : IkNodeGroupBase where T : IIkGroup
{
	public new readonly T Group;

	public IkNodeGroup(ISceneManager scene, EntityPose pose, T group)
		: base(scene, pose, group)
	{
		Group = group;
	}
}
