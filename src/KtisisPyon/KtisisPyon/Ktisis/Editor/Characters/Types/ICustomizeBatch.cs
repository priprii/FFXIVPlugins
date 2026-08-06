using Dalamud.Game.ClientState.Objects.Enums;

namespace Ktisis.Editor.Characters.Types;

public interface ICustomizeBatch
{
	ICustomizeBatch SetCustomization(CustomizeIndex index, byte value);

	ICustomizeBatch SetIfNotNull(CustomizeIndex index, byte? value);

	ICustomizeBatch SetModelId(uint id);

	void Apply();
}
