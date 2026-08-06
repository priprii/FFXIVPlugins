using System;
using System.Runtime.InteropServices;

namespace TargetPyon;

public static class Input
{
	public static bool IsGameFocused
	{
		get
		{
			nint foregroundWindow = GetForegroundWindow();
			if (foregroundWindow == IntPtr.Zero)
			{
				return false;
			}
			int processId = Environment.ProcessId;
			GetWindowThreadProcessId(foregroundWindow, out var processId2);
			return processId2 == processId;
		}
	}

	public static bool IsCtrlDown
	{
		get
		{
			if (IsGameFocused)
			{
				if ((GetKeyState(162) & 0x80) == 0)
				{
					return (GetKeyState(163) & 0x80) != 0;
				}
				return true;
			}
			return false;
		}
	}

	public static bool IsShiftDown
	{
		get
		{
			if (IsGameFocused)
			{
				if ((GetKeyState(160) & 0x80) == 0)
				{
					return (GetKeyState(161) & 0x80) != 0;
				}
				return true;
			}
			return false;
		}
	}

	public static bool IsAltDown
	{
		get
		{
			if (IsGameFocused)
			{
				if ((GetKeyState(164) & 0x80) == 0)
				{
					return (GetKeyState(165) & 0x80) != 0;
				}
				return true;
			}
			return false;
		}
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	private static extern nint GetForegroundWindow();

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int GetWindowThreadProcessId(nint handle, out int processId);

	[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern short GetKeyState(int nVirtKey);
}
