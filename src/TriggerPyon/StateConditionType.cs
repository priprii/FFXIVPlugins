using System;

namespace TriggerPyon;

[Flags]
public enum StateConditionType
{
	None = 0,
	Moving = 1,
	Standing = 2,
	GroundSit = 4,
	ChairSit = 8,
	Sleeping = 0x10,
	Emote = 0x20,
	LoopingEmote = 0x40
}
