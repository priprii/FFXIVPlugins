namespace Ktisis.Scene.Decor.Ik;

public interface IIkNode
{
	bool IsEnabled { get; }

	void Enable();

	void Disable();
}
