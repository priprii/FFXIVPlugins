using System;
using Dalamud.Game.ClientState.Objects.Enums;

namespace Ktisis.Editor.Characters.Make;

public class MakeTypeFeature
{
	public string Name = string.Empty;

	public CustomizeIndex Index;

	public MakeTypeParam[] Params = Array.Empty<MakeTypeParam>();

	public bool IsCustomize;

	public bool IsIcon;
}
