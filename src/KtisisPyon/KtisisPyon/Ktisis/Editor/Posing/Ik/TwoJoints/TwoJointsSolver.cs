using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Container.Array;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using Ktisis.Common.Utility;
using Ktisis.Interop;
using Ktisis.Structs.Havok;

namespace Ktisis.Editor.Posing.Ik.TwoJoints;

public class TwoJointsSolver(IkModule module) : IDisposable
{
	private readonly Alloc<TwoJointsIkSetup> AllocIkSetup = new Alloc<TwoJointsIkSetup>(16uL);

	private Transform? LastPoseInModel;

	public unsafe TwoJointsIkSetup* IkSetup => AllocIkSetup.Data;

	public bool IsDisposed { get; private set; }

	public unsafe void Setup()
	{
		if (AllocIkSetup.Address == IntPtr.Zero)
		{
			throw new Exception("Allocation for IkSetup failed.");
		}
		*IkSetup = new TwoJointsIkSetup
		{
			m_firstJointIdx = -1,
			m_secondJointIdx = -1,
			m_endBoneIdx = -1,
			m_firstJointTwistIdx = -1,
			m_secondJointTwistIdx = -1,
			m_hingeAxisLS = new Vector4(0f, 0f, 1f, 1f),
			m_cosineMaxHingeAngle = -1f,
			m_cosineMinHingeAngle = 1f,
			m_firstJointIkGain = 1f,
			m_secondJointIkGain = 1f,
			m_endJointIkGain = 1f,
			m_endTargetMS = Vector4.Zero,
			m_endTargetRotationMS = Quaternion.Identity,
			m_endBoneOffsetLS = Vector4.Zero,
			m_endBoneRotationOffsetLS = Quaternion.Identity,
			m_enforceEndPosition = true,
			m_enforceEndRotation = false
		};
	}

	public unsafe bool Solve(hkaPose* poseIn, hkaPose* poseOut, bool frozen = false)
	{
		if (poseOut == null || ((hkaPose)poseOut).Skeleton == null)
		{
			return false;
		}
		if (frozen)
		{
			((hkaPose)poseIn).SetToReferencePose();
			((hkaPose)poseIn).SyncModelSpace();
			UpdateModelPose(poseIn, poseOut);
		}
		byte b = 0;
		module.SolveTwoJoints(&b, IkSetup, poseIn);
		if (b == 0)
		{
			return false;
		}
		((hkaPose)poseIn).SyncModelSpace();
		if (frozen)
		{
			ApplyModelPoseStatic(poseIn, poseOut);
		}
		else
		{
			ApplyModelPoseDynamic(poseIn, poseOut);
		}
		return true;
	}

	public unsafe bool SolveGroup(hkaPose* poseIn, hkaPose* poseOut, TwoJointsGroup group, bool frozen = false)
	{
		if (!group.IsEnabled)
		{
			return false;
		}
		TwoJointsIkSetup* ikSetup = IkSetup;
		ikSetup->m_firstJointIdx = group.FirstBoneIndex;
		ikSetup->m_firstJointTwistIdx = group.FirstTwistIndex;
		ikSetup->m_secondJointIdx = group.SecondBoneIndex;
		ikSetup->m_secondJointTwistIdx = group.SecondTwistIndex;
		ikSetup->m_endBoneIdx = group.EndBoneIndex;
		ikSetup->m_firstJointIkGain = group.FirstBoneGain;
		ikSetup->m_secondJointIkGain = group.SecondBoneGain;
		ikSetup->m_endJointIkGain = group.EndBoneGain;
		ikSetup->m_enforceEndPosition = group.EnforcePosition;
		ikSetup->m_enforceEndRotation = group.EnforceRotation;
		ikSetup->m_hingeAxisLS = new Vector4(group.HingeAxis, 1f);
		ikSetup->m_cosineMinHingeAngle = group.MinHingeAngle;
		ikSetup->m_cosineMaxHingeAngle = group.MaxHingeAngle;
		Transform modelTransform = HavokPosing.GetModelTransform(poseOut, group.EndBoneIndex);
		if (modelTransform == null)
		{
			return false;
		}
		bool num = group.Mode == TwoJointsMode.Relative;
		if (num || !group.EnforcePosition)
		{
			group.TargetPosition = modelTransform.Position;
		}
		if (num || !group.EnforceRotation)
		{
			group.TargetRotation = modelTransform.Rotation;
		}
		ikSetup->m_endTargetMS = new Vector4(group.TargetPosition, 0f);
		ikSetup->m_endTargetRotationMS = group.TargetRotation;
		return Solve(poseIn, poseOut, frozen);
	}

	private unsafe void UpdateModelPose(hkaPose* poseIn, hkaPose* poseOut)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		short firstJointIdx = IkSetup->m_firstJointIdx;
		for (int i = 1; i < ((hkaSkeleton)((hkaPose)poseIn).Skeleton).Bones.Length; i++)
		{
			if (i == firstJointIdx || HavokPosing.IsBoneDescendantOf(((hkaSkeleton)((hkaPose)poseOut).Skeleton).ParentIndices, firstJointIdx, i))
			{
				Unsafe.Write(((hkaPose)poseIn).AccessBoneModelSpace(i, (PropagateOrNot)1), ((hkaPose)poseOut).ModelPose[i]);
			}
		}
	}

	private unsafe void ApplyModelPoseStatic(hkaPose* poseIn, hkaPose* poseOut)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		hkArray<short> parentIndices = ((hkaSkeleton)((hkaPose)poseOut).Skeleton).ParentIndices;
		hkaSkeletonUtils.transformModelPoseToLocalPose(((hkaSkeleton)((hkaPose)poseOut).Skeleton).Bones.Length, parentIndices.Data, ((hkaPose)poseOut).ModelPose.Data, ((hkaPose)poseIn).LocalPose.Data);
		short firstJointIdx = IkSetup->m_firstJointIdx;
		short endBoneIdx = IkSetup->m_endBoneIdx;
		Transform modelTransform = HavokPosing.GetModelTransform(poseIn, endBoneIdx);
		for (int i = 1; i < ((hkaSkeleton)((hkaPose)poseOut).Skeleton).Bones.Length; i++)
		{
			if (i == firstJointIdx || HavokPosing.IsBoneDescendantOf(parentIndices, i, firstJointIdx))
			{
				if (!HavokPosing.IsBoneDescendantOf(parentIndices, i, endBoneIdx))
				{
					byte* num = (byte*)((hkaPose)poseOut).ModelPose.Data + (nint)i * (nint)Unsafe.SizeOf<hkQsTransformf>();
					hkQsTransformf val = ((hkaPose)poseIn).ModelPose[i];
					Unsafe.Write(&((hkQsTransformf)num).Translation, val.Translation);
					Unsafe.Write(&((hkQsTransformf)num).Rotation, val.Rotation);
				}
				else if (LastPoseInModel == null || !IkSetup->m_enforceEndRotation || !LastPoseInModel.Equals(modelTransform))
				{
					short boneIx = parentIndices[i];
					Transform localTransform = HavokPosing.GetLocalTransform(poseIn, i);
					Transform modelTransform2 = HavokPosing.GetModelTransform(poseOut, boneIx);
					modelTransform2.Position += Vector3.Transform(localTransform.Position, modelTransform2.Rotation);
					modelTransform2.Rotation = Quaternion.Normalize(modelTransform2.Rotation * localTransform.Rotation);
					modelTransform2.Scale *= localTransform.Scale;
					HavokPosing.SetModelTransform(poseOut, i, modelTransform2);
				}
			}
		}
		LastPoseInModel = modelTransform;
	}

	private unsafe void ApplyModelPoseDynamic(hkaPose* poseIn, hkaPose* poseOut)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		hkArray<short> parentIndices = ((hkaSkeleton)((hkaPose)poseOut).Skeleton).ParentIndices;
		short firstJointIdx = IkSetup->m_firstJointIdx;
		for (int i = 1; i < ((hkaSkeleton)((hkaPose)poseOut).Skeleton).Bones.Length; i++)
		{
			if (i == firstJointIdx || HavokPosing.IsBoneDescendantOf(parentIndices, i, firstJointIdx))
			{
				Unsafe.Write(((hkaPose)poseOut).AccessBoneModelSpace(i, (PropagateOrNot)1), ((hkaPose)poseIn).ModelPose[i]);
			}
		}
	}

	public void Dispose()
	{
		LastPoseInModel = null;
		IsDisposed = true;
		AllocIkSetup.Dispose();
		GC.SuppressFinalize(this);
	}
}
