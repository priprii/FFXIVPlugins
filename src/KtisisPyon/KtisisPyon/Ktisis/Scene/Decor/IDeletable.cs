namespace Ktisis.Scene.Decor;

public interface IDeletable
{
	bool CanDelete => true;

	bool Delete();
}
