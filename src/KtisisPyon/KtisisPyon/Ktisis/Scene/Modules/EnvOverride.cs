using System;

namespace Ktisis.Scene.Modules;

[Flags]
public enum EnvOverride
{
	None = 0,
	TimeWeather = 1,
	SkyId = 2,
	Lighting = 4,
	Stars = 8,
	Fog = 0x10,
	Clouds = 0x20,
	Rain = 0x40,
	Dust = 0x80,
	Wind = 0x100,
	Housing = 0x200
}
