using System;

namespace TriggerPyon;

[Flags]
public enum RestoreType
{
	None = 0,
	Emote = 1,
	Target = 2,
	Rotation = 4,
	Position = 8
}
