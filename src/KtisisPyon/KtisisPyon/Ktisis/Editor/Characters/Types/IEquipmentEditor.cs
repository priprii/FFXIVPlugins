using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Ktisis.Editor.Characters.State;

namespace Ktisis.Editor.Characters.Types;

public interface IEquipmentEditor
{
	void ApplyStateFlags();

	EquipmentModelId GetEquipIndex(EquipIndex index);

	void SetEquipIndex(EquipIndex index, EquipmentModelId model);

	void SetEquipIdVariant(EquipIndex index, ushort id, byte variant);

	void SetEquipStainId(EquipIndex index, byte stainId, int dyeIndex = 0);

	bool GetHatVisible();

	void SetHatVisible(bool visible);

	bool GetVisorToggled();

	void SetVisorToggled(bool toggled);

	ushort GetGlassesId(int index = 0);

	void SetGlassesId(int index, ushort id);

	WeaponModelId GetWeaponIndex(WeaponIndex index);

	void SetWeaponIndex(WeaponIndex index, WeaponModelId model);

	void SetWeaponIdBaseVariant(WeaponIndex index, ushort id, ushort second, byte variant);

	void SetWeaponStainId(WeaponIndex index, byte stainId, int dyeIndex = 0);

	bool GetWeaponVisible(WeaponIndex index);

	void SetWeaponVisible(WeaponIndex index, bool visible);

	void ApplyStateToGameObject();
}
