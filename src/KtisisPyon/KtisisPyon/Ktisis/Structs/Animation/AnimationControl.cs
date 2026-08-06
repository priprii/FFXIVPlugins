using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.Havok.Animation.Playback.Control.Default;
using FFXIVClientStructs.Interop;
using FFXIVClientStructs.STD;

namespace Ktisis.Structs.Animation;

[StructLayout(LayoutKind.Explicit)]
public struct AnimationControl
{
	[StructLayout(LayoutKind.Explicit, Size = 40)]
	public struct Handle
	{
		[FieldOffset(0)]
		public ReferencedClassBase Ref;

		[FieldOffset(24)]
		public StdSet<Pointer<AnimationControl>> Set;
	}

	[FieldOffset(0)]
	public unsafe nint* __vfTable;

	[FieldOffset(56)]
	public unsafe hkaDefaultAnimationControl* HavokControl;
}
