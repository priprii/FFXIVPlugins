using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Ktisis.Structs.Characters;

namespace Ktisis.Editor.Characters.State;

public class WeaponState
{
	private WeaponContainer _container;

	private readonly bool[] _state = new bool[3];

	private readonly EquipmentToggle[] _visible = new EquipmentToggle[3];

	public WeaponModelId this[WeaponIndex index]
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return _container[(uint)index];
		}
		set
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			_container[(uint)index] = value;
			_state[(uint)index] = true;
		}
	}

	public bool IsSet(WeaponIndex index)
	{
		return _state[(uint)index];
	}

	public void Unset(WeaponIndex index)
	{
		_state[(uint)index] = false;
	}

	public EquipmentToggle GetVisible(WeaponIndex index)
	{
		return _visible[(uint)index];
	}

	public void SetVisible(WeaponIndex index, bool visible)
	{
		_visible[(uint)index] = ((!visible) ? EquipmentToggle.Off : EquipmentToggle.On);
	}

	public bool CheckVisible(WeaponIndex index, bool visible)
	{
		EquipmentToggle equipmentToggle = _visible[(uint)index];
		if (equipmentToggle == EquipmentToggle.None)
		{
			return visible;
		}
		return equipmentToggle == EquipmentToggle.On;
	}
}
