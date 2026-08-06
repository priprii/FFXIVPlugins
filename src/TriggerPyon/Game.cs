using Hypostasis.Dalamud;

namespace TriggerPyon;

[HypostasisInjection]
public static class Game
{
	[HypostasisSignatureInjection("F3 0F 10 05 ?? ?? ?? ?? 0F 2E C7", Offset = 4, Static = true, Required = true)]
	private static nint forceDisableMovementPtr;

	public unsafe static ref int ForceDisableMovement => ref *(int*)forceDisableMovementPtr;
}
