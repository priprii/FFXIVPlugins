using Google.FlatBuffers;

namespace PyonPix.Ipc;

public static class HostStateVerify
{
	public static bool Verify(Verifier verifier, uint tablePos)
	{
		if (verifier.VerifyTableStart(tablePos) && verifier.VerifyField(tablePos, 4, 1uL, 1uL, required: false) && verifier.VerifyString(tablePos, 6, required: false))
		{
			return verifier.VerifyTableEnd(tablePos);
		}
		return false;
	}
}
