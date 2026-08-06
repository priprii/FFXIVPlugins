using System.Numerics;

namespace PyonCam.Services;

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

	public bool IsButtonHeld(MouseButton button, bool consume = false)
	{
		if ((Clicked & button) != MouseButton.None || (Pressed & button) != MouseButton.None)
		{
			if (consume)
			{
				if ((Clicked & button) != MouseButton.None)
				{
					Clicked = MouseButton.None;
				}
				if ((Pressed & button) != MouseButton.None)
				{
					Pressed = MouseButton.None;
				}
			}
			return true;
		}
		return false;
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
