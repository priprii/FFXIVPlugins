namespace Ktisis.Structs.Actors;

public enum GazeMode : uint
{
	Disabled = 0u,
	Object = 1u,
	Rotate = 2u,
	Target = 3u,
	_KtisisFollowCam_ = 9u,
	_KtisisFollowGizmo_ = 10u
}
