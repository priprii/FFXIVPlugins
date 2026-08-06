using System;

namespace PyonPix.Structs.Browser;

[Flags]
public enum MouseButton : uint
{
	None = 0u,
	Left = 2u,
	Right = 4u,
	Middle = 8u
}
