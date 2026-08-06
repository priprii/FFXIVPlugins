using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Container.Array;
using Ktisis.Common.Extensions;
using Ktisis.Common.Utility;
using Ktisis.Interop;
using Ktisis.Structs.Havok;

namespace Ktisis.Editor.Posing.Ik.Ccd;

public class CcdSolver : IDisposable
{
	private readonly IkModule _module;

	private readonly Alloc<CcdIkSolver> AllocSolver;

	private readonly Alloc<CcdIkConstraint> AllocIkConstraint = new Alloc<CcdIkConstraint>(16uL);

	private readonly Alloc<hkArray<CcdIkConstraint>> AllocHkArray = new Alloc<hkArray<CcdIkConstraint>>(16uL);

	private unsafe CcdIkSolver* IkSolver => AllocSolver.Data;

	public unsafe CcdIkConstraint* IkConstraint => AllocIkConstraint.Data;

	public CcdSolver(IkModule module, Alloc<CcdIkSolver> solver)
	{
		_module = module;
		AllocSolver = solver;
	}

	public unsafe void Setup()
	{
		if (AllocIkConstraint.Address == IntPtr.Zero)
		{
			throw new Exception("Allocation for IkConstraint failed.");
		}
		IkConstraint->m_startBone = -1;
		IkConstraint->m_endBone = -1;
		IkConstraint->m_targetMS = Vector4.Zero;
		HavokEx.Initialize(AllocHkArray.Data, IkConstraint, 1);
	}

	public unsafe void Solve(hkaPose* poseIn, hkaPose* poseOut, bool frozen = false)
	{
		if (poseOut != null && ((hkaPose)poseOut).Skeleton != null)
		{
			if (frozen)
			{
				((hkaPose)poseIn).SetToReferencePose();
				((hkaPose)poseIn).SyncModelSpace();
				UpdateModelPose(poseIn, poseOut);
			}
			byte b = 0;
			_module.SolveCcd(IkSolver, &b, AllocHkArray.Data, poseIn);
			((hkaPose)poseIn).SyncModelSpace();
			if (frozen)
			{
				ApplyModelPoseStatic(poseIn, poseOut);
			}
			else
			{
				ApplyModelPoseDynamic(poseIn, poseOut);
			}
		}
	}

	public unsafe void SolveGroup(hkaPose* poseIn, hkaPose* poseOut, CcdGroup group, bool frozen = false)
	{
		if (group.IsEnabled)
		{
			CcdIkSolver* ikSolver = IkSolver;
			CcdIkConstraint* ikConstraint = IkConstraint;
			ikConstraint->m_startBone = group.StartBoneIndex;
			ikConstraint->m_endBone = group.EndBoneIndex;
			ikConstraint->m_targetMS = new Vector4(group.TargetPosition, 0f);
			ikSolver->m_iterations = group.Iterations;
			ikSolver->m_gain = group.Gain;
			Solve(poseIn, poseOut, frozen);
		}
	}

	private unsafe void UpdateModelPose(hkaPose* poseIn, hkaPose* poseOut)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		short startBone = IkConstraint->m_startBone;
		for (int i = 1; i < ((hkaSkeleton)((hkaPose)poseIn).Skeleton).Bones.Length; i++)
		{
			if (HavokPosing.IsBoneDescendantOf(((hkaSkeleton)((hkaPose)poseOut).Skeleton).ParentIndices, startBone, i))
			{
				Unsafe.Write(((hkaPose)poseIn).AccessBoneModelSpace(i, (PropagateOrNot)1), ((hkaPose)poseOut).ModelPose[i]);
			}
		}
	}

	private unsafe void ApplyModelPoseStatic(hkaPose* poseIn, hkaPose* poseOut)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		hkArray<short> parentIndices = ((hkaSkeleton)((hkaPose)poseOut).Skeleton).ParentIndices;
		short startBone = IkConstraint->m_startBone;
		for (int i = 1; i < ((hkaSkeleton)((hkaPose)poseOut).Skeleton).Bones.Length; i++)
		{
			if (i == startBone || HavokPosing.IsBoneDescendantOf(parentIndices, i, startBone))
			{
				Transform modelTransform = HavokPosing.GetModelTransform(poseIn, i);
				HavokPosing.SetModelTransform(poseOut, i, modelTransform);
			}
		}
	}

	private unsafe void ApplyModelPoseDynamic(hkaPose* poseIn, hkaPose* poseOut)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		hkArray<short> parentIndices = ((hkaSkeleton)((hkaPose)poseOut).Skeleton).ParentIndices;
		short startBone = IkConstraint->m_startBone;
		for (int i = 1; i < ((hkaSkeleton)((hkaPose)poseOut).Skeleton).Bones.Length; i++)
		{
			if (i == startBone || HavokPosing.IsBoneDescendantOf(parentIndices, i, startBone))
			{
				Unsafe.Write(((hkaPose)poseOut).AccessBoneModelSpace(i, (PropagateOrNot)1), ((hkaPose)poseIn).ModelPose[i]);
			}
		}
	}

	public void Dispose()
	{
		AllocSolver.Dispose();
		AllocIkConstraint.Dispose();
		AllocHkArray.Dispose();
		GC.SuppressFinalize(this);
	}
}
