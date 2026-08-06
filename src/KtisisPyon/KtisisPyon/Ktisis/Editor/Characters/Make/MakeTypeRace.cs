using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Enums;
using Ktisis.GameData.Chara;
using Ktisis.Structs.Characters;

namespace Ktisis.Editor.Characters.Make;

public class MakeTypeRace(Tribe tribe, Gender gender)
{
	public Tribe Tribe = tribe;

	public Gender Gender = gender;

	public readonly Dictionary<CustomizeIndex, MakeTypeFeature> Customize = new Dictionary<CustomizeIndex, MakeTypeFeature>();

	public readonly Dictionary<byte, uint[]> FaceFeatureIcons = new Dictionary<byte, uint[]>();

	public TribeColors Colors;

	public bool HasFeature(CustomizeIndex index)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Customize.ContainsKey(index);
	}

	public MakeTypeFeature? GetFeature(CustomizeIndex index)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Customize.GetValueOrDefault(index);
	}
}
