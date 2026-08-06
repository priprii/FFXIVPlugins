namespace Hypostasis.Dalamud;

public sealed class HypostasisSignatureInjectionAttribute(string signature) : HypostasisMemberInjectionAttribute
{
	public string Signature { get; init; } = signature;

	public bool Static { get; init; }
}
