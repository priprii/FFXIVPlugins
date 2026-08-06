using System.Runtime.InteropServices;
using Ktisis.Structs.Vfx;

namespace Ktisis.Structs.Animation.Clips;

[StructLayout(LayoutKind.Explicit, Size = 392)]
public struct VfxClip
{
	[FieldOffset(0)]
	public BaseClip Clip;

	[FieldOffset(152)]
	public unsafe VfxControl* VfxControl;
}
