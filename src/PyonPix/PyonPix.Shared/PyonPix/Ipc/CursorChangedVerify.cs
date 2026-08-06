using Google.FlatBuffers;

namespace PyonPix.Ipc;

public static class CursorChangedVerify
{
	public static bool Verify(Verifier verifier, uint tablePos)
	{
		if (verifier.VerifyTableStart(tablePos) && verifier.VerifyField(tablePos, 4, 4uL, 4uL, required: false))
		{
			return verifier.VerifyTableEnd(tablePos);
		}
		return false;
	}
}
