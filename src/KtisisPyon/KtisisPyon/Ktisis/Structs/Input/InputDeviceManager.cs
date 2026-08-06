namespace Ktisis.Structs.Input;

public struct InputDeviceManager
{
	public unsafe void* Controller;

	public unsafe MouseDeviceData* Mouse;

	public unsafe KeyboardDeviceData* Keyboard;
}
