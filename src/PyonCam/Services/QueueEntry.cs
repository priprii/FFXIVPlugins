using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Keys;

namespace PyonCam.Services;

[StructLayout(LayoutKind.Explicit)]
public struct QueueEntry
{
	[FieldOffset(0)]
	public KeyEvent Event;

	[FieldOffset(1)]
	public byte KeyCode;

	[FieldOffset(4)]
	public byte Unknown;

	public VirtualKey VirtualKey => (VirtualKey)KeyCode;
}
