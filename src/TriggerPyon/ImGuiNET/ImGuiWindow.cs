using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Bindings.ImGui;

namespace ImGuiNET;

[StructLayout(LayoutKind.Explicit)]
public struct ImGuiWindow
{
	[FieldOffset(12)]
	public ImGuiWindowFlags Flags;

	[FieldOffset(213)]
	public byte HasCloseButton;

	[FieldOffset(304)]
	public Vector2 CursorMaxPos;
}
