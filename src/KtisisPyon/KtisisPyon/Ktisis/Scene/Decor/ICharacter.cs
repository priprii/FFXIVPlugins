using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;

namespace Ktisis.Scene.Decor;

public interface ICharacter
{
	bool IsValid { get; }

	unsafe CharacterBase* GetCharacter();
}
