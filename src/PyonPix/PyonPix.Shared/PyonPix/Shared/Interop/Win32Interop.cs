using System;
using System.Runtime.InteropServices;

namespace PyonPix.Shared.Interop;

public static class Win32Interop
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct MSG
	{
		public nint hwnd;

		public uint message;

		public nuint wParam;

		public nuint lParam;

		public uint time;

		public POINT pt;
	}

	public struct POINT
	{
		public int x;

		public int Y;
	}

	[DllImport("user32.dll")]
	public static extern sbyte GetMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

	[DllImport("user32.dll")]
	public static extern bool TranslateMessage(ref MSG lpMsg);

	[DllImport("user32.dll")]
	public static extern nint DispatchMessage(ref MSG lpMsg);

	public static void MessageLoop()
	{
		MSG lpMsg;
		while (GetMessage(out lpMsg, IntPtr.Zero, 0u, 0u) > 0)
		{
			TranslateMessage(ref lpMsg);
			DispatchMessage(ref lpMsg);
		}
	}
}
