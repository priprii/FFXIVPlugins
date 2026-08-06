using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Ktisis.Structs.Characters;

namespace Ktisis.GameData.Excel.Types;

public interface INpcBase
{
	string Name { get; set; }

	ushort GetModelId()
	{
		return 0;
	}

	CustomizeContainer? GetCustomize()
	{
		return null;
	}

	EquipmentContainer? GetEquipment()
	{
		return null;
	}

	WeaponModelId? GetMainHand()
	{
		return null;
	}

	WeaponModelId? GetOffHand()
	{
		return null;
	}
}
