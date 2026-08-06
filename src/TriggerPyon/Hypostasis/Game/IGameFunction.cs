namespace Hypostasis.Game;

public interface IGameFunction
{
	string Signature { get; }

	nint Address { get; }

	bool IsValid { get; }

	bool IsHooked { get; }
}
