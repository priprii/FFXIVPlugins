using System.Numerics;

namespace PyonPix.Structs.Light;

public struct Light
{
	public nint Address;

	public Vector3? ScreenAverage;

	public Vector3[] History;

	public long[] HistoryTicks;

	public int HistoryCount;

	public int HistoryIndex;

	public long LastTimestamp;
}
