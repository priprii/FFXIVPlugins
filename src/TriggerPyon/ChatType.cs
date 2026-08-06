using System;

namespace TriggerPyon;

[Flags]
public enum ChatType
{
	None = 0,
	Command = 2,
	Emote = 4,
	CustomEmote = 8,
	Echo = 0x10,
	Say = 0x20,
	Yell = 0x40,
	Shout = 0x80,
	Party = 0x100,
	Alliance = 0x200,
	FC = 0x400,
	Tell = 0x800,
	CWLS1 = 0x1000,
	CWLS2 = 0x2000,
	CWLS3 = 0x4000,
	CWLS4 = 0x8000,
	CWLS5 = 0x10000,
	CWLS6 = 0x20000,
	CWLS7 = 0x40000,
	CWLS8 = 0x80000
}
