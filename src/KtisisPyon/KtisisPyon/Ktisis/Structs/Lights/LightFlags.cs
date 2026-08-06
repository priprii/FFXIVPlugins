using System;

namespace Ktisis.Structs.Lights;

[Flags]
public enum LightFlags : uint
{
	Reflection = 1u,
	Dynamic = 2u,
	CharaShadow = 4u,
	ObjectShadow = 8u
}
