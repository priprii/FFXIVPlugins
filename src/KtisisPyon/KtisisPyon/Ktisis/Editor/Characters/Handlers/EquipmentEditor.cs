using System;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Ktisis.Editor.Characters.State;
using Ktisis.Editor.Characters.Types;
using Ktisis.Scene.Entities.Game;

namespace Ktisis.Editor.Characters.Handlers;

public class EquipmentEditor(ActorEntity actor) : IEquipmentEditor
{
	public void ApplyStateFlags()
	{
		UpdateWeaponVisibleState(WeaponIndex.MainHand);
		UpdateWeaponVisibleState(WeaponIndex.OffHand);
		if (actor.Appearance.VisorToggled != EquipmentToggle.None)
		{
			SetVisorToggled(actor.Appearance.VisorToggled == EquipmentToggle.On);
		}
	}

	private void SetStateIfNotTracked(EquipIndex index)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (actor.IsValid && !actor.Appearance.Equipment.IsSet(index))
		{
			actor.Appearance.Equipment[index] = GetEquipIndex(index);
		}
	}

	public unsafe EquipmentModelId GetEquipIndex(EquipIndex index)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (!actor.IsValid)
		{
			return default(EquipmentModelId);
		}
		if (actor.Appearance.Equipment.IsSet(index))
		{
			return actor.Appearance.Equipment[index];
		}
		if (actor.CharacterBaseEx == null)
		{
			return default(EquipmentModelId);
		}
		return actor.CharacterBaseEx->Equipment[(uint)index];
	}

	public unsafe void SetEquipIndex(EquipIndex index, EquipmentModelId model)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (actor.IsValid)
		{
			actor.Appearance.Equipment[index] = model;
			CharacterBase* character = actor.GetCharacter();
			if (character != null)
			{
				((CharacterBase)character).SetEquipmentSlotModel((uint)index, &model);
			}
		}
	}

	public void SetEquipIdVariant(EquipIndex index, ushort id, byte variant)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		EquipmentModelId equipIndex = GetEquipIndex(index);
		equipIndex.Id = id;
		equipIndex.Variant = variant;
		SetEquipIndex(index, equipIndex);
	}

	public void SetEquipStainId(EquipIndex index, byte stainId, int dyeIndex = 0)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		EquipmentModelId equipIndex = GetEquipIndex(index);
		if (dyeIndex == 1)
		{
			equipIndex.Stain1 = stainId;
		}
		else
		{
			equipIndex.Stain0 = stainId;
		}
		SetEquipIndex(index, equipIndex);
	}

	private unsafe void ForceUpdateEquipIndex(EquipIndex index)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (actor.IsValid)
		{
			CharacterBase* character = actor.GetCharacter();
			if (character != null)
			{
				EquipmentModelId equipIndex = GetEquipIndex(index);
				((CharacterBase)character).SetEquipmentSlotModel((uint)index, &equipIndex);
			}
		}
	}

	public unsafe bool GetHatVisible()
	{
		if (actor.IsValid && actor.Character != null)
		{
			return actor.Appearance.CheckHatVisible(!((DrawDataContainer)(&((Character)actor.Character).DrawData)).IsHatHidden);
		}
		return false;
	}

	public unsafe void SetHatVisible(bool visible)
	{
		if (actor.IsValid && actor.Character != null)
		{
			SetStateIfNotTracked(EquipIndex.Head);
			actor.Appearance.HatVisible = ((!visible) ? EquipmentToggle.Off : EquipmentToggle.On);
			((DrawDataContainer)(&((Character)actor.Character).DrawData)).HideHeadgear(0u, !visible);
			if (visible)
			{
				ForceUpdateEquipIndex(EquipIndex.Head);
			}
		}
	}

	public unsafe bool GetVisorToggled()
	{
		if (actor.IsValid && actor.Character != null)
		{
			return actor.Appearance.CheckVisorToggled(((DrawDataContainer)(&((Character)actor.Character).DrawData)).IsVisorToggled);
		}
		return false;
	}

	public unsafe void SetVisorToggled(bool toggled)
	{
		if (actor.IsValid && actor.Character != null)
		{
			actor.Appearance.VisorToggled = ((!toggled) ? EquipmentToggle.Off : EquipmentToggle.On);
			((DrawDataContainer)(&((Character)actor.Character).DrawData)).SetVisor(toggled);
		}
	}

	public unsafe ushort GetGlassesId(int index)
	{
		if (actor.Character == null)
		{
			return 0;
		}
		return ((DrawDataContainer)(&((Character)actor.Character).DrawData)).GlassesIds[index];
	}

	public unsafe void SetGlassesId(int index, ushort id)
	{
		if (actor.IsValid && actor.Character != null)
		{
			((DrawDataContainer)(&((Character)actor.Character).DrawData)).SetGlasses(index, id);
		}
	}

	public unsafe WeaponModelId GetWeaponIndex(WeaponIndex index)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (!actor.IsValid)
		{
			return default(WeaponModelId);
		}
		if (actor.Appearance.Weapons.IsSet(index))
		{
			return actor.Appearance.Weapons[index];
		}
		DrawObjectData* weaponData = GetWeaponData(actor, index);
		if (weaponData == null)
		{
			return default(WeaponModelId);
		}
		return ((DrawObjectData)weaponData).ModelId;
	}

	public unsafe void SetWeaponIndex(WeaponIndex index, WeaponModelId model)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (actor.IsValid)
		{
			actor.Appearance.Weapons[index] = model;
			Character* character = actor.Character;
			if (character != null)
			{
				((DrawDataContainer)(&((Character)character).DrawData)).LoadWeapon((WeaponSlot)index, model, (byte)0, (byte)0, (byte)0, (byte)0, false);
			}
		}
	}

	public void SetWeaponIdBaseVariant(WeaponIndex index, ushort id, ushort second, byte variant)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		WeaponModelId weaponIndex = GetWeaponIndex(index);
		weaponIndex.Id = id;
		weaponIndex.Type = second;
		weaponIndex.Variant = variant;
		SetWeaponIndex(index, weaponIndex);
	}

	public void SetWeaponStainId(WeaponIndex index, byte stainId, int dyeIndex = 0)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		WeaponModelId weaponIndex = GetWeaponIndex(index);
		if (dyeIndex == 1)
		{
			weaponIndex.Stain1 = stainId;
		}
		else
		{
			weaponIndex.Stain0 = stainId;
		}
		SetWeaponIndex(index, weaponIndex);
	}

	private unsafe static DrawObjectData* GetWeaponData(ActorEntity actor, WeaponIndex index)
	{
		if (!actor.IsValid || actor.Character == null)
		{
			return null;
		}
		fixed (DrawObjectData* result = &((DrawDataContainer)(&((Character)actor.Character).DrawData)).WeaponData[(int)index])
		{
			return result;
		}
	}

	public unsafe bool GetWeaponVisible(WeaponIndex index)
	{
		DrawObjectData* weaponData = GetWeaponData(actor, index);
		if (weaponData != null)
		{
			return actor.Appearance.Weapons.CheckVisible(index, !((DrawObjectData)weaponData).IsHidden);
		}
		return false;
	}

	public unsafe void SetWeaponVisible(WeaponIndex index, bool visible)
	{
		actor.Appearance.Weapons.SetVisible(index, visible);
		DrawObjectData* weaponData = GetWeaponData(actor, index);
		if (weaponData != null)
		{
			((DrawObjectData)weaponData).IsHidden = !visible;
		}
	}

	private void UpdateWeaponVisibleState(WeaponIndex index)
	{
		EquipmentToggle visible = actor.Appearance.Weapons.GetVisible(index);
		if (visible != EquipmentToggle.None)
		{
			SetWeaponVisible(index, visible == EquipmentToggle.On);
		}
	}

	public unsafe void ApplyStateToGameObject()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (actor.IsValid && actor.Character != null)
		{
			EquipIndex[] values = Enum.GetValues<EquipIndex>();
			foreach (EquipIndex equipIndex in values)
			{
				EquipmentModelId equipIndex2 = GetEquipIndex(equipIndex);
				((DrawDataContainer)(&((Character)actor.Character).DrawData)).LoadEquipment((EquipmentSlot)equipIndex, &equipIndex2, true);
			}
		}
	}
}
