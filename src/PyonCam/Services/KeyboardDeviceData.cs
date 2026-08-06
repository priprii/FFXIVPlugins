using System;
using Dalamud.Game.ClientState.Keys;

namespace PyonCam.Services;

public struct KeyboardDeviceData
{
	public const int Length = 160;

	public byte IsKeyPressed;

	public unsafe fixed uint KeyMap[160];

	public KeyboardQueue Queue;

	public int KeyboardQueueCount;

	public int ControllerQueueCount;

	public bool IsKeyDownValid()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected I4, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		foreach (VirtualKey value in Enum.GetValues(typeof(VirtualKey)))
		{
			ushort num = (ushort)(int)value;
			if (num <= 31)
			{
				if (num <= 15)
				{
					if (num < 8 || num >= 13)
					{
						goto IL_0067;
					}
				}
				else if (num <= 26)
				{
					if (num >= 21)
					{
						goto IL_0067;
					}
				}
				else if (num >= 28)
				{
					goto IL_0067;
				}
			}
			else if (num <= 183)
			{
				if (num >= 160)
				{
					goto IL_0067;
				}
			}
			else if (num > 228)
			{
				goto IL_0067;
			}
			bool flag = false;
			goto IL_006d;
			IL_0067:
			flag = true;
			goto IL_006d;
			IL_006d:
			if (!flag && IsKeyDown(value))
			{
				return true;
			}
		}
		return false;
	}

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

	public unsafe bool IsAnyKeyDown(bool consume = false)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		VirtualKey[] obj = Enum.GetValues(typeof(VirtualKey)) as VirtualKey[];
		bool result = false;
		VirtualKey[] array = obj;
		foreach (VirtualKey val in array)
		{
			bool num = KeyMap[(nint)val] != 0;
			if (num)
			{
				result = true;
			}
			if (num && consume)
			{
				KeyMap[(nint)val] = 0u;
			}
		}
		if (consume)
		{
			KeyMap[0] = 1u;
		}
		return result;
	}
}
