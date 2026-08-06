namespace TriggerPyon;

public class QueuedEmoteEvent : QueuedEvent
{
	public EntityInfo Instigator { get; }

	public EntityInfo? Receiver { get; }

	public ushort EmoteId { get; }

	public QueuedEmoteEvent(EntityInfo instigator, EntityInfo? receiver, ushort emoteId, Trigger trigger, ReactionBase reaction, int delay, int duration)
		: base(trigger, reaction, delay, duration)
	{
		Instigator = instigator;
		Receiver = receiver;
		EmoteId = emoteId;
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
