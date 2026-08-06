namespace TriggerPyon;

public abstract class QueuedEvent
{
	public Trigger Trigger { get; }

	public ReactionBase Reaction { get; }

	public int Delay { get; }

	public int Duration { get; }

	protected QueuedEvent(Trigger trigger, ReactionBase reaction, int delay, int duration)
	{
		Trigger = trigger;
		Reaction = reaction;
		Delay = delay;
		Duration = duration;
	}

	public abstract void Execute(TriggerManager manager);
}
