using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Ktisis.Structs.Characters;

namespace Ktisis.Editor.Characters.State;

public class EquipmentState
{
	private EquipmentContainer _container;

	private readonly bool[] _state = new bool[10];

	public EquipmentModelId this[EquipIndex index]
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

	public bool IsSet(EquipIndex index)
	{
		return _state[(uint)index];
	}

	public void Unset(EquipIndex index)
	{
		_state[(uint)index] = false;
	}
}
