using System.Runtime.InteropServices;

namespace Ktisis.Structs.Characters;

[StructLayout(LayoutKind.Sequential, Size = 12)]
public struct WetnessState
{
	public float WeatherWetness;

	public float SwimmingWetness;

	public float WetnessDepth;
}
