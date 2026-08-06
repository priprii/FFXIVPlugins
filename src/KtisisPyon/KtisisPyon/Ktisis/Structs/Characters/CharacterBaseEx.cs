using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Ktisis.Structs.Attachment;

namespace Ktisis.Structs.Characters;

[StructLayout(LayoutKind.Explicit, Size = 2576)]
public struct CharacterBaseEx
{
	[FieldOffset(0)]
	public CharacterBase Base;

	[FieldOffset(80)]
	public Transform Transform;

	[FieldOffset(216)]
	public Attach Attach;

	[FieldOffset(736)]
	public WetnessState Wetness;

	[FieldOffset(2592)]
	public CustomizeContainer Customize;

	[FieldOffset(2624)]
	public EquipmentContainer Equipment;
}
