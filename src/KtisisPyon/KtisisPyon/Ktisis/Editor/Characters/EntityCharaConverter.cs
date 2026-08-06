using System;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Ktisis.Data.Files;
using Ktisis.Editor.Characters.State;
using Ktisis.Editor.Characters.Types;
using Ktisis.GameData.Excel.Types;
using Ktisis.Scene.Entities.Game;
using Ktisis.Structs.Actors;
using Ktisis.Structs.Characters;

namespace Ktisis.Editor.Characters;

public class EntityCharaConverter
{
	private readonly ActorEntity _entity;

	private readonly ICustomizeEditor _custom;

	private readonly IEquipmentEditor _equip;

	public EntityCharaConverter(ActorEntity entity, ICustomizeEditor custom, IEquipmentEditor equip)
	{
		_entity = entity;
		_custom = custom;
		_equip = equip;
	}

	public void Apply(CharaFile file, SaveModes modes = SaveModes.All)
	{
		ApplyEquipment(file, modes);
		PrepareCustomize(file, modes).Apply();
		ApplyMisc(file);
	}

	public CharaFile Save()
	{
		CharaFile charaFile = new CharaFile
		{
			Nickname = _entity.Name
		};
		WriteCustomize(charaFile);
		WriteEquipment(charaFile);
		WriteMisc(charaFile);
		return charaFile;
	}

	public void Apply(INpcBase npc, SaveModes modes = SaveModes.All)
	{
		ApplyEquipment(npc, modes);
		PrepareCustomize(npc, modes).Apply();
	}

	private ICustomizeBatch PrepareCustomize(INpcBase npc, SaveModes modes = SaveModes.All)
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Invalid comparison between Unknown and I4
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Invalid comparison between Unknown and I4
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Invalid comparison between Unknown and I4
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Invalid comparison between Unknown and I4
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Invalid comparison between Unknown and I4
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected I4, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Invalid comparison between Unknown and I4
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Expected I4, but got Unknown
		ICustomizeBatch customizeBatch = _custom.Prepare();
		bool flag = modes.HasFlag(SaveModes.AppearanceFace);
		bool flag2 = modes.HasFlag(SaveModes.AppearanceBody);
		bool flag3 = modes.HasFlag(SaveModes.AppearanceHair);
		if (!flag && !flag2 && !flag3)
		{
			return customizeBatch;
		}
		if (flag2)
		{
			ushort modelId = npc.GetModelId();
			if (modelId != ushort.MaxValue)
			{
				customizeBatch.SetModelId(modelId);
			}
		}
		CustomizeContainer? customize = npc.GetCustomize();
		if (!customize.HasValue)
		{
			return customizeBatch;
		}
		bool flag4 = true;
		for (uint num = 0u; num < 26; num++)
		{
			flag4 &= customize.Value[num] == 0;
			if (!flag4)
			{
				break;
			}
		}
		if (flag4)
		{
			return customizeBatch;
		}
		CustomizeIndex[] values = Enum.GetValues<CustomizeIndex>();
		int num2 = 0;
		while (num2 < values.Length)
		{
			CustomizeIndex val = values[num2];
			if ((int)val >= 12)
			{
				if ((int)val > 20)
				{
					if ((int)val <= 23)
					{
						goto IL_0124;
					}
					if (val - 24 > 1)
					{
						goto IL_012b;
					}
				}
			}
			else
			{
				if ((int)val < 0)
				{
					goto IL_012b;
				}
				if ((int)val <= 4)
				{
					goto IL_0124;
				}
				switch (val - 5)
				{
				case 0:
					break;
				case 1:
				case 2:
				case 5:
				case 6:
					goto IL_011f;
				default:
					goto IL_012b;
				}
			}
			bool flag5 = flag;
			goto IL_012e;
			IL_011f:
			flag5 = flag3;
			goto IL_012e;
			IL_012e:
			if (flag5)
			{
				customizeBatch.SetCustomization(val, customize.Value[(uint)(int)val]);
			}
			num2++;
			continue;
			IL_012b:
			flag5 = flag2;
			goto IL_012e;
			IL_0124:
			flag5 = flag || flag2;
			goto IL_012e;
		}
		return customizeBatch;
	}

	private ICustomizeBatch PrepareCustomize(CharaFile file, SaveModes modes = SaveModes.All)
	{
		ICustomizeBatch customizeBatch = _custom.Prepare();
		bool num = modes.HasFlag(SaveModes.AppearanceFace);
		bool flag = modes.HasFlag(SaveModes.AppearanceBody);
		if (modes.HasFlag(SaveModes.AppearanceHair))
		{
			bool? enableHighlights = file.EnableHighlights;
			byte? value = ((!enableHighlights.HasValue) ? ((byte?)null) : new byte?((byte)((enableHighlights == true) ? 128u : 0u)));
			customizeBatch.SetIfNotNull((CustomizeIndex)6, file.Hair).SetIfNotNull((CustomizeIndex)10, file.HairTone).SetIfNotNull((CustomizeIndex)11, file.Highlights)
				.SetIfNotNull((CustomizeIndex)7, value);
		}
		if (num || flag)
		{
			customizeBatch.SetIfNotNull((CustomizeIndex)0, (byte?)file.Race).SetIfNotNull((CustomizeIndex)4, (byte?)file.Tribe).SetIfNotNull((CustomizeIndex)1, (byte?)file.Gender)
				.SetIfNotNull((CustomizeIndex)2, (byte?)file.Age);
		}
		if (num)
		{
			customizeBatch.SetIfNotNull((CustomizeIndex)5, file.Head).SetIfNotNull((CustomizeIndex)16, file.Eyes).SetIfNotNull((CustomizeIndex)9, file.REyeColor)
				.SetIfNotNull((CustomizeIndex)15, file.LEyeColor)
				.SetIfNotNull((CustomizeIndex)14, file.Eyebrows)
				.SetIfNotNull((CustomizeIndex)17, file.Nose)
				.SetIfNotNull((CustomizeIndex)18, file.Jaw)
				.SetIfNotNull((CustomizeIndex)19, file.Mouth)
				.SetIfNotNull((CustomizeIndex)20, file.LipsToneFurPattern)
				.SetIfNotNull((CustomizeIndex)13, file.LimbalEyes)
				.SetIfNotNull((CustomizeIndex)12, (byte?)file.FacialFeatures)
				.SetIfNotNull((CustomizeIndex)24, file.FacePaint)
				.SetIfNotNull((CustomizeIndex)25, file.FacePaintColor);
		}
		if (flag)
		{
			customizeBatch.SetIfNotNull((CustomizeIndex)3, file.Height).SetIfNotNull((CustomizeIndex)8, file.Skintone).SetIfNotNull((CustomizeIndex)21, file.EarMuscleTailSize)
				.SetIfNotNull((CustomizeIndex)22, file.TailEarsType)
				.SetIfNotNull((CustomizeIndex)23, file.Bust)
				.SetModelId(file.ModelType);
		}
		return customizeBatch;
	}

	private void ApplyEquipment(INpcBase npc, SaveModes modes = SaveModes.All)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		if (modes.HasFlag(SaveModes.EquipmentWeapons))
		{
			WeaponModelId? mainHand = npc.GetMainHand();
			if (mainHand.HasValue)
			{
				WeaponModelId valueOrDefault = mainHand.GetValueOrDefault();
				_equip.SetWeaponIndex(WeaponIndex.MainHand, valueOrDefault);
			}
			mainHand = npc.GetOffHand();
			if (mainHand.HasValue)
			{
				WeaponModelId valueOrDefault2 = mainHand.GetValueOrDefault();
				_equip.SetWeaponIndex(WeaponIndex.OffHand, valueOrDefault2);
			}
		}
		bool flag = modes.HasFlag(SaveModes.EquipmentGear);
		bool flag2 = modes.HasFlag(SaveModes.EquipmentAccessories);
		if (!flag && !flag2)
		{
			return;
		}
		EquipmentContainer? equipment = npc.GetEquipment();
		if (!equipment.HasValue)
		{
			return;
		}
		bool flag3 = true;
		for (uint num = 0u; num < 10; num++)
		{
			flag3 &= equipment.Value[num].Value == 0;
			if (!flag3)
			{
				break;
			}
		}
		if (flag3)
		{
			return;
		}
		EquipIndex[] values = Enum.GetValues<EquipIndex>();
		foreach (EquipIndex equipIndex in values)
		{
			if (equipIndex > EquipIndex.Feet || flag)
			{
				if (equipIndex >= EquipIndex.Earring && !flag2)
				{
					break;
				}
				_equip.SetEquipIndex(equipIndex, equipment.Value[(uint)equipIndex]);
			}
		}
	}

	private void ApplyEquipment(CharaFile file, SaveModes modes = SaveModes.All)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (modes.HasFlag(SaveModes.EquipmentWeapons))
		{
			SetWeaponIndex(file, WeaponIndex.MainHand).SetWeaponIndex(file, WeaponIndex.OffHand);
		}
		bool flag = modes.HasFlag(SaveModes.EquipmentGear);
		bool flag2 = modes.HasFlag(SaveModes.EquipmentAccessories);
		if (!flag && !flag2)
		{
			return;
		}
		EquipIndex[] values = Enum.GetValues<EquipIndex>();
		foreach (EquipIndex equipIndex in values)
		{
			if (equipIndex > EquipIndex.Feet || flag)
			{
				if (equipIndex >= EquipIndex.Earring && !flag2)
				{
					break;
				}
				EquipmentModelId? equipModelId = GetEquipModelId(file, equipIndex);
				if (equipModelId.HasValue)
				{
					EquipmentModelId valueOrDefault = equipModelId.GetValueOrDefault();
					_equip.SetEquipIndex(equipIndex, valueOrDefault);
				}
			}
		}
		if (file.Glasses == null)
		{
			CharaFile.GlassesSave glassesSave = (file.Glasses = new CharaFile.GlassesSave());
		}
		_equip.SetGlassesId(0, file.Glasses.GlassesId);
	}

	private EntityCharaConverter SetWeaponIndex(CharaFile file, WeaponIndex index)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		CharaFile.WeaponSave weaponSave = index switch
		{
			WeaponIndex.MainHand => file.MainHand, 
			WeaponIndex.OffHand => file.OffHand, 
			_ => null, 
		};
		if (weaponSave == null)
		{
			return this;
		}
		_equip.SetWeaponIndex(index, new WeaponModelId
		{
			Id = weaponSave.ModelSet,
			Type = weaponSave.ModelBase,
			Variant = weaponSave.ModelVariant,
			Stain0 = (byte)weaponSave.DyeId,
			Stain1 = (byte)weaponSave.DyeId2
		});
		return this;
	}

	private static EquipmentModelId? GetEquipModelId(CharaFile file, EquipIndex index)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		CharaFile.ItemSave itemSave = index switch
		{
			EquipIndex.Head => file.HeadGear, 
			EquipIndex.Chest => file.Body, 
			EquipIndex.Hands => file.Hands, 
			EquipIndex.Legs => file.Legs, 
			EquipIndex.Feet => file.Feet, 
			EquipIndex.Earring => file.Ears, 
			EquipIndex.Necklace => file.Neck, 
			EquipIndex.Bracelet => file.Wrists, 
			EquipIndex.RingLeft => file.LeftRing, 
			EquipIndex.RingRight => file.RightRing, 
			_ => null, 
		};
		if (itemSave == null)
		{
			return null;
		}
		return new EquipmentModelId
		{
			Id = itemSave.ModelBase,
			Variant = itemSave.ModelVariant,
			Stain0 = itemSave.DyeId,
			Stain1 = itemSave.DyeId2
		};
	}

	private unsafe void ApplyMisc(CharaFile file)
	{
		Character* character = _entity.Character;
		if (character != null)
		{
			float? transparency = file.Transparency;
			if (transparency.HasValue)
			{
				float valueOrDefault = transparency.GetValueOrDefault();
				((CharacterEx*)character)->Opacity = valueOrDefault;
			}
		}
	}

	private void WriteCustomize(CharaFile file)
	{
		file.ModelType = _custom.GetModelId();
		file.Hair = _custom.GetCustomization((CustomizeIndex)6);
		file.HairTone = _custom.GetCustomization((CustomizeIndex)10);
		file.Highlights = _custom.GetCustomization((CustomizeIndex)11);
		file.EnableHighlights = (_custom.GetCustomization((CustomizeIndex)7) & 0x80) != 0;
		file.Race = (CharaFile.AnamRace)_custom.GetCustomization((CustomizeIndex)0);
		file.Tribe = (CharaFile.AnamTribe)_custom.GetCustomization((CustomizeIndex)4);
		file.Gender = (Gender)_custom.GetCustomization((CustomizeIndex)1);
		file.Age = (Age)_custom.GetCustomization((CustomizeIndex)2);
		file.Head = _custom.GetCustomization((CustomizeIndex)5);
		file.Eyes = _custom.GetCustomization((CustomizeIndex)16);
		file.REyeColor = _custom.GetCustomization((CustomizeIndex)9);
		file.LEyeColor = _custom.GetCustomization((CustomizeIndex)15);
		file.Eyebrows = _custom.GetCustomization((CustomizeIndex)14);
		file.Nose = _custom.GetCustomization((CustomizeIndex)17);
		file.Jaw = _custom.GetCustomization((CustomizeIndex)18);
		file.Mouth = _custom.GetCustomization((CustomizeIndex)19);
		file.LipsToneFurPattern = _custom.GetCustomization((CustomizeIndex)20);
		file.LimbalEyes = _custom.GetCustomization((CustomizeIndex)13);
		file.FacialFeatures = (CharaFile.AnamFacialFeature)_custom.GetCustomization((CustomizeIndex)12);
		file.FacePaint = _custom.GetCustomization((CustomizeIndex)24);
		file.FacePaintColor = _custom.GetCustomization((CustomizeIndex)25);
		file.Height = _custom.GetCustomization((CustomizeIndex)3);
		file.Skintone = _custom.GetCustomization((CustomizeIndex)8);
		file.EarMuscleTailSize = _custom.GetCustomization((CustomizeIndex)21);
		file.TailEarsType = _custom.GetCustomization((CustomizeIndex)22);
		file.Bust = _custom.GetCustomization((CustomizeIndex)23);
	}

	private void WriteEquipment(CharaFile file)
	{
		file.MainHand = SaveWeapon(WeaponIndex.MainHand);
		file.OffHand = SaveWeapon(WeaponIndex.OffHand);
		file.HeadGear = SaveItem(EquipIndex.Head);
		file.Body = SaveItem(EquipIndex.Chest);
		file.Hands = SaveItem(EquipIndex.Hands);
		file.Legs = SaveItem(EquipIndex.Legs);
		file.Feet = SaveItem(EquipIndex.Feet);
		file.Ears = SaveItem(EquipIndex.Earring);
		file.Neck = SaveItem(EquipIndex.Necklace);
		file.Wrists = SaveItem(EquipIndex.Bracelet);
		file.LeftRing = SaveItem(EquipIndex.RingLeft);
		file.RightRing = SaveItem(EquipIndex.RingRight);
		file.Glasses = SaveGlasses();
	}

	private CharaFile.WeaponSave SaveWeapon(WeaponIndex index)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new CharaFile.WeaponSave(_equip.GetWeaponIndex(index));
	}

	private CharaFile.ItemSave SaveItem(EquipIndex index)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new CharaFile.ItemSave(_equip.GetEquipIndex(index));
	}

	private CharaFile.GlassesSave SaveGlasses()
	{
		return new CharaFile.GlassesSave(_equip.GetGlassesId());
	}

	private unsafe void WriteMisc(CharaFile file)
	{
		Character* character = _entity.Character;
		if (character != null)
		{
			file.Transparency = ((CharacterEx*)character)->Opacity;
			file.HeightMultiplier = ((GameObject)(&((Character)character).GameObject)).Height;
		}
	}
}
