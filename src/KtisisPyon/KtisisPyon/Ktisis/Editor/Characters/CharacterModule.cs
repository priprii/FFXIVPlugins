using System;
using System.Runtime.CompilerServices;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;
using Ktisis.Editor.Characters.State;
using Ktisis.Editor.Characters.Types;
using Ktisis.Interop.Hooking;
using Ktisis.Scene.Entities.Game;
using Ktisis.Services.Data;
using Ktisis.Services.Game;
using Ktisis.Structs.Actors;
using Ktisis.Structs.Characters;

namespace Ktisis.Editor.Characters;

public class CharacterModule : HookModule
{
	private unsafe delegate nint DisableDrawDelegate(GameObject* chara);

	private unsafe delegate nint EnableDrawDelegate(GameObject* gameObject);

	private unsafe delegate CharacterBase* CreateCharacterDelegate(uint model, CustomizeContainer* customize, EquipmentContainer* equip, byte unk);

	private unsafe delegate CharacterSetupContainer* CopyFromCharacterDelegate(CharacterSetupContainer* self, Character* source, CopyFlags flags);

	private readonly ICharacterManager Manager;

	private readonly ActorService _actors;

	private readonly CustomizeService _discovery;

	private unsafe GameObject* _prepareCharaFor;

	[Signature("40 53 48 83 EC ?? 80 B9 ?? ?? ?? ?? 00 48 8B D9 7D ?? 48 81 C1", DetourName = "DisableDrawDetour")]
	private Hook<DisableDrawDelegate> DisableDrawHook;

	[Signature("E8 ?? ?? ?? ?? 48 8B 8B ?? ?? ?? ?? 48 85 C9 74 33 45 33 C0", DetourName = "EnableDrawDetour")]
	private Hook<EnableDrawDelegate> EnableDrawHook;

	[Signature("E8 ?? ?? ?? ?? 48 8B 4F 08 48 8B D0 4C 8B 01", DetourName = "CreateCharacterDetour")]
	private Hook<CreateCharacterDelegate> CreateCharacterHook;

	[Signature("48 89 5C 24 ?? 55 56 41 54 41 56 41 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 0F B6 82", DetourName = "CharacterSetupDetour")]
	private Hook<CopyFromCharacterDelegate> CopyFromCharacterHook;

	private bool IsValid => Manager.IsValid;

	public event DisableDrawHandler? OnDisableDraw;

	public event EnableDrawHandler? OnEnableDraw;

	public CharacterModule(IHookMediator hook, ICharacterManager manager, ActorService actors, CustomizeService discovery)
		: base(hook)
	{
		Manager = manager;
		_actors = actors;
		_discovery = discovery;
	}

	private unsafe nint DisableDrawDetour(GameObject* chara)
	{
		try
		{
			if (((GameObject)chara).DrawObject != null)
			{
				HandleDisableDraw(chara);
			}
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to handle disable draw:\n{value}");
		}
		return DisableDrawHook.Original(chara);
	}

	private unsafe void HandleDisableDraw(GameObject* chara)
	{
		IGameObject address = _actors.GetAddress((nint)chara);
		if (address != null)
		{
			this.OnDisableDraw?.Invoke(address, ((GameObject)chara).DrawObject);
		}
	}

	private unsafe nint EnableDrawDetour(GameObject* gameObject)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!IsValid)
		{
			return EnableDrawHook.Original(gameObject);
		}
		bool num = (((GameObject)gameObject).TargetableStatus & 0x80) > 0;
		bool flag = ((uint)((GameObject)gameObject).RenderFlags & 0x2000000) == 0;
		if (num && flag)
		{
			return EnableDrawHook.Original(gameObject);
		}
		nint result = IntPtr.Zero;
		try
		{
			_prepareCharaFor = gameObject;
			result = EnableDrawHook.Original(gameObject);
			this.OnEnableDraw?.Invoke(gameObject);
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to handle character update:\n{value}");
		}
		finally
		{
			_prepareCharaFor = null;
		}
		return result;
	}

	private unsafe CharacterBase* CreateCharacterDetour(uint model, CustomizeContainer* customize, EquipmentContainer* equip, byte unk)
	{
		try
		{
			if (customize != null && equip != null)
			{
				PreHandleCreate(ref model, customize, equip);
			}
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failure on PreHandleCreate:\n{value}");
		}
		return CreateCharacterHook.Original(model, customize, equip, unk);
	}

	private unsafe void PreHandleCreate(ref uint model, CustomizeContainer* customize, EquipmentContainer* equip)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		if (!IsValid || _prepareCharaFor == null)
		{
			return;
		}
		IGameObject address = _actors.GetAddress((nint)_prepareCharaFor);
		if (address == null || !Manager.TryGetStateForActor(address, out ActorEntity entity, out AppearanceState state))
		{
			return;
		}
		if (state.ModelId.HasValue)
		{
			model = state.ModelId.Value;
		}
		for (int i = 0; i < 26; i++)
		{
			CustomizeIndex index = (CustomizeIndex)i;
			if (state.Customize.IsSet(index))
			{
				customize->Bytes[i] = state.Customize[index];
			}
		}
		CharacterEx* prepareCharaFor = (CharacterEx*)_prepareCharaFor;
		if (prepareCharaFor->Mode == 3 && prepareCharaFor->EmoteMode == EmoteModeEnum.Normal)
		{
			prepareCharaFor->Mode = 1;
		}
		if (state.Customize.IsSet((CustomizeIndex)4) || state.Customize.IsSet((CustomizeIndex)5))
		{
			ushort num = _discovery.CalcDataIdFor(customize->Tribe, customize->Gender);
			bool flag = _discovery.IsFaceIdValidFor(num, customize->FaceType);
			Ktisis.Log.Debug($"Face {customize->FaceType} for {num} is valid? {flag}");
			if (!flag)
			{
				byte faceType = customize->FaceType;
				faceType = ((customize->Tribe != Tribe.Highlander || faceType >= 101) ? _discovery.FindBestFaceTypeFor(num, customize->FaceType) : ((byte)(faceType + 100)));
				Ktisis.Log.Debug($"\tSetting {faceType} as next best face type");
				state.Customize.SetIfActive((CustomizeIndex)5, faceType);
				customize->FaceType = faceType;
			}
		}
		for (uint num2 = 0u; num2 < 10; num2++)
		{
			EquipIndex equipIndex = (EquipIndex)num2;
			if (equipIndex == EquipIndex.Head && state.HatVisible == EquipmentToggle.Off)
			{
				Unsafe.Write(equip->GetData(num2), default(EquipmentModelId));
			}
			else if (state.Equipment.IsSet(equipIndex))
			{
				Unsafe.Write(equip->GetData(num2), state.Equipment[equipIndex]);
			}
		}
		Manager.GetEquipmentEditor(entity).ApplyStateFlags();
	}

	private unsafe CharacterSetupContainer* CharacterSetupDetour(CharacterSetupContainer* self, Character* source, CopyFlags flags)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		CharacterSetupContainer* result = CopyFromCharacterHook.Original(self, source, flags);
		Character* ownerObject = ((CharacterSetupContainer)self).OwnerObject;
		((EffectContainer)(&((Character)ownerObject).Effects)).CurrentFloatHeight = ((EffectContainer)(&((Character)source).Effects)).CurrentFloatHeight;
		((Vector3)(&((Character)ownerObject).DrawOffset)).Y += ((EffectContainer)(&((Character)source).Effects)).CurrentFloatHeight;
		return result;
	}
}
