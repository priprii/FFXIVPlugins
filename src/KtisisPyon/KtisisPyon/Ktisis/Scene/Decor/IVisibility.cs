namespace Ktisis.Scene.Decor;

public interface IVisibility
{
	bool Visible { get; set; }

	bool Toggle()
	{
		return Visible = !Visible;
	}
}
