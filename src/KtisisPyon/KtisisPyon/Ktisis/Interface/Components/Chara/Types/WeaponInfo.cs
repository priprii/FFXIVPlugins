using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Ktisis.Editor.Characters.State;
using Ktisis.Editor.Characters.Types;
using Ktisis.GameData.Excel;

namespace Ktisis.Interface.Components.Chara.Types;

public class WeaponInfo(IEquipmentEditor editor) : ItemInfo
{
	public required WeaponIndex Index;

	public required WeaponModelId Model;

	public override EquipSlot Slot => Index.ToEquipSlot();

	public override ushort ModelId => Model.Id;

	public override byte[] StainIds => new byte[2] { Model.Stain0, Model.Stain1 };

	public override bool IsHideable => true;

	public override bool IsVisible => editor.GetWeaponVisible(Index);

	public void SetModel(ushort id, ushort second, byte variant)
	{
		editor.SetWeaponIdBaseVariant(Index, id, second, variant);
	}

	public override void SetEquipItem(ItemSheet item)
	{
		bool num = Index == WeaponIndex.MainHand;
		ItemModel itemModel = (((num && item.Model.Id != 0) || item.SubModel.Id == 0) ? item.Model : item.SubModel);
		SetModel(itemModel.Id, itemModel.Base, (byte)itemModel.Variant);
		if (num && item.SubModel.Id != 0)
		{
			editor.SetWeaponIdBaseVariant(WeaponIndex.OffHand, item.SubModel.Id, item.SubModel.Base, (byte)item.SubModel.Variant);
		}
	}

	public override void SetStainId(byte id, int index = 0)
	{
		editor.SetWeaponStainId(Index, id, index);
	}

	public override void Unequip()
	{
		SetModel(0, 0, 0);
	}

	public override void SetVisible(bool visible)
	{
		editor.SetWeaponVisible(Index, visible);
	}

	public override bool IsCurrent()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return ((object)editor.GetWeaponIndex(Index)/*cast due to constrained. prefix*/).Equals((object?)Model);
	}

	public override bool IsItemPredicate(ItemSheet item)
	{
		if (!item.Model.Matches(Model.Id, Model.Type, Model.Variant))
		{
			if (item.SubModel.Id != 0)
			{
				return item.SubModel.Matches(Model.Id, Model.Type, Model.Variant);
			}
			return false;
		}
		return true;
	}
}
