using System;
using System.Runtime.InteropServices;

namespace PyonPix.Interop;

public sealed class WindowSubclass : IDisposable
{
	public delegate long WndProcDelegate(nint hWnd, uint msg, ulong wParam, long lParam);

	private readonly nint Hwnd;

	private readonly WndProcDelegate Callback;

	private readonly nint CallbackPtr;

	private nint CacheWndProc;

	private bool IsDisposed;

	[DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
	private static extern long CallWindowProc(nint lpPrevWndFunc, nint hWnd, uint msg, ulong wParam, long lParam);

	[DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
	private static extern nint GetWindowLongPtr(nint hWnd, Win32Interop.WindowLongFlags nIndex);

	[DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
	private static extern nint SetWindowLongPtr(nint hWnd, Win32Interop.WindowLongFlags nIndex, nint dwNewLong);

	public WindowSubclass(nint hWnd, WndProcDelegate callback)
	{
		Hwnd = hWnd;
		Callback = callback;
		CallbackPtr = Marshal.GetFunctionPointerForDelegate(Callback);
		CacheWndProc = SetWindowLongPtr(Hwnd, Win32Interop.WindowLongFlags.GWL_WNDPROC, CallbackPtr);
	}

	public long CallOriginal(nint hWnd, uint msg, ulong wParam, long lParam)
	{
		return CallWindowProc(CacheWndProc, hWnd, msg, wParam, lParam);
	}

	public void Dispose()
	{
		if (!IsDisposed)
		{
			IsDisposed = true;
			if (GetWindowLongPtr(Hwnd, Win32Interop.WindowLongFlags.GWL_WNDPROC) == CallbackPtr && CacheWndProc != IntPtr.Zero)
			{
				SetWindowLongPtr(Hwnd, Win32Interop.WindowLongFlags.GWL_WNDPROC, CacheWndProc);
			}
			CacheWndProc = IntPtr.Zero;
			GC.SuppressFinalize(this);
		}
	}

	~WindowSubclass()
	{
		Dispose();
	}
}
