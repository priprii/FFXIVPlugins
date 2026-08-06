namespace Ktisis.Scene.Decor;

public interface IAttachTarget
{
	bool TryAcceptAttach(IAttachable child);
}
