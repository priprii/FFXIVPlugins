namespace PyonCam.Services;

public struct InputDeviceManager
{
	public unsafe void* Controller;

	public unsafe MouseDeviceData* Mouse;

	public unsafe KeyboardDeviceData* Keyboard;
}
