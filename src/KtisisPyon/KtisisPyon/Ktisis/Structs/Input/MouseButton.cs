using System;

namespace Ktisis.Structs.Input;

[Flags]
public enum MouseButton
{
	None = 0,
	Left = 1,
	Middle = 2,
	Right = 4,
	Mouse4 = 8,
	Mouse5 = 0x10
}
