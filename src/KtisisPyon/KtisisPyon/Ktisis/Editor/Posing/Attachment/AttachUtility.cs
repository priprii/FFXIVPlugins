using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Common.Math;
using FFXIVClientStructs.Havok.Animation.Rig;
using Ktisis.Common.Utility;
using Ktisis.Structs.Animation;
using Ktisis.Structs.Attachment;

namespace Ktisis.Editor.Posing.Attachment;

public static class AttachUtility
{
	public unsafe static void SetBoneAttachment(Skeleton* parent, Skeleton* child, Attach* attach, ushort parentBoneId, ushort childBoneId = 0)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (parent == child)
		{
			throw new Exception("Attempting to parent attachment point to itself.");
		}
		bool num = attach->Count != 0;
		attach->Type = AttachType.BoneIndex;
		attach->Count = 1u;
		attach->Parent = parent;
		attach->Child = child;
		attach->Param->ParentId = parentBoneId;
		attach->Param->ChildId = childBoneId;
		if (!num)
		{
			Unsafe.Write(&attach->Param->Transform, (Transform)new Transform());
		}
	}

	public unsafe static bool TryGetParentBoneIndex(Attach* attach, out ushort index)
	{
		index = attach->Param->ParentId;
		return attach->Type switch
		{
			AttachType.BoneIndex => true, 
			AttachType.ElementId => ((SkeletonEx*)attach->GetParentSkeleton())->TryGetBoneIndexForElementId(index, out index), 
			_ => false, 
		};
	}

	public unsafe static void SetTransformRelative(Attach* attach, Transform target, Transform source)
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		Skeleton* parentSkeleton = attach->GetParentSkeleton();
		if (parentSkeleton == null || ((Skeleton)parentSkeleton).PartialSkeletons == null || ((PartialSkeleton)((Skeleton)parentSkeleton).PartialSkeletons).HavokPoses.IsEmpty)
		{
			return;
		}
		hkaPose* havokPose = ((PartialSkeleton)((Skeleton)parentSkeleton).PartialSkeletons).GetHavokPose(0);
		if (havokPose == null || !TryGetParentBoneIndex(attach, out var index))
		{
			return;
		}
		Quaternion quaternion = Quaternion.Identity;
		if (attach->Type == AttachType.ElementId)
		{
			SkeletonEx* ptr = (SkeletonEx*)parentSkeleton;
			for (int i = 0; i < ptr->ElementCount; i++)
			{
				ElementParam* ptr2 = ptr->ElementParam + i;
				if ((ushort)ptr2->ElementId == attach->Param->ParentId)
				{
					quaternion = (ptr2->Rotation * MathHelpers.Rad2Deg).EulerAnglesToQuaternion();
				}
			}
		}
		Transform modelTransform = HavokPosing.GetModelTransform(havokPose, index);
		Quaternion quaternion2 = Quaternion.Inverse(Quaternion.Normalize(Quaternion.op_Implicit(((Transform)(&((Skeleton)parentSkeleton).Transform)).Rotation) * modelTransform.Rotation * quaternion));
		Transform transform = new Transform(attach->Param->Transform);
		transform.Position += Vector3.Transform(target.Position - source.Position, quaternion2);
		transform.Rotation = Quaternion.Normalize(quaternion2 * target.Rotation);
		Unsafe.Write(&attach->Param->Transform, (Transform)transform);
	}

	public unsafe static void Detach(Attach* attach)
	{
		attach->Type = AttachType.None;
		attach->Count = 0u;
		attach->Parent = null;
		attach->Child = null;
		if (attach->Param != null)
		{
			attach->Param->ParentId = ushort.MaxValue;
			attach->Param->ChildId = ushort.MaxValue;
		}
	}
}
