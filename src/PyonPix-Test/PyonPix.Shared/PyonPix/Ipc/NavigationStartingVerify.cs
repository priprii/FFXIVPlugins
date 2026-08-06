using Google.FlatBuffers;

namespace PyonPix.Ipc;

public static class NavigationStartingVerify
{
	public static bool Verify(Verifier verifier, uint tablePos)
	{
		if (verifier.VerifyTableStart(tablePos) && verifier.VerifyString(tablePos, 4, required: false) && verifier.VerifyString(tablePos, 6, required: false) && verifier.VerifyField(tablePos, 8, 1uL, 1uL, required: false))
		{
			return verifier.VerifyTableEnd(tablePos);
		}
		return false;
	}
}
