using System;

namespace Ktisis.Interface.Components.Transforms;

[Flags]
public enum TransformTableFlags
{
	None = 0,
	Position = 1,
	Rotation = 2,
	Scale = 4,
	Operation = 8,
	UseAvailable = 0x10,
	Default = 7
}
