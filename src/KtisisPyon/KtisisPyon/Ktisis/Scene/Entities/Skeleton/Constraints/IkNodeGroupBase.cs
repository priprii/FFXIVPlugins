using Ktisis.Editor.Posing.Ik.Types;
using Ktisis.Scene.Decor.Ik;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Entities.Skeleton.Constraints;

public abstract class IkNodeGroupBase : BoneNodeGroup, IIkNode
{
	public readonly IIkGroup Group;

	public bool IsEnabled => Group.IsEnabled;

	protected IkNodeGroupBase(ISceneManager scene, EntityPose pose, IIkGroup group)
		: base(scene, pose)
	{
		Group = group;
	}

	public virtual void Enable()
	{
		Group.IsEnabled = true;
	}

	public virtual void Disable()
	{
		Group.IsEnabled = false;
	}
}
