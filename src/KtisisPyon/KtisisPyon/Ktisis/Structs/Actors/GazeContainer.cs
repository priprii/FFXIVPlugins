using System.Runtime.InteropServices;

namespace Ktisis.Structs.Actors;

[StructLayout(LayoutKind.Explicit, Size = 480)]
public struct GazeContainer
{
	[FieldOffset(48)]
	public Gaze Gaze;
}
