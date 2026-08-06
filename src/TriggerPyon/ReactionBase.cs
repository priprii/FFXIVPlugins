namespace TriggerPyon;

public abstract class ReactionBase
{
	public abstract TriggerType ObjType { get; }

	public int Delay { get; set; }

	public int Duration { get; set; }
}
