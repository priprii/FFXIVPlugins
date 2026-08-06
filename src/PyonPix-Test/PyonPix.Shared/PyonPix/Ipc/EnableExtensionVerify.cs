using Google.FlatBuffers;

namespace PyonPix.Ipc;

public static class EnableExtensionVerify
{
	public static bool Verify(Verifier verifier, uint tablePos)
	{
		if (verifier.VerifyTableStart(tablePos) && verifier.VerifyString(tablePos, 4, required: false) && verifier.VerifyString(tablePos, 6, required: false))
		{
			return verifier.VerifyTableEnd(tablePos);
		}
		return false;
	}
}
