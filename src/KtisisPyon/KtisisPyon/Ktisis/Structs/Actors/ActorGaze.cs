namespace Ktisis.Structs.Actors;

public struct ActorGaze
{
	public GazeContainer Torso;

	public GazeContainer Head;

	public GazeContainer Eyes;

	public GazeContainer Other;

	public Gaze this[GazeControl type]
	{
		get
		{
			return type switch
			{
				GazeControl.Torso => Torso.Gaze, 
				GazeControl.Head => Head.Gaze, 
				GazeControl.Eyes => Eyes.Gaze, 
				_ => Other.Gaze, 
			};
		}
		set
		{
			switch (type)
			{
			case GazeControl.Torso:
				Torso.Gaze = value;
				break;
			case GazeControl.Head:
				Head.Gaze = value;
				break;
			case GazeControl.Eyes:
				Eyes.Gaze = value;
				break;
			case GazeControl.All:
				Torso.Gaze = value;
				Head.Gaze = value;
				Eyes.Gaze = value;
				Other.Gaze = value;
				break;
			}
		}
	}
}
