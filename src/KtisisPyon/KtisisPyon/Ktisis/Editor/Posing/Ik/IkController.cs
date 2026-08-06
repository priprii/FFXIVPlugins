using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Container.Array;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using Ktisis.Common.Extensions;
using Ktisis.Data.Config.Bones;
using Ktisis.Editor.Posing.Data;
using Ktisis.Editor.Posing.Ik.Ccd;
using Ktisis.Editor.Posing.Ik.TwoJoints;
using Ktisis.Editor.Posing.Ik.Types;
using Ktisis.Interop;
using Ktisis.Scene.Decor;

namespace Ktisis.Editor.Posing.Ik;

public class IkController : IIkController
{
	private readonly IkModule _module;

	private ISkeleton? Skeleton;

	private readonly Alloc<hkaPose> _allocPose = new Alloc<hkaPose>(16uL);

	private bool IsInitialized;

	private readonly CcdSolver _ccd;

	private readonly TwoJointsSolver _twoJoints;

	private readonly Dictionary<string, IIkGroup> Groups = new Dictionary<string, IIkGroup>();

	private bool _isDestroyed;

	private unsafe hkaPose* Pose => _allocPose.Data;

	public int GroupCount => Groups.Count;

	public IkController(IkModule module, CcdSolver ccd, TwoJointsSolver twoJoints)
	{
		_module = module;
		_ccd = ccd;
		_twoJoints = twoJoints;
	}

	public void Setup(ISkeleton skeleton)
	{
		Skeleton = skeleton;
	}

	private unsafe void Initialize(hkaPose* pose)
	{
		if (_allocPose.Address == IntPtr.Zero)
		{
			throw new Exception("Allocation for hkaPose failed.");
		}
		((hkaPose)Pose).Skeleton = ((hkaPose)pose).Skeleton;
		HavokEx.Initialize<hkQsTransformf>(&((hkaPose)Pose).LocalPose, (hkQsTransformf*)null, 0);
		HavokEx.Initialize<hkQsTransformf>(&((hkaPose)Pose).ModelPose, (hkQsTransformf*)null, 0);
		HavokEx.Initialize(&((hkaPose)Pose).BoneFlags, null);
		HavokEx.Initialize(&((hkaPose)Pose).FloatSlotValues, null);
		((hkaPose)Pose).ModelInSync = 0;
		hkArray<hkQsTransformf>* syncedPoseLocalSpace = ((hkaPose)pose).GetSyncedPoseLocalSpace();
		_module.InitHkaPose(Pose, 1, (nint)syncedPoseLocalSpace, syncedPoseLocalSpace);
		IsInitialized = true;
	}

	public unsafe void Solve(bool frozen = false)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (Skeleton == null)
		{
			return;
		}
		Skeleton* skeleton = Skeleton.GetSkeleton();
		if (skeleton == null || ((Skeleton)skeleton).PartialSkeletons == null)
		{
			return;
		}
		PartialSkeleton partialSkeletons = *((Skeleton)skeleton).PartialSkeletons;
		if (((PartialSkeleton)(ref partialSkeletons)).HavokPoses.IsEmpty || partialSkeletons.SkeletonResourceHandle == null)
		{
			return;
		}
		hkaPose* havokPose = ((PartialSkeleton)(ref partialSkeletons)).GetHavokPose(0);
		if (havokPose != null && ((hkaPose)havokPose).Skeleton != null)
		{
			uint id = ((SkeletonResourceHandle)partialSkeletons.SkeletonResourceHandle).Id;
			List<IIkGroup> list = Groups.Values.Where((IIkGroup group) => group.IsEnabled && group.SkeletonId == id).ToList();
			if (list.Count != 0)
			{
				Solve(havokPose, list, frozen);
			}
		}
	}

	private unsafe void Solve(hkaPose* pose, IEnumerable<IIkGroup> groups, bool frozen)
	{
		if (!IsInitialized || ((hkaPose)pose).Skeleton != ((hkaPose)Pose).Skeleton)
		{
			Initialize(pose);
		}
		if (!frozen)
		{
			((hkaPose)Pose).SetPoseLocalSpace(&((hkaPose)pose).LocalPose);
			((hkaPose)Pose).SyncModelSpace();
		}
		foreach (IIkGroup group in groups)
		{
			if (!(group is TwoJointsGroup twoJointsGroup))
			{
				if (group is CcdGroup ccdGroup)
				{
					_ccd.SolveGroup(Pose, pose, ccdGroup, frozen);
				}
			}
			else
			{
				_twoJoints.SolveGroup(Pose, pose, twoJointsGroup, frozen);
			}
		}
	}

	public IEnumerable<(string name, IIkGroup group)> GetGroups()
	{
		return Groups.Select<KeyValuePair<string, IIkGroup>, (string, IIkGroup)>((KeyValuePair<string, IIkGroup> pair) => (Key: pair.Key, Value: pair.Value));
	}

	public unsafe bool TrySetupGroup(string name, CcdGroupParams param, out CcdGroup? group)
	{
		group = null;
		Ktisis.Log.Verbose("Setting up group for CCD IK: " + name);
		if (Skeleton != null)
		{
			SkeletonPoseData skeletonPoseData = SkeletonPoseData.TryGet(Skeleton, 0, 0);
			if (skeletonPoseData != null)
			{
				if (Groups.TryGetValue(name, out IIkGroup value))
				{
					group = value as CcdGroup;
				}
				if (group == null)
				{
					group = new CcdGroup();
				}
				short num = skeletonPoseData.TryResolveBone(param.StartBone);
				short num2 = skeletonPoseData.TryResolveBone(param.EndBone);
				if (num == -1 || num2 == -1)
				{
					Ktisis.Log.Warning($"Resolve failed: {num} {num2}");
					return false;
				}
				group.StartBoneIndex = num;
				group.EndBoneIndex = num2;
				Ktisis.Log.Verbose($"Resolved bones: {num} {num2}");
				group.SkeletonId = ((SkeletonResourceHandle)skeletonPoseData.Partial.SkeletonResourceHandle).Id;
				Groups[name] = group;
				return true;
			}
		}
		return false;
	}

	public unsafe bool TrySetupGroup(string name, TwoJointsGroupParams param, out TwoJointsGroup? group)
	{
		group = null;
		Ktisis.Log.Verbose("Setting up group for TwoJoints IK: " + name);
		if (Skeleton != null)
		{
			SkeletonPoseData skeletonPoseData = SkeletonPoseData.TryGet(Skeleton, 0, 0);
			if (skeletonPoseData != null)
			{
				if (Groups.TryGetValue(name, out IIkGroup value))
				{
					group = value as TwoJointsGroup;
				}
				if ((object)group == null)
				{
					group = new TwoJointsGroup
					{
						HingeAxis = ((param.Type == TwoJointsType.Leg) ? (-Vector3.UnitZ) : Vector3.UnitZ)
					};
				}
				short num = skeletonPoseData.TryResolveBone(param.FirstBone);
				short num2 = skeletonPoseData.TryResolveBone(param.SecondBone);
				short num3 = skeletonPoseData.TryResolveBone(param.EndBone);
				if (num == -1 || num2 == -1 || num3 == -1)
				{
					return false;
				}
				group.FirstBoneIndex = num;
				group.FirstTwistIndex = skeletonPoseData.TryResolveBone(param.FirstTwist);
				group.SecondBoneIndex = num2;
				group.SecondTwistIndex = skeletonPoseData.TryResolveBone(param.SecondTwist);
				group.EndBoneIndex = num3;
				Ktisis.Log.Verbose($"Resolved bones: {num} {num2} {num3} ({group.FirstTwistIndex}, {group.SecondTwistIndex})");
				group.SkeletonId = ((SkeletonResourceHandle)skeletonPoseData.Partial.SkeletonResourceHandle).Id;
				Groups[name] = group;
				return true;
			}
		}
		return false;
	}

	public bool IsEnabled()
	{
		return Groups.Values.Any((IIkGroup group) => group.IsEnabled);
	}

	public void Destroy()
	{
		if (_isDestroyed)
		{
			throw new Exception("IK controller is already disposed.");
		}
		_ccd.Dispose();
		_twoJoints.Dispose();
		_isDestroyed = _module.RemoveController(this);
	}
}
