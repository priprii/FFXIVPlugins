using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;
using Ktisis.Data.Files;
using Ktisis.Data.Mcdf;
using Ktisis.Editor.Characters.Handlers;
using Ktisis.Editor.Characters.State;
using Ktisis.Editor.Characters.Types;
using Ktisis.Editor.Context.Types;
using Ktisis.GameData.Excel.Types;
using Ktisis.Interop.Hooking;
using Ktisis.Scene.Entities.Game;

namespace Ktisis.Editor.Characters;

public class CharacterManager : ICharacterManager, IDisposable
{
	private readonly IEditorContext _context;

	private readonly IObjectTable _objectTable;

	private readonly HookScope _scope;

	private readonly IFramework _framework;

	private readonly Dictionary<ushort, Transform> _savedTransforms = new Dictionary<ushort, Transform>();

	public bool IsValid => _context.IsValid;

	public McdfManager Mcdf { get; }

	private CharacterModule? Module { get; set; }

	public event DisableDrawHandler? OnDisableDraw;

	public CharacterManager(IEditorContext context, IObjectTable objectTable, HookScope scope, IFramework framework, McdfManager mcdf)
	{
		_context = context;
		_objectTable = objectTable;
		_scope = scope;
		_framework = framework;
		Mcdf = mcdf;
	}

	public void Initialize()
	{
		Ktisis.Log.Verbose("Initializing character manager...");
		try
		{
			Module = _scope.Create<CharacterModule>(new object[1] { this });
			Subscribe();
			Module.Initialize();
			Module.EnableAll();
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to initialize character module:\n{value}");
		}
	}

	private unsafe void Subscribe()
	{
		Module.OnDisableDraw += HandleDisableDraw;
		Module.OnEnableDraw += HandleEnableDraw;
	}

	private unsafe void HandleDisableDraw(IGameObject gameObj, DrawObject* drawObj)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		this.OnDisableDraw?.Invoke(gameObj, drawObj);
		if (drawObj != null)
		{
			_savedTransforms[gameObj.ObjectIndex] = (Transform)((DrawObject)drawObj).Position;
		}
	}

	private unsafe void HandleEnableDraw(GameObject* gameObj)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		ushort objectIndex = ((GameObject)gameObj).ObjectIndex;
		if (_savedTransforms.TryGetValue(objectIndex, out var value))
		{
			if (((GameObject)gameObj).DrawObject != null)
			{
				Unsafe.Write(&((DrawObject)((GameObject)gameObj).DrawObject).Position, value);
			}
			_savedTransforms.Remove(objectIndex);
		}
		else if (((GameObject)gameObj).DrawObject != null)
		{
			SetDrawObjectPosition(gameObj);
		}
	}

	private unsafe static void SetDrawObjectPosition(GameObject* gameObj)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if ((int)((GameObject)gameObj).ObjectKind == 9 && ((EffectContainer)(&((Character)gameObj).Effects)).CurrentFloatHeight > 0f)
		{
			Unsafe.Write(&((DrawObject)((GameObject)gameObj).DrawObject).Position, ((GameObject)gameObj).Position);
			((Vector3)(&((GameObject)gameObj).Position)).Y += ((EffectContainer)(&((Character)gameObj).Effects)).CurrentFloatHeight;
		}
		if (((Vector3)(&((GameObject)gameObj).DrawOffset)).Equals(Vector3.op_Implicit(Vector3.Zero)))
		{
			Unsafe.Write(&((DrawObject)((GameObject)gameObj).DrawObject).Position, ((GameObject)gameObj).Position);
			return;
		}
		Vector3 drawOffset = ((GameObject)gameObj).DrawOffset;
		Vector3 position = ((GameObject)gameObj).Position;
		float num = MathF.Cos(((GameObject)gameObj).DefaultRotation);
		float num2 = MathF.Sin(((GameObject)gameObj).DefaultRotation);
		((Vector3)(&((DrawObject)((GameObject)gameObj).DrawObject).Position)).X = drawOffset.X * num + drawOffset.Z * num2 + position.X;
		((Vector3)(&((DrawObject)((GameObject)gameObj).DrawObject).Position)).Z = drawOffset.Z * num - drawOffset.X * num2 + position.Z;
		((Vector3)(&((DrawObject)((GameObject)gameObj).DrawObject).Position)).Y = drawOffset.Y + position.Y;
	}

	private unsafe Transform* GetLocalPlayerPosition()
	{
		IPlayerCharacter localPlayer = _objectTable.LocalPlayer;
		nint? num = ((localPlayer != null) ? new nint?(((IGameObject)localPlayer).Address) : ((nint?)null));
		GameObject* ptr = (GameObject*)(num.HasValue ? ((void*)num.GetValueOrDefault()) : null);
		if (ptr != null && ((GameObject)ptr).DrawObject != null)
		{
			return (Transform*)(&((DrawObject)((GameObject)ptr).DrawObject).Position);
		}
		return null;
	}

	public ICustomizeEditor GetCustomizeEditor(ActorEntity actor)
	{
		return new CustomizeEditor(actor);
	}

	public IEquipmentEditor GetEquipmentEditor(ActorEntity actor)
	{
		return new EquipmentEditor(actor);
	}

	private EntityCharaConverter BuildEntityConverter(ActorEntity actor)
	{
		ICustomizeEditor customizeEditor = GetCustomizeEditor(actor);
		IEquipmentEditor equipmentEditor = GetEquipmentEditor(actor);
		return new EntityCharaConverter(actor, customizeEditor, equipmentEditor);
	}

	public bool TryGetStateForActor(IGameObject actor, out ActorEntity entity, out AppearanceState state)
	{
		ActorEntity actorEntity = (entity = _context.Scene.GetEntityForActor(actor));
		state = actorEntity?.Appearance;
		return actorEntity != null;
	}

	public void ApplyStateToGameObject(ActorEntity entity)
	{
		GetCustomizeEditor(entity).ApplyStateToGameObject();
		GetEquipmentEditor(entity).ApplyStateToGameObject();
	}

	public Task ApplyCharaFile(ActorEntity actor, CharaFile file, SaveModes modes = SaveModes.All, bool gameState = false)
	{
		EntityCharaConverter loader = BuildEntityConverter(actor);
		return _framework.RunOnFrameworkThread((Action)delegate
		{
			loader.Apply(file, modes);
			if (gameState)
			{
				ApplyStateToGameObject(actor);
			}
		});
	}

	public Task<CharaFile> SaveCharaFile(ActorEntity actor)
	{
		return _framework.RunOnFrameworkThread<CharaFile>((Func<CharaFile>)(() => BuildEntityConverter(actor).Save()));
	}

	public Task ApplyNpc(ActorEntity actor, INpcBase npc, SaveModes modes = SaveModes.All, bool gameState = false)
	{
		EntityCharaConverter loader = BuildEntityConverter(actor);
		return _framework.RunOnFrameworkThread((Action)delegate
		{
			loader.Apply(npc, modes);
			if (gameState)
			{
				ApplyStateToGameObject(actor);
			}
		});
	}

	public void Dispose()
	{
		Module?.Dispose();
		Module = null;
		GC.SuppressFinalize(this);
	}
}
