using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Container.Array;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using FFXIVClientStructs.Havok.Common.Base.Object;
using Ktisis.Editor.Posing.Ik.Ccd;
using Ktisis.Editor.Posing.Ik.TwoJoints;
using Ktisis.Interop;
using Ktisis.Interop.Hooking;
using Ktisis.Structs.Havok;

namespace Ktisis.Editor.Posing.Ik;

public sealed class IkModule : HookModule
{
	public unsafe delegate nint SolveTwoJointsDelegate(byte* result, TwoJointsIkSetup* setup, hkaPose* pose);

	public unsafe delegate nint SolveCcdDelegate(CcdIkSolver* solver, byte* result, hkArray<CcdIkConstraint>* constraints, hkaPose* hkaPose);

	public unsafe delegate nint InitHkaPoseDelegate(hkaPose* pose, int space, nint unk, hkArray<hkQsTransformf>* transforms);

	private delegate void UpdateAnimationDelegate(nint a1);

	private readonly PosingManager Manager;

	private readonly List<IIkController> Controllers = new List<IIkController>();

	[Signature(/*Could not decode attribute arguments.*/)]
	private unsafe nint** CcdVfTable = null;

	[Signature("E8 ?? ?? ?? ?? 0F 28 55 10")]
	public SolveTwoJointsDelegate SolveTwoJoints;

	[Signature("E8 ?? ?? ?? ?? 8B 45 EF 48 8B 7D F7")]
	public SolveCcdDelegate SolveCcd;

	[Signature("48 89 5C 24 ?? 48 89 6C 24 ?? 56 57 41 56 48 83 EC 30 48 8B 01 49 8B E9")]
	public InitHkaPoseDelegate InitHkaPose;

	[Signature("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 F3 0F 10 81 ?? ?? ?? ?? 48 8B FA", DetourName = "UpdateAnimationDetour")]
	private Hook<UpdateAnimationDelegate> UpdateAnimationHook;

	public unsafe IkModule(IHookMediator hook, PosingManager manager)
		: base(hook)
	{
		Manager = manager;
	}

	public override bool Initialize()
	{
		bool num = base.Initialize();
		if (num)
		{
			EnableAll();
		}
		return num;
	}

	public IIkController CreateController()
	{
		CcdSolver ccd = CreateCcdSolver();
		TwoJointsSolver twoJoints = CreateTwoJointsSolver();
		IkController ikController = new IkController(this, ccd, twoJoints);
		lock (Controllers)
		{
			Controllers.Add(ikController);
			return ikController;
		}
	}

	public bool RemoveController(IIkController controller)
	{
		lock (Controllers)
		{
			return Controllers.Remove(controller);
		}
	}

	public unsafe CcdSolver CreateCcdSolver(int iterations = 8, float gain = 0.5f)
	{
		Alloc<CcdIkSolver> alloc = new Alloc<CcdIkSolver>(8uL);
		alloc.Data->_vfTable = CcdVfTable;
		((hkReferencedObject)(&alloc.Data->hkRefObject)).MemSizeAndRefCount = 4294901761u;
		alloc.Data->m_iterations = iterations;
		alloc.Data->m_gain = gain;
		CcdSolver ccdSolver = new CcdSolver(this, alloc);
		ccdSolver.Setup();
		return ccdSolver;
	}

	public TwoJointsSolver CreateTwoJointsSolver()
	{
		TwoJointsSolver twoJointsSolver = new TwoJointsSolver(this);
		twoJointsSolver.Setup();
		return twoJointsSolver;
	}

	private void UpdateAnimationDetour(nint a1)
	{
		UpdateAnimationHook.Original(a1);
		if (Manager.IsSolvingIk)
		{
			return;
		}
		try
		{
			UpdateIkPoses();
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to update IK poses:\n{value}");
		}
	}

	private void UpdateIkPoses()
	{
		if (!Manager.IsValid)
		{
			return;
		}
		IEnumerable<IIkController> enumerable;
		lock (Controllers)
		{
			enumerable = Controllers.Where((IIkController controller) => controller.IsEnabled()).ToList();
		}
		Manager.IsIkEnabled = enumerable.Any();
		try
		{
			Manager.IsSolvingIk = true;
			foreach (IIkController item in enumerable)
			{
				item.Solve(Manager.IsEnabled);
			}
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to update IK controllers:\n{value}");
		}
		finally
		{
			Manager.IsSolvingIk = false;
		}
	}
}
