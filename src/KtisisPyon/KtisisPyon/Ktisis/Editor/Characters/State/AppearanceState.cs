using Ktisis.Structs.Characters;

namespace Ktisis.Editor.Characters.State;

public class AppearanceState
{
	public readonly CustomizeState Customize = new CustomizeState();

	public readonly EquipmentState Equipment = new EquipmentState();

	public readonly WeaponState Weapons = new WeaponState();

	public uint? ModelId { get; set; }

	public EquipmentToggle HatVisible { get; set; }

	public EquipmentToggle VisorToggled { get; set; }

	public WetnessState? Wetness { get; set; }

	public bool CheckHatVisible(bool visible)
	{
		if (HatVisible == EquipmentToggle.None)
		{
			return visible;
		}
		return HatVisible == EquipmentToggle.On;
	}

	public bool CheckVisorToggled(bool toggled)
	{
		if (VisorToggled == EquipmentToggle.None)
		{
			return toggled;
		}
		return VisorToggled == EquipmentToggle.On;
	}
}
