using Ktisis.Editor.Posing.Types;
using Ktisis.Structs.Attachment;

namespace Ktisis.Scene.Decor;

public interface IAttachable : ICharacter
{
	bool IsAttached();

	unsafe Attach* GetAttach();

	PartialBoneInfo? GetParentBone();

	void Detach();
}
