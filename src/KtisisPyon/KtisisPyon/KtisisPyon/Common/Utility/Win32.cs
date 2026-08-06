using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using Ktisis.Data.Config.Sections;

namespace KtisisPyon.Common.Utility;

public static class Win32
{
	public struct RECT(int left, int top, int right, int bottom)
	{
		public int Left = left;

		public int Top = top;

		public int Right = right;

		public int Bottom = bottom;

		public int X
		{
			get
			{
				return Left;
			}
			set
			{
				Right -= Left - value;
				Left = value;
			}
		}

		public int Y
		{
			get
			{
				return Top;
			}
			set
			{
				Bottom -= Top - value;
				Top = value;
			}
		}

		public int Height
		{
			get
			{
				return Bottom - Top;
			}
			set
			{
				Bottom = value + Top;
			}
		}

		public int Width
		{
			get
			{
				return Right - Left;
			}
			set
			{
				Right = value + Left;
			}
		}

		public Point Location
		{
			get
			{
				return new Point(Left, Top);
			}
			set
			{
				X = value.X;
				Y = value.Y;
			}
		}

		public Size Size
		{
			get
			{
				return new Size(Width, Height);
			}
			set
			{
				Width = value.Width;
				Height = value.Height;
			}
		}

		public RECT(Rectangle r)
			: this(r.Left, r.Top, r.Right, r.Bottom)
		{
		}

		public static implicit operator Rectangle(RECT r)
		{
			return new Rectangle(r.Left, r.Top, r.Width, r.Height);
		}

		public static implicit operator RECT(Rectangle r)
		{
			return new RECT(r);
		}

		public static bool operator ==(RECT r1, RECT r2)
		{
			return r1.Equals(r2);
		}

		public static bool operator !=(RECT r1, RECT r2)
		{
			return !r1.Equals(r2);
		}

		public bool Equals(RECT r)
		{
			if (r.Left == Left && r.Top == Top && r.Right == Right)
			{
				return r.Bottom == Bottom;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is RECT)
			{
				return Equals((RECT)obj);
			}
			if (obj is Rectangle)
			{
				return Equals(new RECT((Rectangle)obj));
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((Rectangle)this/*cast due to constrained. prefix*/).GetHashCode();
		}

		public override string ToString()
		{
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			InlineArray4<object> buffer = default(InlineArray4<object>);
			buffer[0] = Left;
			buffer[1] = Top;
			buffer[2] = Right;
			buffer[3] = Bottom;
			return string.Format((IFormatProvider?)currentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", (ReadOnlySpan<object?>)buffer);
		}
	}

	public struct POINT(int x, int y)
	{
		public int X = x;

		public int Y = y;

		public static implicit operator Point(POINT p)
		{
			return new Point(p.X, p.Y);
		}

		public static implicit operator POINT(Point p)
		{
			return new POINT(p.X, p.Y);
		}

		public override string ToString()
		{
			return $"X: {X}, Y: {Y}";
		}
	}

	[Flags]
	public enum WindowStyles : uint
	{
		WS_BORDER = 0x800000u,
		WS_CAPTION = 0xC00000u,
		WS_CHILD = 0x40000000u,
		WS_CLIPCHILDREN = 0x2000000u,
		WS_CLIPSIBLINGS = 0x4000000u,
		WS_DISABLED = 0x8000000u,
		WS_DLGFRAME = 0x400000u,
		WS_GROUP = 0x20000u,
		WS_HSCROLL = 0x100000u,
		WS_MAXIMIZE = 0x1000000u,
		WS_MAXIMIZEBOX = 0x10000u,
		WS_MINIMIZE = 0x20000000u,
		WS_MINIMIZEBOX = 0x20000u,
		WS_OVERLAPPED = 0u,
		WS_OVERLAPPEDWINDOW = 0xCF0000u,
		WS_POPUP = 0x80000000u,
		WS_POPUPWINDOW = 0x80880000u,
		WS_SIZEFRAME = 0x40000u,
		WS_SYSMENU = 0x80000u,
		WS_TABSTOP = 0x10000u,
		WS_VISIBLE = 0x10000000u,
		WS_VSCROLL = 0x200000u
	}

	[Flags]
	public enum SWP : uint
	{
		NOSIZE = 1u,
		NOMOVE = 2u,
		NOZORDER = 4u,
		NOREDRAW = 8u,
		NOACTIVATE = 0x10u,
		DRAWFRAME = 0x20u,
		FRAMECHANGED = 0x20u,
		SHOWWINDOW = 0x40u,
		HIDEWINDOW = 0x80u,
		NOCOPYBITS = 0x100u,
		NOOWNERZORDER = 0x200u,
		NOREPOSITION = 0x200u,
		NOSENDCHANGING = 0x400u,
		DEFERERASE = 0x2000u,
		ASYNCWINDOWPOS = 0x4000u
	}

	public enum GWL
	{
		WNDPROC = -4,
		HINSTANCE = -6,
		HWNDPARENT = -8,
		STYLE = -16,
		EXSTYLE = -20,
		USERDATA = -21,
		ID = -12
	}

	private const int SW_MAXIMIZE = 3;

	private const int SW_RESTORE = 9;

	private static nint GameWindowHandle = IntPtr.Zero;

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool GetWindowRect(nint hwnd, out RECT lpRect);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int width, int height, uint uFlags);

	[DllImport("user32.DLL")]
	private static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

	[DllImport("user32.DLL")]
	private static extern int GetWindowLong(nint hWnd, int nIndex);

	[DllImport("user32.dll")]
	private static extern bool ShowWindow(nint hWnd, int nCmdShow);

	public unsafe static (Point pos, Size size, int style, Size deviceSize) GetWinProperties()
	{
		if (GameWindowHandle == IntPtr.Zero)
		{
			GameWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
		}
		GetWindowRect(GameWindowHandle, out var lpRect);
		int windowLong = GetWindowLong(GameWindowHandle, -16);
		Device* ptr = Device.Instance();
		return (pos: new Point(lpRect.X, lpRect.Y), size: new Size(lpRect.Width, lpRect.Height), style: windowLong, deviceSize: new Size((int)((Device)ptr).Width, (int)((Device)ptr).Height));
	}

	public unsafe static void SetWinDefault(PyonConfig cfg)
	{
		if (cfg.DefaultSize == Size.Empty)
		{
			return;
		}
		var (point, size, num, size2) = GetWinProperties();
		if (cfg.DefaultDeviceSize != size2)
		{
			Device* intPtr = Device.Instance();
			((Device)intPtr).NewWidth = (uint)cfg.DefaultDeviceSize.Width;
			((Device)intPtr).NewHeight = (uint)cfg.DefaultDeviceSize.Height;
			((Device)intPtr).RequestResolutionChange = 1;
		}
		if (cfg.DefaultStyle != num || cfg.DefaultPosition != point || cfg.DefaultSize != size)
		{
			if (((WindowStyles)cfg.DefaultStyle).HasFlag(WindowStyles.WS_MAXIMIZE))
			{
				SetWindowLong(GameWindowHandle, -16, cfg.DefaultStyle);
				ShowWindow(GameWindowHandle, 3);
				SetWindowPos(GameWindowHandle, IntPtr.Zero, cfg.DefaultPosition.X, cfg.DefaultPosition.Y, cfg.DefaultSize.Width + cfg.DefaultPosition.X, cfg.DefaultSize.Height + cfg.DefaultPosition.Y, 36u);
				SetWindowLong(GameWindowHandle, -16, cfg.DefaultStyle);
				ShowWindow(GameWindowHandle, 3);
			}
			else
			{
				SetWindowLong(GameWindowHandle, -16, cfg.DefaultStyle);
				ShowWindow(GameWindowHandle, 9);
				SetWindowPos(GameWindowHandle, IntPtr.Zero, cfg.DefaultPosition.X, cfg.DefaultPosition.Y, cfg.DefaultSize.Width + cfg.DefaultPosition.X, cfg.DefaultSize.Height + cfg.DefaultPosition.Y, 36u);
				SetWindowLong(GameWindowHandle, -16, cfg.DefaultStyle);
				ShowWindow(GameWindowHandle, 9);
			}
		}
	}

	public unsafe static void SetWinRes(PyonConfig cfg)
	{
		if (!(cfg.HiResSize == Size.Empty))
		{
			GetWinProperties();
			Device* ptr = Device.Instance();
			if (((Device)ptr).Width != (uint)cfg.HiResSize.Width || ((Device)ptr).Height != (uint)cfg.HiResSize.Height)
			{
				SetWindowLong(GameWindowHandle, -16, cfg.DefaultStyle & 0x1000000);
				ShowWindow(GameWindowHandle, 3);
				((Device)ptr).NewWidth = (uint)cfg.HiResSize.Width;
				((Device)ptr).NewHeight = (uint)cfg.HiResSize.Height;
				((Device)ptr).RequestResolutionChange = 1;
				SetWindowLong(GameWindowHandle, -16, cfg.DefaultStyle & -29360129);
				ShowWindow(GameWindowHandle, 9);
			}
			else
			{
				SetWinDefault(cfg);
			}
		}
	}
}
