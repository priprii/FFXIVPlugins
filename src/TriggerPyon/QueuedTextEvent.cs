namespace TriggerPyon;

public class QueuedTextEvent : QueuedEvent
{
	public string InstigatorName { get; }

	public string InstigatorMessage { get; }

	public EntityInfo? Instigator { get; }

	public ChatType Channel { get; }

	public QueuedTextEvent(string instigatorName, string instigatorMessage, EntityInfo? instigator, ChatType channel, Trigger trigger, ReactionBase reaction, int delay, int duration)
		: base(trigger, reaction, delay, duration)
	{
		InstigatorName = instigatorName;
		InstigatorMessage = instigatorMessage;
		Instigator = instigator;
		Channel = channel;
	}

	public override void Execute(TriggerManager manager)
	{
		if (base.Reaction is EmoteReaction reaction)
		{
			manager.PerformEmoteReaction(this, reaction);
		}
		if (base.Reaction is TextReaction reaction2)
		{
			manager.PerformTextReaction(this, reaction2);
		}
	}
}
