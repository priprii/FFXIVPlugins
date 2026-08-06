using Google.FlatBuffers;

namespace PyonPix.Ipc;

public static class RepositionVerify
{
	public static bool Verify(Verifier verifier, uint tablePos)
	{
		if (verifier.VerifyTableStart(tablePos) && verifier.VerifyString(tablePos, 4, required: false) && verifier.VerifyField(tablePos, 6, 4uL, 4uL, required: false) && verifier.VerifyField(tablePos, 8, 4uL, 4uL, required: false))
		{
			return verifier.VerifyTableEnd(tablePos);
		}
		return false;
	}
}
