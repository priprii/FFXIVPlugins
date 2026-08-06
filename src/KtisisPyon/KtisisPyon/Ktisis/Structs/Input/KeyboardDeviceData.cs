using Dalamud.Game.ClientState.Keys;

namespace Ktisis.Structs.Input;

public struct KeyboardDeviceData
{
	public const int Length = 160;

	public byte IsKeyPressed;

	public unsafe fixed uint KeyMap[160];

	public KeyboardQueue Queue;

	public int KeyboardQueueCount;

	public int ControllerQueueCount;

	public unsafe bool IsKeyDown(VirtualKey key, bool consume = false)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		bool num = KeyMap[(nint)key] != 0;
		if (num && consume)
		{
			KeyMap[(nint)key] = 0u;
		}
		return num;
	}
}
