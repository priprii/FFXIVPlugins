using System.Numerics;

namespace Ktisis.Structs.Input;

public struct MouseDeviceData
{
	public int PosX;

	public int PosY;

	public int ScrollDelta;

	public MouseButton Pressed;

	public MouseButton Clicked;

	public ulong Unk1;

	public int DeltaX;

	public int DeltaY;

	public uint Unk2;

	public bool IsFocused;

	public bool IsButtonHeld(MouseButton button)
	{
		return (Pressed & button) != 0;
	}

	public Vector2 GetDelta(bool consume = false)
	{
		Vector2 result = new Vector2(DeltaX, DeltaY);
		if (consume)
		{
			DeltaX = 0;
			DeltaY = 0;
		}
		return result;
	}
}
