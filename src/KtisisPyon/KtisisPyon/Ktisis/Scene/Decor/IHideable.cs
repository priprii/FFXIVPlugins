namespace Ktisis.Scene.Decor;

public interface IHideable
{
	bool IsHidden { get; set; }

	void ToggleHidden();
}
