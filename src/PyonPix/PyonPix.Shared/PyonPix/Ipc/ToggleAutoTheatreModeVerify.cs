using Google.FlatBuffers;

namespace PyonPix.Ipc;

public static class ToggleAutoTheatreModeVerify
{
	public static bool Verify(Verifier verifier, uint tablePos)
	{
		if (verifier.VerifyTableStart(tablePos) && verifier.VerifyField(tablePos, 4, 1uL, 1uL, required: false))
		{
			return verifier.VerifyTableEnd(tablePos);
		}
		return false;
	}
}
