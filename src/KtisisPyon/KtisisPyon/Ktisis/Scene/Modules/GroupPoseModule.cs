using System;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Ktisis.Common.Extensions;
using Ktisis.Interop.Hooking;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Types;
using Ktisis.Structs.GPose;

namespace Ktisis.Scene.Modules;

public class GroupPoseModule : SceneModule
{
	private unsafe delegate GPoseState* GetGPoseStateDelegate();

	private delegate void UpdateGposeTarNameDelegate(nint a1);

	private readonly IObjectTable _objectTable;

	[Signature("E8 ?? ?? ?? ?? 0F B7 56 3C")]
	private GetGPoseStateDelegate? _getGPoseState;

	[Signature("E8 ?? ?? ?? ?? 48 8D 8D ?? ?? ?? ?? 48 83 C4 28", DetourName = "UpdateGposeTarNameDetour")]
	private Hook<UpdateGposeTarNameDelegate>? UpdateGposeTarNameHook;

	public GroupPoseModule(IHookMediator hook, ISceneManager scene, IObjectTable objectTable)
		: base(hook, scene)
	{
		_objectTable = objectTable;
	}

	public override void Setup()
	{
		EnableAll();
	}

	public unsafe GPoseState* GetGPoseState()
	{
		if (_getGPoseState == null)
		{
			return null;
		}
		return _getGPoseState();
	}

	public bool IsPrimaryActor(ActorEntity actor)
	{
		ushort objectIndex = actor.Actor.ObjectIndex;
		if ((uint)(objectIndex - 200) <= 1u)
		{
			return true;
		}
		return false;
	}

	private unsafe IGameObject? GetGposeTarget()
	{
		nint gPoseTarget = (nint)((TargetSystem)TargetSystem.Instance()).GPoseTarget;
		return _objectTable.CreateObjectReference((IntPtr)gPoseTarget);
	}

	private unsafe void UpdateGposeTarNameDetour(nint a1)
	{
		if (!CheckValid())
		{
			UpdateGposeTarNameHook.Original(a1);
			return;
		}
		IGameObject gposeTarget = GetGposeTarget();
		if (gposeTarget != null && gposeTarget.IsPcCharacter())
		{
			string nameOrFallback = gposeTarget.GetNameOrFallback(Scene.Context);
			for (int i = 0; i < nameOrFallback.Length; i++)
			{
				*(char*)(a1 + 488 + i) = nameOrFallback[i];
			}
		}
		UpdateGposeTarNameHook.Original(a1);
	}
}
