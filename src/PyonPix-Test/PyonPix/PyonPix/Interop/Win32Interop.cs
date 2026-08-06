using System;
using System.Runtime.InteropServices;

namespace PyonPix.Interop;

public static class Win32Interop
{
	public struct POINT
	{
		public int X;

		public int Y;
	}

	public struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	public enum WindowLongFlags
	{
		GWL_EXSTYLE = -20,
		GWLP_HINSTANCE = -6,
		GWLP_HWNDPARENT = -8,
		GWL_ID = -12,
		GWL_STYLE = -16,
		GWL_USERDATA = -21,
		GWL_WNDPROC = -4,
		DWLP_USER = 8,
		DWLP_MSGRESULT = 0,
		DWLP_DLGPROC = 4
	}

	public enum IDC_STANDARD_CURSORS
	{
		IDC_ARROW = 32512,
		IDC_IBEAM = 32513,
		IDC_WAIT = 32514,
		IDC_CROSS = 32515,
		IDC_UPARROW = 32516,
		IDC_SIZE = 32640,
		IDC_ICON = 32641,
		IDC_SIZENWSE = 32642,
		IDC_SIZENESW = 32643,
		IDC_SIZEWE = 32644,
		IDC_SIZENS = 32645,
		IDC_SIZEALL = 32646,
		IDC_NO = 32648,
		IDC_HAND = 32649,
		IDC_APPSTARTING = 32650,
		IDC_HELP = 32651
	}

	public enum WM : uint
	{
		NULL = 0u,
		CREATE = 1u,
		DESTROY = 2u,
		MOVE = 3u,
		SIZE = 5u,
		ACTIVATE = 6u,
		SETFOCUS = 7u,
		KILLFOCUS = 8u,
		ENABLE = 10u,
		SETREDRAW = 11u,
		SETTEXT = 12u,
		GETTEXT = 13u,
		GETTEXTLENGTH = 14u,
		PAINT = 15u,
		CLOSE = 16u,
		QUERYENDSESSION = 17u,
		QUERYOPEN = 19u,
		ENDSESSION = 22u,
		QUIT = 18u,
		ERASEBKGND = 20u,
		SYSCOLORCHANGE = 21u,
		SHOWWINDOW = 24u,
		WININICHANGE = 26u,
		SETTINGCHANGE = 26u,
		DEVMODECHANGE = 27u,
		ACTIVATEAPP = 28u,
		FONTCHANGE = 29u,
		TIMECHANGE = 30u,
		CANCELMODE = 31u,
		SETCURSOR = 32u,
		MOUSEACTIVATE = 33u,
		CHILDACTIVATE = 34u,
		QUEUESYNC = 35u,
		GETMINMAXINFO = 36u,
		PAINTICON = 38u,
		ICONERASEBKGND = 39u,
		NEXTDLGCTL = 40u,
		SPOOLERSTATUS = 42u,
		DRAWITEM = 43u,
		MEASUREITEM = 44u,
		DELETEITEM = 45u,
		VKEYTOITEM = 46u,
		CHARTOITEM = 47u,
		SETFONT = 48u,
		GETFONT = 49u,
		SETHOTKEY = 50u,
		GETHOTKEY = 51u,
		QUERYDRAGICON = 55u,
		COMPAREITEM = 57u,
		GETOBJECT = 61u,
		COMPACTING = 65u,
		[Obsolete]
		COMMNOTIFY = 68u,
		WINDOWPOSCHANGING = 70u,
		WINDOWPOSCHANGED = 71u,
		[Obsolete]
		POWER = 72u,
		COPYDATA = 74u,
		CANCELJOURNAL = 75u,
		NOTIFY = 78u,
		INPUTLANGCHANGEREQUEST = 80u,
		INPUTLANGCHANGE = 81u,
		TCARD = 82u,
		HELP = 83u,
		USERCHANGED = 84u,
		NOTIFYFORMAT = 85u,
		CONTEXTMENU = 123u,
		STYLECHANGING = 124u,
		STYLECHANGED = 125u,
		DISPLAYCHANGE = 126u,
		GETICON = 127u,
		SETICON = 128u,
		NCCREATE = 129u,
		NCDESTROY = 130u,
		NCCALCSIZE = 131u,
		NCHITTEST = 132u,
		NCPAINT = 133u,
		NCACTIVATE = 134u,
		GETDLGCODE = 135u,
		SYNCPAINT = 136u,
		NCMOUSEMOVE = 160u,
		NCLBUTTONDOWN = 161u,
		NCLBUTTONUP = 162u,
		NCLBUTTONDBLCLK = 163u,
		NCRBUTTONDOWN = 164u,
		NCRBUTTONUP = 165u,
		NCRBUTTONDBLCLK = 166u,
		NCMBUTTONDOWN = 167u,
		NCMBUTTONUP = 168u,
		NCMBUTTONDBLCLK = 169u,
		NCXBUTTONDOWN = 171u,
		NCXBUTTONUP = 172u,
		NCXBUTTONDBLCLK = 173u,
		INPUT_DEVICE_CHANGE = 254u,
		INPUT = 255u,
		KEYDOWN = 256u,
		KEYUP = 257u,
		CHAR = 258u,
		DEADCHAR = 259u,
		SYSKEYDOWN = 260u,
		SYSKEYUP = 261u,
		SYSCHAR = 262u,
		SYSDEADCHAR = 263u,
		UNICHAR = 265u,
		KEYLAST = 265u,
		IME_STARTCOMPOSITION = 269u,
		IME_ENDCOMPOSITION = 270u,
		IME_COMPOSITION = 271u,
		IME_KEYLAST = 271u,
		INITDIALOG = 272u,
		COMMAND = 273u,
		SYSCOMMAND = 274u,
		TIMER = 275u,
		HSCROLL = 276u,
		VSCROLL = 277u,
		INITMENU = 278u,
		INITMENUPOPUP = 279u,
		MENUSELECT = 287u,
		MENUCHAR = 288u,
		ENTERIDLE = 289u,
		MENURBUTTONUP = 290u,
		MENUDRAG = 291u,
		MENUGETOBJECT = 292u,
		UNINITMENUPOPUP = 293u,
		MENUCOMMAND = 294u,
		CHANGEUISTATE = 295u,
		UPDATEUISTATE = 296u,
		QUERYUISTATE = 297u,
		CTLCOLORMSGBOX = 306u,
		CTLCOLOREDIT = 307u,
		CTLCOLORLISTBOX = 308u,
		CTLCOLORBTN = 309u,
		CTLCOLORDLG = 310u,
		CTLCOLORSCROLLBAR = 311u,
		CTLCOLORSTATIC = 312u,
		MOUSEFIRST = 512u,
		MOUSEMOVE = 512u,
		LBUTTONDOWN = 513u,
		LBUTTONUP = 514u,
		LBUTTONDBLCLK = 515u,
		RBUTTONDOWN = 516u,
		RBUTTONUP = 517u,
		RBUTTONDBLCLK = 518u,
		MBUTTONDOWN = 519u,
		MBUTTONUP = 520u,
		MBUTTONDBLCLK = 521u,
		MOUSEWHEEL = 522u,
		XBUTTONDOWN = 523u,
		XBUTTONUP = 524u,
		XBUTTONDBLCLK = 525u,
		MOUSEHWHEEL = 526u,
		MOUSELAST = 526u,
		PARENTNOTIFY = 528u,
		ENTERMENULOOP = 529u,
		EXITMENULOOP = 530u,
		NEXTMENU = 531u,
		SIZING = 532u,
		CAPTURECHANGED = 533u,
		MOVING = 534u,
		POWERBROADCAST = 536u,
		DEVICECHANGE = 537u,
		MDICREATE = 544u,
		MDIDESTROY = 545u,
		MDIACTIVATE = 546u,
		MDIRESTORE = 547u,
		MDINEXT = 548u,
		MDIMAXIMIZE = 549u,
		MDITILE = 550u,
		MDICASCADE = 551u,
		MDIICONARRANGE = 552u,
		MDIGETACTIVE = 553u,
		MDISETMENU = 560u,
		ENTERSIZEMOVE = 561u,
		EXITSIZEMOVE = 562u,
		DROPFILES = 563u,
		MDIREFRESHMENU = 564u,
		IME_SETCONTEXT = 641u,
		IME_NOTIFY = 642u,
		IME_CONTROL = 643u,
		IME_COMPOSITIONFULL = 644u,
		IME_SELECT = 645u,
		IME_CHAR = 646u,
		IME_REQUEST = 648u,
		IME_KEYDOWN = 656u,
		IME_KEYUP = 657u,
		MOUSEHOVER = 673u,
		MOUSELEAVE = 675u,
		NCMOUSEHOVER = 672u,
		NCMOUSELEAVE = 674u,
		WTSSESSION_CHANGE = 689u,
		TABLET_FIRST = 704u,
		TABLET_LAST = 735u,
		CUT = 768u,
		COPY = 769u,
		PASTE = 770u,
		CLEAR = 771u,
		UNDO = 772u,
		RENDERFORMAT = 773u,
		RENDERALLFORMATS = 774u,
		DESTROYCLIPBOARD = 775u,
		DRAWCLIPBOARD = 776u,
		PAINTCLIPBOARD = 777u,
		VSCROLLCLIPBOARD = 778u,
		SIZECLIPBOARD = 779u,
		ASKCBFORMATNAME = 780u,
		CHANGECBCHAIN = 781u,
		HSCROLLCLIPBOARD = 782u,
		QUERYNEWPALETTE = 783u,
		PALETTEISCHANGING = 784u,
		PALETTECHANGED = 785u,
		HOTKEY = 786u,
		PRINT = 791u,
		PRINTCLIENT = 792u,
		APPCOMMAND = 793u,
		THEMECHANGED = 794u,
		CLIPBOARDUPDATE = 797u,
		DWMCOMPOSITIONCHANGED = 798u,
		DWMNCRENDERINGCHANGED = 799u,
		DWMCOLORIZATIONCOLORCHANGED = 800u,
		DWMWINDOWMAXIMIZEDCHANGE = 801u,
		GETTITLEBARINFOEX = 831u,
		HANDHELDFIRST = 856u,
		HANDHELDLAST = 863u,
		AFXFIRST = 864u,
		AFXLAST = 895u,
		PENWINFIRST = 896u,
		PENWINLAST = 911u,
		APP = 32768u,
		USER = 1024u,
		CPL_LAUNCH = 5120u,
		CPL_LAUNCHED = 5121u,
		SYSTIMER = 280u,
		EXECUTE = 1280u
	}

	public static bool IsGameFocused
	{
		get
		{
			nint foregroundWindow = GetForegroundWindow();
			if (foregroundWindow == IntPtr.Zero)
			{
				return false;
			}
			GetWindowThreadProcessId(foregroundWindow, out var processId);
			return processId == Environment.ProcessId;
		}
	}

	[DllImport("user32.dll")]
	public static extern bool GetCursorPos(out POINT lpPoint);

	[DllImport("user32.dll")]
	private static extern bool SetCursorPos(int X, int Y);

	[DllImport("user32.dll")]
	private static extern int ShowCursor(bool bShow);

	[DllImport("user32.dll", EntryPoint = "LoadCursorW", ExactSpelling = true)]
	private static extern nint LoadCursor(nint hInstance, uint lpCursorName);

	[DllImport("user32.dll", ExactSpelling = true)]
	private static extern nint SetCursor(nint hCursor);

	[DllImport("user32.dll", ExactSpelling = true)]
	private static extern nint GetForegroundWindow();

	[DllImport("user32.dll", ExactSpelling = true)]
	private static extern int GetWindowThreadProcessId(nint handle, out int processId);

	public static bool BeginDrag(out int xPos, out int yPos)
	{
		if (GetDragCursorPos(out xPos, out yPos))
		{
			ShowCursor(bShow: false);
			return true;
		}
		return false;
	}

	public static bool GetDragCursorPos(out int xPos, out int yPos)
	{
		xPos = 0;
		yPos = 0;
		if (GetCursorPos(out var lpPoint))
		{
			xPos = lpPoint.X;
			yPos = lpPoint.Y;
			return true;
		}
		return false;
	}

	public static void SetDragCursorPos(int xPos, int yPos)
	{
		SetCursorPos(xPos, yPos);
	}

	public static void EndDrag()
	{
		ShowCursor(bShow: true);
	}

	public static void SetOSCursor(uint cursorId)
	{
		nint num = LoadCursor(IntPtr.Zero, cursorId);
		if (num != IntPtr.Zero)
		{
			SetCursor(num);
		}
	}
}
