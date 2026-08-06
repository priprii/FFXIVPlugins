using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace Ktisis.Structs.Events;

[StructLayout(LayoutKind.Explicit, Size = 304)]
public struct GPoseActorEvent
{
	[FieldOffset(0)]
	public unsafe nint* __vfTable;

	[FieldOffset(32)]
	public ulong EntityID;

	[FieldOffset(208)]
	public unsafe Character* Character;

	[FieldOffset(224)]
	public uint ObjectID;

	[FieldOffset(264)]
	public uint _param4;

	[FieldOffset(268)]
	public uint _param5;

	[FieldOffset(272)]
	public uint _param6;

	[FieldOffset(276)]
	public uint CopyObjectIndex;
}
