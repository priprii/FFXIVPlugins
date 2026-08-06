using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Ktisis.Editor.Characters.State;
using Ktisis.Editor.Characters.Types;
using Ktisis.GameData.Excel;

namespace Ktisis.Interface.Components.Chara.Types;

public class EquipInfo(IEquipmentEditor editor) : ItemInfo
{
	public required EquipIndex Index;

	public required EquipmentModelId Model;

	public override EquipSlot Slot => Index.ToEquipSlot();

	public override ushort ModelId => Model.Id;

	public override byte[] StainIds => new byte[2] { Model.Stain0, Model.Stain1 };

	public override bool IsHideable => Slot == EquipSlot.Head;

	public override bool IsVisible
	{
		get
		{
			if (Slot == EquipSlot.Head)
			{
				return editor.GetHatVisible();
			}
			return false;
		}
	}

	public override bool IsVisor => Slot == EquipSlot.Head;

	public override bool IsVisorToggled
	{
		get
		{
			if (Slot == EquipSlot.Head)
			{
				return editor.GetVisorToggled();
			}
			return false;
		}
	}

	public void SetModel(ushort id, byte variant)
	{
		editor.SetEquipIdVariant(Index, id, variant);
	}

	public override void SetEquipItem(ItemSheet item)
	{
		SetModel(item.Model.Id, (byte)item.Model.Variant);
	}

	public override void SetStainId(byte id, int index = 0)
	{
		editor.SetEquipStainId(Index, id, index);
	}

	public override void Unequip()
	{
		SetModel(0, 0);
	}

	public override void SetVisible(bool visible)
	{
		if (Slot == EquipSlot.Head)
		{
			editor.SetHatVisible(visible);
		}
	}

	public override void SetVisorToggled(bool toggled)
	{
		if (Slot == EquipSlot.Head)
		{
			editor.SetVisorToggled(toggled);
		}
	}

	public override bool IsCurrent()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return ((object)editor.GetEquipIndex(Index)/*cast due to constrained. prefix*/).Equals((object?)Model);
	}

	public override bool IsItemPredicate(ItemSheet item)
	{
		return item.Model.Matches(Model.Id, Model.Variant);
	}
}
