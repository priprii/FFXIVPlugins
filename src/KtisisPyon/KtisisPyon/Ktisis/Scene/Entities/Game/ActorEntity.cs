using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Ktisis.Common.Extensions;
using Ktisis.Data.Config.Sections;
using Ktisis.Editor.Characters.State;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Entities.Character;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Scene.Factory.Builders;
using Ktisis.Scene.Modules.Actors;
using Ktisis.Scene.Types;
using Ktisis.Structs.Actors;
using Ktisis.Structs.Characters;

namespace Ktisis.Scene.Entities.Game;

public class ActorEntity : CharaEntity, IDeletable, IHideable
{
	public readonly IGameObject Actor;

	private bool DefaultsInitialized;

	public string? MCDF;

	public ActorGaze? Gaze;

	private readonly Dictionary<string, PresetState> _presetStates = new Dictionary<string, PresetState>();

	public Guid? AssignedProfile;

	private string Anonymized;

	private string RealName;

	public bool IsManaged { get; set; }

	public unsafe bool IsHidden
	{
		get
		{
			CharacterEx* character = (CharacterEx*)Character;
			if (character != null)
			{
				return character->Opacity == 0f;
			}
			return false;
		}
		set
		{
			CharacterEx* character = (CharacterEx*)Character;
			if (character != null)
			{
				if (character->Opacity != 0f)
				{
					character->Opacity = 0f;
				}
				else
				{
					character->Opacity = 1f;
				}
			}
		}
	}

	public override bool IsValid
	{
		get
		{
			if (base.IsValid)
			{
				return Actor.IsValid();
			}
			return false;
		}
	}

	public override string Name
	{
		get
		{
			if (!Scene.Context.Config.Editor.IncognitoPlayerNames)
			{
				return RealName;
			}
			return Anonymized;
		}
		set
		{
			RealName = value;
		}
	}

	public AppearanceState Appearance { get; } = new AppearanceState();

	public unsafe GameObject* CsGameObject => (GameObject*)Actor.Address;

	public unsafe Character* Character
	{
		get
		{
			if (CsGameObject == null || !((GameObject)CsGameObject).IsCharacter())
			{
				return null;
			}
			return (Character*)CsGameObject;
		}
	}

	public ActorEntity(ISceneManager scene, IPoseBuilder pose, IGameObject actor)
		: base(scene, pose)
	{
		base.Type = EntityType.Actor;
		Actor = actor;
		Anonymized = actor.GetNameOrFallback(Scene.Context, true);
		PresetConfig.PresetRemovedEvent = (PresetConfig.PresetRemoved)Delegate.Combine(PresetConfig.PresetRemovedEvent, new PresetConfig.PresetRemoved(RemovePreset));
	}

	private void RemovePreset(string presetName)
	{
		if (_presetStates.ContainsKey(presetName))
		{
			TogglePreset(presetName, false);
		}
	}

	public override void Update()
	{
		if (IsObjectValid)
		{
			UpdateChara();
			base.Update();
			if (!DefaultsInitialized)
			{
				SetDefaultPresets();
			}
		}
	}

	private unsafe void UpdateChara()
	{
		CharacterBaseEx* characterBaseEx = base.CharacterBaseEx;
		nint num = (nint)characterBaseEx;
		if (base.Address != num)
		{
			base.Address = num;
		}
		if (characterBaseEx != null)
		{
			WetnessState? wetness = Appearance.Wetness;
			if (wetness.HasValue)
			{
				WetnessState valueOrDefault = wetness.GetValueOrDefault();
				characterBaseEx->Wetness = valueOrDefault;
			}
		}
	}

	private unsafe CustomizeData* GetCustomize()
	{
		Human* human = GetHuman();
		if (human != null)
		{
			return &((Human)human).Customize;
		}
		Character* character = Character;
		if (character != null)
		{
			return &((DrawDataContainer)(&((Character)character).DrawData)).CustomizeData;
		}
		return null;
	}

	public unsafe byte GetCustomizeValue(CustomizeIndex index)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (Appearance.Customize.IsSet(index))
		{
			return Appearance.Customize[index];
		}
		Human* human = GetHuman();
		if (human == null)
		{
			return 0;
		}
		return ((CustomizeData)(&((Human)human).Customize))[(int)(byte)index];
	}

	public unsafe string? GetRaceSexId()
	{
		Human* human = GetHuman();
		if (human == null)
		{
			return null;
		}
		return Convert.ToString((int)((Human)human).RaceSexId);
	}

	public bool IsViera()
	{
		return GetCustomizeValue((CustomizeIndex)0) == 8;
	}

	public bool TryGetEarId(out byte id)
	{
		if (!IsViera())
		{
			id = 0;
			return false;
		}
		id = GetCustomizeValue((CustomizeIndex)22);
		return true;
	}

	public bool TryGetEarIdAsChar(out char id)
	{
		byte id2;
		bool result = TryGetEarId(out id2);
		id = (char)(96 + id2);
		return result;
	}

	public unsafe override Object* GetObject()
	{
		if (CsGameObject == null)
		{
			return null;
		}
		return (Object*)((GameObject)CsGameObject).DrawObject;
	}

	public unsafe override CharacterBase* GetCharacter()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Invalid comparison between Unknown and I4
		if (!IsObjectValid)
		{
			return null;
		}
		DrawObject* ptr = ((CsGameObject != null) ? ((GameObject)CsGameObject).DrawObject : null);
		if (ptr == null || (int)((Object)(&((DrawObject)ptr).Object)).GetObjectType() != 3)
		{
			return null;
		}
		return (CharacterBase*)ptr;
	}

	public unsafe Human* GetHuman()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		CharacterBase* character = GetCharacter();
		if (character != null && (int)((CharacterBase)character).GetModelType() == 1)
		{
			return (Human*)character;
		}
		return null;
	}

	public void Redraw()
	{
		Actor.Redraw();
	}

	public void ToggleHidden()
	{
		IsHidden = !IsHidden;
	}

	public bool Delete()
	{
		Scene.GetModule<ActorModule>().Delete(this);
		PresetConfig.PresetRemovedEvent = (PresetConfig.PresetRemoved)Delegate.Remove(PresetConfig.PresetRemovedEvent, new PresetConfig.PresetRemoved(RemovePreset));
		return false;
	}

	public IEnumerable<(string name, PresetState isEnabled)> GetPresets()
	{
		yield return default((string, PresetState));
		SortedDictionary<string, System.Collections.Immutable.ImmutableHashSet<string>>.KeyCollection keys = Scene.Context.Config.Presets.Presets.Keys;
		foreach (string item in keys)
		{
			yield return (name: item, isEnabled: _presetStates.GetValueOrDefault(item, PresetState.Disabled));
		}
	}

	public bool TogglePreset(string presetName, bool? state = null)
	{
		return false;
	}

	public void ToggleOtherPreset(bool? state = null)
	{
	}

	public void ClearVisibility()
	{
		foreach (BoneNode item in Recurse().OfType<BoneNode>())
		{
			item.Visible = false;
		}
		_presetStates.Clear();
	}

	private void SetDefaultPresets()
	{
	}

	internal void EnsurePresetVisibility()
	{
	}

	public bool SavePreset(string presetName)
	{
		return false;
	}

	private void CheckImplicitlyEnabled()
	{
	}

	public unsafe void SetActorGazeTarget(ActorEntity? otherActor)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (otherActor == null || otherActor.CsGameObject == null)
		{
			return;
		}
		GameObjectId gameObjectId = ((GameObject)otherActor.CsGameObject).GetGameObjectId();
		if (Actor.IsPcCharacter())
		{
			((Character)Character).SetTargetId(gameObjectId);
			((Character)Character).SetSoftTargetId(gameObjectId);
		}
		else
		{
			if (GetActorGazeTarget() == 0)
			{
				return;
			}
			CharacterEx* character = (CharacterEx*)Character;
			if (character == null)
			{
				return;
			}
			if (!Gaze.HasValue)
			{
				Gaze = character->Gaze;
			}
			ActorGaze value = Gaze.Value;
			for (int i = 0; i < 3; i++)
			{
				GazeControl type = (GazeControl)i;
				Gaze value2 = value[type];
				if (value2.TargetId.Type > 0 && value2.TargetId.ObjectId != 0)
				{
					value2.TargetId = gameObjectId;
					value[type] = value2;
				}
			}
			Gaze = value;
		}
	}

	public unsafe uint GetActorGazeTarget()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		CharacterEx* ptr = (CharacterEx*)(IsValid ? Character : null);
		if (ptr == null)
		{
			return 0u;
		}
		for (int i = 0; i < 3; i++)
		{
			GazeControl type = (GazeControl)i;
			Gaze gaze = ptr->Gaze[type];
			if (gaze.Mode == GazeMode.Object && gaze.TargetId.Type > 0 && gaze.TargetId.ObjectId >= 201 && gaze.TargetId.ObjectId <= 448)
			{
				return gaze.TargetId.ObjectId;
			}
		}
		return 0u;
	}
}
