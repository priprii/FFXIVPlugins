using System.Numerics;
using Ktisis.Common.Extensions;
using Ktisis.Common.Utility;
using Ktisis.Editor.Posing.Ik.TwoJoints;
using Ktisis.Editor.Posing.Types;
using Ktisis.Scene.Decor.Ik;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Entities.Skeleton.Constraints;

public class TwoJointEndNode : IkEndNode, ITwoJointsNode, IIkNode
{
	public TwoJointsGroup Group { get; }

	protected override bool IsOverride
	{
		get
		{
			if (IsEnabled)
			{
				return Group.Mode == TwoJointsMode.Fixed;
			}
			return false;
		}
	}

	public TwoJointEndNode(ISceneManager scene, EntityPose pose, PartialBoneInfo bone, uint partialId, TwoJointsGroup group)
		: base(scene, pose, bone, partialId)
	{
		Group = group;
	}

	public override Transform GetTransformTarget(Transform offset, Transform world)
	{
		offset.Position += Group.TargetPosition.ModelToWorldPos(offset);
		offset.Rotation = Quaternion.Normalize(offset.Rotation * Group.TargetRotation);
		offset.Scale = world.Scale;
		return offset;
	}

	public unsafe override void SetTransformTarget(Transform transform, Transform offset, Transform world)
	{
		if (base.Pose.GetSkeleton() != null)
		{
			bool flag = false;
			if (Group.EnforcePosition)
			{
				Group.TargetPosition = transform.Position.WorldToModelPos(offset);
			}
			else
			{
				world.Position = transform.Position;
				flag = true;
			}
			if (Group.EnforceRotation)
			{
				Group.TargetRotation = Quaternion.Normalize(Quaternion.Inverse(offset.Rotation) * transform.Rotation);
			}
			else
			{
				world.Rotation = transform.Rotation;
				flag = true;
			}
			if (!world.Scale.Equals(transform.Scale))
			{
				world.Scale = transform.Scale;
				flag = true;
			}
			if (flag)
			{
				SetTransformWorld(world);
			}
		}
	}
}
