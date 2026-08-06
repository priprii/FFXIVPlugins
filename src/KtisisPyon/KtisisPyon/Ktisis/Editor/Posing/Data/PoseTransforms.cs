using System;

namespace Ktisis.Editor.Posing.Data;

[Flags]
public enum PoseTransforms
{
	None = 0,
	Rotation = 1,
	Position = 2,
	Scale = 4,
	PositionRoot = 8
}
