using System;

namespace PyonPix.Shared.Structs.Light;

[Flags]
public enum LightFlags : uint
{
	Reflections = 1u,
	DynamicShadows = 2u,
	CharacterShadows = 4u,
	ObjectShadows = 8u
}
