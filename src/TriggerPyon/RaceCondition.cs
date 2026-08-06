using System;

namespace TriggerPyon;

[Flags]
public enum RaceCondition
{
	Any = 0,
	Midlander = 1,
	Highlander = 2,
	Elezen = 4,
	Miqote = 8,
	Roegadyn = 0x10,
	Lalafell = 0x20,
	AuRa = 0x40,
	Hrothgar = 0x80,
	Viera = 0x100
}
