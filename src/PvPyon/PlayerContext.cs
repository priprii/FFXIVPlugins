namespace PvPyon;

public enum PlayerContext
{
	None = 0,
	Self = 1,
	Party = 2,
	Alliance = 4,
	Enemy = 8,
	Friend = 0x10
}
