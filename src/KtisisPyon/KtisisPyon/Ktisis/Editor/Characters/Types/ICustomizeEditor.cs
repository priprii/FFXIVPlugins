using Dalamud.Game.ClientState.Objects.Enums;

namespace Ktisis.Editor.Characters.Types;

public interface ICustomizeEditor
{
	void SetCustomization(CustomizeIndex index, byte value);

	byte GetCustomization(CustomizeIndex index);

	void SetHeterochromia(bool enabled);

	bool GetHeterochromia();

	void SetEyeColor(byte value);

	uint GetModelId();

	void SetModelId(uint id, bool redraw = true);

	void ApplyStateToGameObject();

	ICustomizeBatch Prepare();
}
