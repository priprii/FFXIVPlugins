using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TriggerPyon;

public class ReactionQueue
{
	private class QueueContext
	{
		public Trigger Trigger { get; }

		public RestoreContext? RestoreContext { get; }

		public Queue<QueuedEvent> EmoteQueue { get; } = new Queue<QueuedEvent>();

		public Queue<QueuedEvent> TextQueue { get; } = new Queue<QueuedEvent>();

		public CancellationTokenSource Cancellation { get; }

		public QueueContext(Trigger trigger, RestoreContext? restoreContext)
		{
			Trigger = trigger;
			RestoreContext = restoreContext;
			Cancellation = new CancellationTokenSource();
		}

		public void Cancel()
		{
			try
			{
				Cancellation.Cancel();
			}
			catch (ObjectDisposedException)
			{
			}
		}
	}

	private readonly Plugin plugin;

	private readonly TriggerManager TriggerManager;

	private readonly Stack<QueueContext> queueStack = new Stack<QueueContext>();

	private bool processingEmote;

	private bool processingText;

	public ReactionQueue(Plugin plugin, TriggerManager triggerManager)
	{
		this.plugin = plugin;
		TriggerManager = triggerManager;
	}

	public void EnqueueEmote(EntityInfo instigator, EntityInfo? receiver, ushort emoteId, Trigger trigger, CounterManager? counterManager = null)
	{
		if (PlayerManager.LocalPlayer == null)
		{
			return;
		}
		if (counterManager != null && trigger.ReactionOptions.CountFailedConditions)
		{
			counterManager.UpdateCounter(trigger, instigator, receiver);
		}
		if (!HandleInterrupt(trigger, out QueueContext ctx))
		{
			return;
		}
		List<EmoteReaction> list = trigger.Reactions?.OfType<EmoteReaction>().ToList() ?? new List<EmoteReaction>();
		List<TextReaction> list2 = trigger.Reactions?.OfType<TextReaction>().ToList() ?? new List<TextReaction>();
		if (list.Count == 0 && list2.Count == 0)
		{
			if (counterManager == null || trigger.ReactionOptions.CountFailedConditions)
			{
				return;
			}
			bool flag = true;
			if (trigger.ReactionOptions.RestrictRange)
			{
				flag = false;
				if (!instigator.IsLocalPlayer && instigator.IsWithinReactionAngleAndDistanceToLocalPlayer(trigger.ReactionOptions))
				{
					flag = true;
				}
				else if (instigator.IsLocalPlayer && receiver != null && receiver.IsWithinReactionAngleAndDistanceToLocalPlayer(trigger.ReactionOptions))
				{
					flag = true;
				}
			}
			if (flag && trigger.ReactionOptions.RestrictTerritory)
			{
				flag = trigger.ReactionOptions.MeetsTerritoryConditions();
			}
			if (flag)
			{
				counterManager.UpdateCounter(trigger, instigator, receiver);
			}
		}
		else
		{
			if (!PlayerManager.LocalPlayer.CanReactionInterruptCurrentState(trigger.ReactionOptions) || (trigger.ReactionOptions.RestrictRange && ((!instigator.IsLocalPlayer && !instigator.IsWithinReactionAngleAndDistanceToLocalPlayer(trigger.ReactionOptions)) || (instigator.IsLocalPlayer && receiver != null && !receiver.IsWithinReactionAngleAndDistanceToLocalPlayer(trigger.ReactionOptions)))) || (trigger.ReactionOptions.RestrictTerritory && !trigger.ReactionOptions.MeetsTerritoryConditions()))
			{
				return;
			}
			if (counterManager != null && !trigger.ReactionOptions.CountFailedConditions)
			{
				counterManager.UpdateCounter(trigger, instigator, receiver);
			}
			ctx?.Cancel();
			QueueContext queueContext = new QueueContext(trigger, (trigger.ReactionOptions.RestoreType != RestoreType.None) ? new RestoreContext(plugin, trigger.ReactionOptions.RestoreType) : null);
			int delay = ((list.Count > 0) ? list[0].Delay : 0);
			foreach (EmoteReaction item in list)
			{
				queueContext.EmoteQueue.Enqueue(new QueuedEmoteEvent(instigator, receiver, emoteId, trigger, item, delay, item.Duration));
				delay = 0;
			}
			int delay2 = ((list2.Count <= 0) ? 1 : (list2[0].Delay + 1));
			foreach (TextReaction item2 in list2)
			{
				queueContext.TextQueue.Enqueue(new QueuedEmoteEvent(instigator, receiver, emoteId, trigger, item2, delay2, item2.Duration));
				delay2 = 1;
			}
			queueStack.Push(queueContext);
			if (!processingEmote)
			{
				ProcessEmoteQueue();
			}
			if (!processingText)
			{
				ProcessTextQueue();
			}
		}
	}

	public void EnqueueText(string instigatorName, string instigatorMessage, EntityInfo? instigator, ChatType channel, Trigger trigger, CounterManager? counterManager = null)
	{
		if (PlayerManager.LocalPlayer == null)
		{
			return;
		}
		if (counterManager != null && trigger.ReactionOptions.CountFailedConditions)
		{
			counterManager.UpdateCounter(trigger, instigatorName);
		}
		if (!HandleInterrupt(trigger, out QueueContext ctx))
		{
			return;
		}
		List<EmoteReaction> list = trigger.Reactions?.OfType<EmoteReaction>().ToList() ?? new List<EmoteReaction>();
		List<TextReaction> list2 = trigger.Reactions?.OfType<TextReaction>().ToList() ?? new List<TextReaction>();
		if (list.Count == 0 && list2.Count == 0)
		{
			if (counterManager != null && !trigger.ReactionOptions.CountFailedConditions)
			{
				bool flag = true;
				if (trigger.ReactionOptions.RestrictRange && instigator != null && !instigator.IsLocalPlayer)
				{
					flag = instigator.IsWithinReactionAngleAndDistanceToLocalPlayer(trigger.ReactionOptions);
				}
				if (flag && trigger.ReactionOptions.RestrictTerritory)
				{
					flag = trigger.ReactionOptions.MeetsTerritoryConditions();
				}
				if (flag)
				{
					counterManager.UpdateCounter(trigger, instigatorName);
				}
			}
		}
		else
		{
			if (!PlayerManager.LocalPlayer.CanReactionInterruptCurrentState(trigger.ReactionOptions) || (trigger.ReactionOptions.RestrictRange && instigator != null && !instigator.IsLocalPlayer && !instigator.IsWithinReactionAngleAndDistanceToLocalPlayer(trigger.ReactionOptions)) || (trigger.ReactionOptions.RestrictTerritory && !trigger.ReactionOptions.MeetsTerritoryConditions()))
			{
				return;
			}
			if (counterManager != null && !trigger.ReactionOptions.CountFailedConditions)
			{
				counterManager.UpdateCounter(trigger, instigatorName);
			}
			ctx?.Cancel();
			QueueContext queueContext = new QueueContext(trigger, (trigger.ReactionOptions.RestoreType != RestoreType.None) ? new RestoreContext(plugin, trigger.ReactionOptions.RestoreType) : null);
			int delay = ((list.Count > 0) ? list[0].Delay : 0);
			foreach (EmoteReaction item in list)
			{
				queueContext.EmoteQueue.Enqueue(new QueuedTextEvent(instigatorName, instigatorMessage, instigator, channel, trigger, item, delay, item.Duration));
				delay = 0;
			}
			int delay2 = ((list2.Count <= 0) ? 1 : (list2[0].Delay + 1));
			foreach (TextReaction item2 in list2)
			{
				queueContext.TextQueue.Enqueue(new QueuedTextEvent(instigatorName, instigatorMessage, instigator, channel, trigger, item2, delay2, item2.Duration));
				delay2 = 1;
			}
			queueStack.Push(queueContext);
			if (!processingEmote)
			{
				ProcessEmoteQueue();
			}
			if (!processingText)
			{
				ProcessTextQueue();
			}
		}
	}

	private bool HandleInterrupt(Trigger trigger, out QueueContext? ctx)
	{
		ctx = null;
		if (queueStack.Count == 0)
		{
			return true;
		}
		ctx = queueStack.Peek();
		if (trigger.ReactionOptions.InterruptType == ReactionInterruptType.None)
		{
			return false;
		}
		if (trigger.ReactionOptions.InterruptType == ReactionInterruptType.Same && trigger != ctx.Trigger)
		{
			return false;
		}
		if (trigger.ReactionOptions.InterruptType == ReactionInterruptType.Other && trigger == ctx.Trigger)
		{
			return false;
		}
		return true;
	}

	private async Task ProcessEmoteQueue()
	{
		bool isCanceled = false;
		processingEmote = true;
		try
		{
			while (queueStack.Count > 0 && queueStack.Peek().EmoteQueue.Count > 0)
			{
				QueueContext queueContext = queueStack.Peek();
				QueuedEvent item = queueContext.EmoteQueue.Dequeue();
				isCanceled = await ProcessQueueItemAsync(item, queueContext);
			}
		}
		finally
		{
			processingEmote = false;
			if (!isCanceled)
			{
				HandleQueueCompletion();
			}
		}
	}

	private async Task ProcessTextQueue()
	{
		bool isCanceled = false;
		processingText = true;
		try
		{
			while (queueStack.Count > 0 && queueStack.Peek().TextQueue.Count > 0)
			{
				QueueContext queueContext = queueStack.Peek();
				QueuedEvent item = queueContext.TextQueue.Dequeue();
				isCanceled = await ProcessQueueItemAsync(item, queueContext);
			}
		}
		finally
		{
			processingText = false;
			if (!isCanceled)
			{
				HandleQueueCompletion();
			}
		}
	}

	private async Task<bool> ProcessQueueItemAsync(QueuedEvent item, QueueContext ctx)
	{
		_ = 1;
		try
		{
			if (item.Delay > 0)
			{
				await Task.Delay(item.Delay, ctx.Cancellation.Token);
			}
			item.Execute(TriggerManager);
			if (item.Duration > 0)
			{
				await Task.Delay(item.Duration, ctx.Cancellation.Token);
			}
		}
		catch (TaskCanceledException)
		{
			return true;
		}
		return false;
	}

	private void HandleQueueCompletion()
	{
		if (!processingEmote && !processingText && queueStack.Count != 0)
		{
			while (queueStack.Count > 1)
			{
				queueStack.Pop();
			}
			QueueContext queueContext = queueStack.Pop();
			if (queueStack.Count == 0)
			{
				queueContext.RestoreContext?.Restore();
				TriggerManager.PreviewMode = false;
			}
		}
	}
}
