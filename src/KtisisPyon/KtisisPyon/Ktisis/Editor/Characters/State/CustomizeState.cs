using Dalamud.Game.ClientState.Objects.Enums;
using Ktisis.Structs.Characters;

namespace Ktisis.Editor.Characters.State;

public class CustomizeState
{
	private CustomizeContainer _container;

	private readonly bool[] _state = new bool[26];

	public byte this[CustomizeIndex index]
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected I4, but got Unknown
			return _container[(uint)(int)index];
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Expected I4, but got Unknown
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			_container[(uint)(int)index] = value;
			_state[index] = true;
		}
	}

	public void SetIfActive(CustomizeIndex index, byte value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (IsSet(index))
		{
			this[index] = value;
		}
	}

	public bool IsSet(CustomizeIndex index)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _state[index];
	}

	public void Unset(CustomizeIndex index)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_state[index] = false;
	}
}
