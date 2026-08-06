using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace Ktisis.Structs.Actors;

[StructLayout(LayoutKind.Explicit, Size = 8928)]
public struct CharacterEx
{
	public const int AnimationOffset = 2608;

	public const int GazeOffset = 3456;

	[FieldOffset(0)]
	public Character Character;

	[FieldOffset(224)]
	public Vector3 DrawObjectOffset;

	[FieldOffset(304)]
	public Vector3 CameraOffsetSmooth;

	[FieldOffset(384)]
	public Vector3 CameraOffset;

	[FieldOffset(1584)]
	public unsafe nint* _emoteControllerVf;

	[FieldOffset(1584)]
	public EmoteController EmoteController;

	[FieldOffset(3298)]
	public CombatFlags CombatFlags;

	[FieldOffset(2608)]
	public AnimationContainer Animation;

	[FieldOffset(3472)]
	public ActorGaze Gaze;

	[FieldOffset(8936)]
	public float Opacity;

	[FieldOffset(9060)]
	public byte Mode;

	[FieldOffset(9061)]
	public EmoteModeEnum EmoteMode;

	public bool IsGPose
	{
		get
		{
			ushort objectIndex = Character.ObjectIndex;
			if (objectIndex >= 201)
			{
				return objectIndex <= 448;
			}
			return false;
		}
	}
}
