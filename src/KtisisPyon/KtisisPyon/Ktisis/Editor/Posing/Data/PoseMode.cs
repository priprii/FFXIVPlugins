using System;

namespace Ktisis.Editor.Posing.Data;

[Flags]
public enum PoseMode
{
	None = 0,
	Body = 1,
	Face = 2,
	BodyFace = 3,
	Weapons = 4,
	All = 7
}
