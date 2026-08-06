using System;
using System.Runtime.CompilerServices;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Plugin;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Container.String;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using Ktisis.Data.Json;
using Ktisis.Editor.Context;
using Ktisis.Interop.Hooking;
using Ktisis.Interop.Ipc;
using Ktisis.Scene.Entities.Game;
using Ktisis.Services.Game;

namespace Ktisis.Editor.Posing;

public sealed class PosingModule : HookModule
{
	private delegate ulong SetBoneModelSpaceDelegate(nint partial, ushort boneId, nint transform, bool enableSecondary, bool enablePropagate);

	private unsafe delegate void SyncModelSpaceDelegate(hkaPose* pose);

	private unsafe delegate hkQsTransformf* CalcBoneModelSpaceDelegate(hkaPose* pose, int boneIdx);

	private delegate nint LookAtIKDelegate(nint a1, nint a2, nint a3, float a4, nint a5, nint a6);

	private delegate nint KineDriverDelegate(nint a1, nint a2);

	private delegate byte AnimFrozenDelegate(nint a1, int a2);

	private delegate void UpdatePosDelegate(nint gameObject);

	private unsafe delegate byte SetSkeletonDelegate(Skeleton* skeleton, ushort partialId, nint a3);

	private delegate nint DisconnectDelegate(nint a1);

	private readonly PosingManager Manager;

	private readonly ActorService _actors;

	private readonly IpcProvider _ipc;

	[Signature("48 8B C4 48 89 58 18 55 56 57 41 54 41 55 41 56 41 57 48 81 EC ?? ?? ?? ?? 0F 29 70 B8 0F 29 78 A8 44 0F 29 40 ?? 44 0F 29 48 ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 8B B1", DetourName = "SetBoneModelSpace")]
	private Hook<SetBoneModelSpaceDelegate> _setBoneModelSpaceHook;

	[Signature("48 83 EC 18 80 79 38 00", DetourName = "SyncModelSpace")]
	private Hook<SyncModelSpaceDelegate> _syncModelSpaceHook;

	[Signature("40 53 48 83 EC 10 4C 8B 49 28", DetourName = "CalcBoneModelSpace")]
	private Hook<CalcBoneModelSpaceDelegate> _calcBoneModelSpaceHook;

	[Signature("48 8B C4 48 89 58 08 48 89 70 10 F3 0F 11 58", DetourName = "LookAtIK")]
	private Hook<LookAtIKDelegate> _lookAtIKHook;

	[Signature("48 8B C4 55 57 48 83 EC 58", DetourName = "KineDriverDetour")]
	private Hook<KineDriverDelegate> _kineDriverHook;

	[Signature("E8 ?? ?? ?? ?? 0F B6 F8 84 C0 74 12", DetourName = "AnimFrozen")]
	private Hook<AnimFrozenDelegate> _animFrozenHook;

	[Signature("E8 ?? ?? ?? ?? 84 DB 74 3A", DetourName = "UpdatePosDetour")]
	private Hook<UpdatePosDelegate> _updatePosHook;

	[Signature("E8 ?? ?? ?? ?? 48 C1 E5 08", DetourName = "SetSkeletonDetour")]
	private Hook<SetSkeletonDelegate> _setSkeletonHook;

	[Signature("E8 ?? ?? ?? ?? 84 C0 0F 44 FE", DetourName = "DisconnectDetour")]
	private Hook<DisconnectDelegate> _disconnectHook;

	public bool IsEnabled { get; private set; }

	public event SkeletonInitHandler? OnSkeletonInit;

	public event Action? OnDisconnect;

	public PosingModule(IHookMediator hook, PosingManager manager, ActorService actors, ContextManager contextManager, IDalamudPluginInterface dpi, JsonFileSerializer fileSerializer)
		: base(hook)
	{
		Manager = manager;
		_actors = actors;
		_ipc = new IpcProvider(contextManager, dpi, fileSerializer);
	}

	public override void EnableAll()
	{
		base.EnableAll();
		IsEnabled = true;
		_ipc.InvokePosingChanged(IsEnabled);
	}

	public override void DisableAll()
	{
		base.DisableAll();
		IsEnabled = false;
		_ipc.InvokePosingChanged(IsEnabled);
	}

	private ulong SetBoneModelSpace(nint partial, ushort boneId, nint transform, bool enableSecondary, bool enablePropagate)
	{
		return boneId;
	}

	private unsafe void SyncModelSpace(hkaPose* pose)
	{
		if (Manager.IsSolvingIk)
		{
			_syncModelSpaceHook.Original(pose);
		}
	}

	public unsafe void SyncFaceModelSpace(ActorEntity actor)
	{
		hkaPose* havokPose = ((PartialSkeleton)((byte*)((Skeleton)((CharacterBase)actor.GetCharacter()).Skeleton).PartialSkeletons + Unsafe.SizeOf<PartialSkeleton>())).GetHavokPose(0);
		_syncModelSpaceHook.Original(havokPose);
	}

	private unsafe hkQsTransformf* CalcBoneModelSpace(hkaPose* pose, int boneIdx)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (Manager.IsSolvingIk)
		{
			return _calcBoneModelSpaceHook.Original(pose, boneIdx);
		}
		if (boneIdx == 1)
		{
			hkaBone val = ((hkaSkeleton)((hkaPose)pose).Skeleton).Bones[boneIdx];
			if (((hkStringPtr)(ref val.Name)).String == "n_hara")
			{
				HavokPosing.CalcCachedAbdomenModelTransform(pose, boneIdx);
			}
		}
		return (hkQsTransformf*)((byte*)((hkaPose)pose).ModelPose.Data + (nint)boneIdx * (nint)Unsafe.SizeOf<hkQsTransformf>());
	}

	private nint LookAtIK(nint a1, nint a2, nint a3, float a4, nint a5, nint a6)
	{
		return IntPtr.Zero;
	}

	private nint KineDriverDetour(nint a1, nint a2)
	{
		return IntPtr.Zero;
	}

	private byte AnimFrozen(nint a1, int a2)
	{
		return 1;
	}

	private void UpdatePosDetour(nint gameObject)
	{
	}

	private unsafe byte SetSkeletonDetour(Skeleton* skeleton, ushort partialId, nint a3)
	{
		byte result = _setSkeletonHook.Original(skeleton, partialId, a3);
		try
		{
			HandleRestoreState(skeleton, partialId);
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to handle SetSkeleton:\n{value}");
		}
		return result;
	}

	private unsafe void HandleRestoreState(Skeleton* skeleton, ushort partialId)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (!Manager.IsValid || !IsEnabled || ((Skeleton)skeleton).PartialSkeletons == null)
		{
			return;
		}
		PartialSkeleton val = ((PartialSkeleton*)((Skeleton)skeleton).PartialSkeletons)[(int)partialId];
		hkaPose* havokPose = ((PartialSkeleton)(ref val)).GetHavokPose(0);
		if (havokPose == null)
		{
			return;
		}
		_syncModelSpaceHook.Original(havokPose);
		IGameObject skeletonOwner = _actors.GetSkeletonOwner(skeleton);
		if (skeletonOwner != null)
		{
			Ktisis.Log.Verbose($"Restoring partial {partialId} for {skeletonOwner.Name} ({skeletonOwner.ObjectIndex})");
			if (partialId == 0)
			{
				_updatePosHook.Original(skeletonOwner.Address);
			}
			this.OnSkeletonInit?.Invoke(skeletonOwner, skeleton, partialId);
		}
	}

	private nint DisconnectDetour(nint a1)
	{
		try
		{
			this.OnDisconnect?.Invoke();
		}
		catch (Exception ex)
		{
			Ktisis.Log.Error(ex.ToString());
		}
		return _disconnectHook.Original(a1);
	}
}
