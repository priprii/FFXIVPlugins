using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace Ktisis.Structs.Actors;

[StructLayout(LayoutKind.Explicit, Size = 40)]
public struct Gaze
{
	[FieldOffset(8)]
	public GazeMode Mode;

	[FieldOffset(16)]
	public GameObjectId TargetId;

	[FieldOffset(16)]
	public Vector3 Pos;

	[FieldOffset(32)]
	public uint Unk5;
}
