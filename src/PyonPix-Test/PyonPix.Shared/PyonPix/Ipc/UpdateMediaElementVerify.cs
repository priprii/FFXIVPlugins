using Google.FlatBuffers;

namespace PyonPix.Ipc;

public static class UpdateMediaElementVerify
{
	public static bool Verify(Verifier verifier, uint tablePos)
	{
		if (verifier.VerifyTableStart(tablePos) && verifier.VerifyString(tablePos, 4, required: false) && verifier.VerifyField(tablePos, 6, 1uL, 1uL, required: false) && verifier.VerifyString(tablePos, 8, required: false) && verifier.VerifyField(tablePos, 10, 1uL, 1uL, required: false) && verifier.VerifyField(tablePos, 12, 1uL, 1uL, required: false) && verifier.VerifyField(tablePos, 14, 8uL, 8uL, required: false) && verifier.VerifyField(tablePos, 16, 8uL, 8uL, required: false) && verifier.VerifyField(tablePos, 18, 8uL, 8uL, required: false))
		{
			return verifier.VerifyTableEnd(tablePos);
		}
		return false;
	}
}
