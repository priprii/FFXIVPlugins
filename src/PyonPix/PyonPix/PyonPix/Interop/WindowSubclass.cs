using System;
using System.Runtime.InteropServices;

namespace PyonPix.Interop;

public static class WindowSubclass
{
	public delegate nint WndProcDelegate(nint hwnd, uint msg, nuint wParam, nint lParam);

	public static nint Hwnd;

	private static WndProcDelegate? Callback;

	private static nint CallbackPtr;

	private static nint CacheWndProc;

	public static void Initialize(nint hwnd, WndProcDelegate callback)
	{
		Hwnd = hwnd;
		Callback = callback;
		CallbackPtr = Marshal.GetFunctionPointerForDelegate(Callback);
		CacheWndProc = Win32Interop.SetWindowLongPtr(hwnd, Win32Interop.WindowLongFlags.GWL_WNDPROC, CallbackPtr);
	}

	public static nint CallOriginal(nint hWnd, uint msg, nuint wParam, nint lParam)
	{
		return Win32Interop.CallWindowProc(CacheWndProc, hWnd, msg, wParam, lParam);
	}

	public static void Dispose()
	{
		if (Win32Interop.GetWindowLongPtr(Hwnd, Win32Interop.WindowLongFlags.GWL_WNDPROC) == CallbackPtr && CacheWndProc != IntPtr.Zero)
		{
			Win32Interop.SetWindowLongPtr(Hwnd, Win32Interop.WindowLongFlags.GWL_WNDPROC, CacheWndProc);
			CacheWndProc = IntPtr.Zero;
		}
	}
}
