using System;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Ktisis.Common.Extensions;
using Ktisis.Common.Utility;
using Ktisis.Editor.Camera.Types;
using Ktisis.Interop.Hooking;
using Ktisis.Interop.Ipc;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Types;
using Ktisis.Services.Game;
using Ktisis.Structs.Actors;
using Ktisis.Structs.Camera;
using Ktisis.Structs.GPose;

namespace Ktisis.Scene.Modules.Actors;

public class ActorModule : SceneModule
{
	private delegate void AddCharacterDelegate(nint a1, nint a2, ulong a3, nint a4);

	private unsafe delegate nint RemoveCharacterDelegate(GPoseState* gpose, GameObject* gameObject);

	private unsafe delegate char ActorLookAtDelegate(ActorGaze* writeTo, Gaze* readFrom, GazeControl bodyPart, nint unk4);

	private delegate void ControlGazeDelegate(nint a1);

	private readonly ActorService _actors;

	private readonly IObjectTable _objectTable;

	private readonly IFramework _framework;

	private readonly GroupPoseModule _gpose;

	private readonly ActorSpawner _spawner;

	[Signature("40 56 57 48 83 EC 38 48 89 5C 24 ??", DetourName = "AddCharacterDetour")]
	private Hook<AddCharacterDelegate>? AddCharacterHook;

	[Signature("45 33 D2 4C 8D 81 ?? ?? ?? ?? 41 8B C2 4C 8B C9 49 3B 10")]
	private RemoveCharacterDelegate _removeCharacter;

	[Signature("E8 ?? ?? ?? ?? 8B D6 48 8B CF E8 ?? ?? ?? ?? EB 2A")]
	private ActorLookAtDelegate _actorLookAt;

	[Signature("E8 ?? ?? ?? ?? 48 83 C3 08 48 83 EF 01 75 CF", DetourName = "ControlGazeDetour")]
	private Hook<ControlGazeDelegate>? ControlGazeHook;

	public ActorModule(IHookMediator hook, ISceneManager scene, ActorService actors, IObjectTable objectTable, IFramework framework, GroupPoseModule gpose)
		: base(hook, scene)
	{
		_actors = actors;
		_objectTable = objectTable;
		_framework = framework;
		_gpose = gpose;
		_spawner = hook.Create<ActorSpawner>(Array.Empty<object>());
	}

	public override void Setup()
	{
		foreach (IGameObject gPoseActor in _actors.GetGPoseActors())
		{
			AddActor(gPoseActor, addCompanion: false);
		}
		Subscribe();
		EnableAll();
		_spawner.TryInitialize();
	}

	private unsafe void Subscribe()
	{
		Scene.Context.Characters.OnDisableDraw += OnDisableDraw;
	}

	private unsafe void OnDisableDraw(IGameObject gameObject, DrawObject* drawObject)
	{
		if (base.IsInit && Scene.IsValid)
		{
			ActorEntity entityForActor = Scene.GetEntityForActor(gameObject);
			if (entityForActor != null)
			{
				entityForActor.Address = IntPtr.Zero;
				Ktisis.Log.Debug($"Invalidated object address for entity '{entityForActor.Name}' ({gameObject.ObjectIndex})");
			}
		}
	}

	public async Task<ActorEntity> Spawn()
	{
		IPlayerCharacter localPlayer = _objectTable.LocalPlayer;
		if (localPlayer == null)
		{
			throw new Exception("Local player not found.");
		}
		nint num = await _spawner.CreateActor((IGameObject)(object)localPlayer);
		if (num == IntPtr.Zero)
		{
			return null;
		}
		ActorEntity actorEntity = AddSpawnedActor(num);
		actorEntity.Actor.SetName(PlayerNameUtil.CalcActorName(actorEntity.Actor.ObjectIndex));
		actorEntity.Actor.SetWorld((ushort)localPlayer.CurrentWorld.RowId);
		ReassignParentIndex(actorEntity.Actor);
		return actorEntity;
	}

	public async Task<ActorEntity> AddFromOverworld(IGameObject actor)
	{
		if (!_spawner.IsInit)
		{
			throw new Exception("Actor spawner is uninitialized.");
		}
		nint num = await _spawner.CreateActor(actor);
		if (num == IntPtr.Zero)
		{
			return null;
		}
		ActorEntity actorEntity = AddSpawnedActor(num);
		actorEntity.Actor.SetTargetable(targetable: true);
		actorEntity.Visible = true;
		return actorEntity;
	}

	private ActorEntity AddSpawnedActor(nint address)
	{
		ActorEntity? obj = AddActor(address, addCompanion: false) ?? throw new Exception("Failed to create entity for spawned actor.");
		obj.IsManaged = true;
		return obj;
	}

	public unsafe void Delete(ActorEntity actor, bool force = false)
	{
		if (_gpose.IsPrimaryActor(actor) && !force)
		{
			Ktisis.Log.Warning("Refusing to delete primary actor.");
			return;
		}
		GPoseState* gpose = _gpose.GetGPoseState();
		if (gpose == null)
		{
			return;
		}
		Scene.Context.Characters.Mcdf.RevertIfTouched(actor.Actor);
		GameObject* gameObject = (GameObject*)actor.Actor.Address;
		_framework.RunOnFrameworkThread((Action)delegate
		{
			ClientObjectManager* ptr = ClientObjectManager.Instance();
			ushort num = (ushort)((ClientObjectManager)ptr).GetIndexByObject(gameObject);
			_removeCharacter(gpose, gameObject);
			if (num != ushort.MaxValue)
			{
				((ClientObjectManager)ptr).DeleteObjectByIndex(num, (byte)1);
			}
		});
		actor.Remove();
	}

	private void ReassignParentIndex(IGameObject gameObject)
	{
		IpcManager ipc = Scene.Context.Plugin.Ipc;
		if (ipc.IsPenumbraActive)
		{
			ipc.GetPenumbraIpc().SetAssignedParentIndex(gameObject, gameObject.ObjectIndex);
		}
		if (ipc.IsCustomizeActive)
		{
			CustomizeIpcProvider customizeIpc = ipc.GetCustomizeIpc();
			if (customizeIpc.IsCompatible())
			{
				customizeIpc.SetCutsceneParentIndex(gameObject.ObjectIndex, gameObject.ObjectIndex);
			}
		}
	}

	private ActorEntity? AddActor(nint address, bool addCompanion)
	{
		IGameObject address2 = _actors.GetAddress(address);
		if (address2 != null && address2.ObjectIndex != 200)
		{
			return AddActor(address2, addCompanion);
		}
		Ktisis.Log.Warning($"Actor address at 0x{address:X} is invalid.");
		return null;
	}

	private ActorEntity? AddActor(IGameObject actor, bool addCompanion)
	{
		if (!actor.IsValid())
		{
			Ktisis.Log.Warning($"Actor address at 0x{actor.Address:X} is invalid.");
			return null;
		}
		ActorEntity result = Scene.Factory.BuildActor(actor).Add();
		if (addCompanion)
		{
			AddCompanion(actor);
		}
		return result;
	}

	private unsafe void AddCompanion(IGameObject owner)
	{
		Character* address = (Character*)owner.Address;
		if (address != null && ((Character)address).CompanionObject != null)
		{
			IGameObject address2 = _actors.GetAddress((nint)((Character)address).CompanionObject);
			bool flag = ((address2 == null || address2.ObjectIndex == 0) ? true : false);
			if (!flag && address2.IsValid())
			{
				Scene.Factory.BuildActor(address2).Add();
			}
		}
	}

	public unsafe void RefreshGPoseActors()
	{
		foreach (ActorEntity item in Scene.Children.Where((SceneEntity entity) => entity is ActorEntity).Cast<ActorEntity>().ToList())
		{
			if (item.IsValid)
			{
				ActorEntity entityForActor = Scene.GetEntityForActor(item.Actor);
				if (entityForActor != null && entityForActor.Character == null)
				{
					Delete(entityForActor);
				}
			}
		}
		foreach (IGameObject gPoseActor in _actors.GetGPoseActors())
		{
			if (Scene.GetEntityForActor(gPoseActor) == null)
			{
				AddActor(gPoseActor, addCompanion: false);
			}
		}
	}

	private void AddCharacterDetour(nint gpose, nint address, ulong id, nint a4)
	{
		AddCharacterHook.Original(gpose, address, id, a4);
		if (!CheckValid())
		{
			return;
		}
		try
		{
			if (id != 3758096384u)
			{
				AddActor(address, addCompanion: true);
			}
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to handle character add for 0x{address:X}:\n{value}");
		}
	}

	private unsafe void ControlGazeDetour(nint a1)
	{
		if (!CheckValid())
		{
			return;
		}
		foreach (ActorEntity item in Scene.Children.OfType<ActorEntity>().ToList())
		{
			if (!item.IsValid || !item.Gaze.HasValue || item.Actor.Address != (IntPtr)(a1 - 3456))
			{
				continue;
			}
			CharacterEx* ptr = (CharacterEx*)(a1 - 3456);
			ActorGaze value = item.Gaze.Value;
			for (int i = -1; i < 3; i++)
			{
				GazeControl gazeControl = (GazeControl)i;
				Gaze value2 = value[gazeControl];
				if (value2.Mode == GazeMode.Disabled)
				{
					continue;
				}
				if (value2.Mode == GazeMode._KtisisFollowCam_)
				{
					if (Scene.Context.Cameras.IsWorkCameraActive)
					{
						WorkCamera workCamera = (WorkCamera)Scene.Context.Cameras.Current;
						value2.Pos = workCamera.Position;
						value[gazeControl] = value2;
					}
					else
					{
						GameCameraEx* active = GameCameraEx.GetActive();
						if (active != null)
						{
							value2.Pos = active->Position;
							value[gazeControl] = value2;
						}
					}
					value2.Mode = GazeMode.Target;
				}
				if (value2.Mode == GazeMode._KtisisFollowGizmo_)
				{
					value2.Mode = GazeMode.Target;
				}
				_actorLookAt(&ptr->Gaze, &value2, gazeControl, IntPtr.Zero);
				if (gazeControl == GazeControl.All)
				{
					break;
				}
			}
		}
		ControlGazeHook.Original(a1);
	}

	public override void Dispose()
	{
		base.Dispose();
		_spawner.Dispose();
		GC.SuppressFinalize(this);
	}
}
