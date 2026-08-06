using Google.FlatBuffers;

namespace PyonPix.Ipc;

public static class CreateTabVerify
{
	public static bool Verify(Verifier verifier, uint tablePos)
	{
		if (verifier.VerifyTableStart(tablePos) && verifier.VerifyString(tablePos, 4, required: false) && verifier.VerifyField(tablePos, 6, 1uL, 1uL, required: false) && verifier.VerifyField(tablePos, 8, 4uL, 4uL, required: false) && verifier.VerifyField(tablePos, 10, 4uL, 4uL, required: false) && verifier.VerifyField(tablePos, 12, 4uL, 4uL, required: false) && verifier.VerifyField(tablePos, 14, 4uL, 4uL, required: false) && verifier.VerifyField(tablePos, 16, 1uL, 1uL, required: false) && verifier.VerifyVectorOfStrings(tablePos, 18, required: false))
		{
			return verifier.VerifyTableEnd(tablePos);
		}
		return false;
	}
}
