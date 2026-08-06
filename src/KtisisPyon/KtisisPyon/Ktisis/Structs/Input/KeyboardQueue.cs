namespace Ktisis.Structs.Input;

public struct KeyboardQueue
{
	public unsafe fixed ulong _data[66];

	public unsafe QueueEntry this[int i]
	{
		get
		{
			fixed (ulong* data = _data)
			{
				return ((QueueEntry*)data)[i];
			}
		}
	}
}
