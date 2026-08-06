namespace Ktisis.Actions.Types;

public interface IMemento
{
	void Restore();

	void Apply();
}
