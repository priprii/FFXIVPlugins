using Google.FlatBuffers;

namespace PyonPix.Ipc;

public static class UpdateMediaStateVerify
{
	public static bool Verify(Verifier verifier, uint tablePos)
	{
		if (verifier.VerifyTableStart(tablePos) && verifier.VerifyString(tablePos, 4, required: false) && verifier.VerifyField(tablePos, 6, 1uL, 1uL, required: false) && verifier.VerifyField(tablePos, 8, 1uL, 1uL, required: false) && verifier.VerifyField(tablePos, 10, 8uL, 8uL, required: false) && verifier.VerifyField(tablePos, 12, 8uL, 8uL, required: false) && verifier.VerifyField(tablePos, 14, 8uL, 8uL, required: false))
		{
			return verifier.VerifyTableEnd(tablePos);
		}
		return false;
	}
}
