using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Ktisis.Data.Json.Converters;
using Ktisis.Structs.Characters;

namespace Ktisis.Data.Files;

public class CharaFile : JsonFile
{
	[Serializable]
	public class WeaponSave
	{
		public Vector3 Color { get; set; }

		public Vector3 Scale { get; set; }

		public ushort ModelSet { get; set; }

		public ushort ModelBase { get; set; }

		public ushort ModelVariant { get; set; }

		public ushort DyeId { get; set; }

		public ushort DyeId2 { get; set; }

		public WeaponSave()
		{
		}

		public WeaponSave(WeaponModelId from)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			ModelSet = from.Id;
			ModelBase = from.Type;
			ModelVariant = from.Variant;
			DyeId = from.Stain0;
			DyeId2 = from.Stain1;
		}
	}

	[Serializable]
	public class ItemSave
	{
		public ushort ModelBase { get; set; }

		public byte ModelVariant { get; set; }

		public byte DyeId { get; set; }

		public byte DyeId2 { get; set; }

		public ItemSave()
		{
		}

		public ItemSave(EquipmentModelId from)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			ModelBase = from.Id;
			ModelVariant = from.Variant;
			DyeId = from.Stain0;
			DyeId2 = from.Stain1;
		}
	}

	[Serializable]
	public class GlassesSave
	{
		public ushort GlassesId { get; set; }

		public GlassesSave()
		{
		}

		public GlassesSave(ushort id)
		{
			GlassesId = id;
		}
	}

	public enum AnamRace : byte
	{
		Hyur = 1,
		Elezen,
		Lalafel,
		Miqote,
		Roegadyn,
		AuRa,
		Hrothgar,
		Viera
	}

	public enum AnamTribe : byte
	{
		Midlander = 1,
		Highlander,
		Wildwood,
		Duskwight,
		Plainsfolk,
		Dunesfolk,
		SeekerOfTheSun,
		KeeperOfTheMoon,
		SeaWolf,
		Hellsguard,
		Raen,
		Xaela,
		Helions,
		TheLost,
		Rava,
		Veena
	}

	[Flags]
	public enum AnamFacialFeature : byte
	{
		None = 0,
		First = 1,
		Second = 2,
		Third = 4,
		Fourth = 8,
		Fifth = 0x10,
		Sixth = 0x20,
		Seventh = 0x40,
		LegacyTattoo = 0x80
	}

	public const int CurrentVersion = 1;

	public new string FileExtension { get; set; } = ".chara";

	public new string TypeName { get; set; } = "Ktisis Character File";

	[DeserializerDefault(1)]
	public new int FileVersion { get; set; } = 1;

	public string? Nickname { get; set; }

	public uint ModelType { get; set; }

	public ObjectKind ObjectKind { get; set; }

	public AnamRace? Race { get; set; }

	public Gender? Gender { get; set; }

	public Age? Age { get; set; }

	public byte? Height { get; set; }

	public AnamTribe? Tribe { get; set; }

	public byte? Head { get; set; }

	public byte? Hair { get; set; }

	public bool? EnableHighlights { get; set; }

	public byte? Skintone { get; set; }

	public byte? REyeColor { get; set; }

	public byte? HairTone { get; set; }

	public byte? Highlights { get; set; }

	public AnamFacialFeature? FacialFeatures { get; set; }

	public byte? LimbalEyes { get; set; }

	public byte? Eyebrows { get; set; }

	public byte? LEyeColor { get; set; }

	public byte? Eyes { get; set; }

	public byte? Nose { get; set; }

	public byte? Jaw { get; set; }

	public byte? Mouth { get; set; }

	public byte? LipsToneFurPattern { get; set; }

	public byte? EarMuscleTailSize { get; set; }

	public byte? TailEarsType { get; set; }

	public byte? Bust { get; set; }

	public byte? FacePaint { get; set; }

	public byte? FacePaintColor { get; set; }

	public WeaponSave? MainHand { get; set; }

	public WeaponSave? OffHand { get; set; }

	public ItemSave? HeadGear { get; set; }

	public ItemSave? Body { get; set; }

	public ItemSave? Hands { get; set; }

	public ItemSave? Legs { get; set; }

	public ItemSave? Feet { get; set; }

	public ItemSave? Ears { get; set; }

	public ItemSave? Neck { get; set; }

	public ItemSave? Wrists { get; set; }

	public ItemSave? LeftRing { get; set; }

	public ItemSave? RightRing { get; set; }

	public GlassesSave? Glasses { get; set; }

	public Vector3? BustScale { get; set; }

	public float? Transparency { get; set; }

	public float? HeightMultiplier { get; set; }
}
