using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Discord;

namespace TriggerPyon;

public class TriggerManager
{
	private Plugin plugin;

	public CounterManager CounterManager;

	private ReactionQueue ReactionQueue;

	public bool PreviewMode;

	public TriggerManager(Plugin plugin)
	{
		this.plugin = plugin;
		CounterManager = new CounterManager(plugin);
		ReactionQueue = new ReactionQueue(plugin, this);
		plugin.EmoteHook.OnEmote += OnEmote;
		plugin.Chat.OnChat += OnChat;
		plugin.DiscordManager.OnActivity += OnActivity;
	}

	public void Update()
	{
		CounterManager.Update();
	}

	public void Dispose()
	{
		plugin.EmoteHook.OnEmote -= OnEmote;
		plugin.Chat.OnChat -= OnChat;
		CounterManager.Dispose();
	}

	public void PreviewTitle(Trigger trigger, Counter counter)
	{
		CounterManager.SetTitle(trigger, counter, "Primu", "Pyon", "Miyu", "Myon");
	}

	public void PreviewToast(Counter counter)
	{
		CounterManager.SetToast(counter, "Primu", "Pyon", "Miyu", "Myon");
	}

	public void PreviewEcho(Counter counter)
	{
		CounterManager.SetEcho(counter, "Primu", "Pyon", "Miyu", "Myon");
	}

	public void PreviewQueue(Trigger trigger)
	{
		if (trigger.Reactions == null || trigger.Reactions.Count == 0)
		{
			return;
		}
		EntityInfo localPlayer = PlayerManager.LocalPlayer;
		if (localPlayer == null)
		{
			return;
		}
		PreviewMode = true;
		if (trigger.Type == TriggerType.Emote)
		{
			EmoteTargetReceiver emoteTargetReceiver = trigger.Receiver as EmoteTargetReceiver;
			if (localPlayer.IsTargetValid)
			{
				IGameObject target = localPlayer.Target;
				IPlayerCharacter val = (IPlayerCharacter)(object)((target is IPlayerCharacter) ? target : null);
				if (val != null)
				{
					EntityInfo entityInfo = new EntityInfo(val);
					Instigator? instigator = trigger.Instigator;
					EntityInfo instigator2 = ((instigator != null && instigator.Type == PlayerType.Self) ? localPlayer : entityInfo);
					EntityInfo receiver = ((emoteTargetReceiver != null && emoteTargetReceiver.Type == PlayerType.Self) ? localPlayer : ((emoteTargetReceiver == null || emoteTargetReceiver.Type != PlayerType.None) ? entityInfo : null));
					ReactionQueue.EnqueueEmote(instigator2, receiver, 0, trigger);
					return;
				}
			}
			if ((emoteTargetReceiver == null || emoteTargetReceiver.Type != PlayerType.None) && (emoteTargetReceiver == null || emoteTargetReceiver.Type != PlayerType.Others))
			{
				if (emoteTargetReceiver == null)
				{
					_ = 1;
				}
				else
					_ = emoteTargetReceiver.Type != PlayerType.Player;
			}
			ReactionQueue.EnqueueEmote(localPlayer, ((emoteTargetReceiver == null || emoteTargetReceiver.Type != PlayerType.None) && (emoteTargetReceiver == null || emoteTargetReceiver.Type != PlayerType.Others)) ? localPlayer : null, 0, trigger);
		}
		else if (trigger.Type == TriggerType.Text)
		{
			ReactionQueue.EnqueueText(localPlayer.Name, string.Empty, null, ChatType.Echo, trigger);
		}
		else
		{
			PreviewMode = false;
		}
	}

	private void OnActivity(IActivity? activity, Trigger? trigger)
	{
		if (activity == null || trigger == null)
		{
			CounterManager.ClearDiscordTitle();
		}
		else if (!PreviewMode)
		{
			CounterManager.UpdateDiscordCounter(activity, trigger);
		}
	}

	private void OnEmote(EntityInfo instigator, EntityInfo? receiver, ushort emoteId, Trigger trigger)
	{
		if (PreviewMode)
		{
			return;
		}
		if (trigger.Reactions == null || trigger.Reactions.Count == 0)
		{
			CounterManager.UpdateCounter(trigger, instigator, receiver);
			return;
		}
		long tickCount = Environment.TickCount64;
		if (tickCount - trigger.LastReactionTime >= trigger.ReactionOptions.ReactionCooldown)
		{
			trigger.LastReactionTime = tickCount;
			ReactionQueue.EnqueueEmote(instigator, receiver, emoteId, trigger, CounterManager);
		}
	}

	private void OnChat(string instigatorName, string instigatorMessage, EntityInfo? instigator, ChatType channel, Trigger trigger)
	{
		if (PreviewMode)
		{
			return;
		}
		if (trigger.Reactions == null || trigger.Reactions.Count == 0)
		{
			CounterManager.UpdateCounter(trigger, instigatorName);
			return;
		}
		long tickCount = Environment.TickCount64;
		if (tickCount - trigger.LastReactionTime >= trigger.ReactionOptions.ReactionCooldown)
		{
			trigger.LastReactionTime = tickCount;
			ReactionQueue.EnqueueText(instigatorName, instigatorMessage, instigator, channel, trigger, CounterManager);
		}
	}

	public void PerformEmoteReaction(QueuedEmoteEvent qr, EmoteReaction reaction)
	{
		Plugin.Framework.RunOnFrameworkThread((Action)delegate
		{
			if (reaction.CopyInstigator)
			{
				if (!PreviewMode)
				{
					Emote instEmote = plugin.Emotes.FirstOrDefault((Emote e) => e.ID == qr.EmoteId);
					Emote emote = plugin.Emotes.FirstOrDefault((Emote e) => e.Name == instEmote?.Name && (!string.IsNullOrWhiteSpace(e.Command) || e.IsPose));
					plugin.EmoteHook.PerformEmote(emote, qr.Trigger, reaction, qr.Instigator, qr.Receiver);
				}
			}
			else
			{
				Emote emote2 = plugin.Emotes.FirstOrDefault((Emote e) => e.ID == reaction.ID && (!string.IsNullOrWhiteSpace(e.Command) || e.IsPose));
				plugin.EmoteHook.PerformEmote(emote2, qr.Trigger, reaction, qr.Instigator, qr.Receiver);
			}
		});
	}

	public void PerformEmoteReaction(QueuedTextEvent qr, EmoteReaction reaction)
	{
		Plugin.Framework.RunOnFrameworkThread((Action)delegate
		{
			Emote emote = plugin.Emotes.FirstOrDefault((Emote e) => e.ID == reaction.ID && (!string.IsNullOrWhiteSpace(e.Command) || e.IsPose));
			plugin.EmoteHook.PerformEmote(emote, qr.Trigger, reaction, qr.Instigator, null);
		});
	}

	public void PerformTextReaction(QueuedEmoteEvent qr, TextReaction reaction)
	{
		Plugin.Framework.RunOnFrameworkThread((Action)delegate
		{
			if (reaction.Channel != ChatType.None && reaction.Channel != ChatType.Emote && !string.IsNullOrWhiteSpace(reaction.Template))
			{
				string message = reaction.Template.Replace("%ifn%", qr.Instigator.Forename).Replace("%isn%", qr.Instigator.Surname);
				plugin.Chat.SendMessage(PreviewMode ? ChatType.Echo : reaction.Channel, message, qr.Instigator.Forename, qr.Instigator.Surname, qr.Instigator.HomeWorld);
			}
		});
	}

	public void PerformTextReaction(QueuedTextEvent qr, TextReaction reaction)
	{
		Plugin.Framework.RunOnFrameworkThread((Action)delegate
		{
			if ((reaction.SameChannel || (reaction.Channel != ChatType.None && reaction.Channel != ChatType.Emote)) && (reaction.CopyInstigator || !string.IsNullOrWhiteSpace(reaction.Template)))
			{
				Instigator? instigator = qr.Trigger.Instigator;
				if (instigator == null || instigator.Type != PlayerType.Self || reaction.Channel == ChatType.Echo || (!reaction.SameChannel && reaction.Channel != qr.Channel))
				{
					string forename = qr.InstigatorName.GetForename();
					(string, string?) surnameWorld = qr.InstigatorName.GetSurnameWorld();
					string item = surnameWorld.Item1;
					string item2 = surnameWorld.Item2;
					string text = (reaction.CopyInstigator ? qr.InstigatorMessage : reaction.Template.Replace("%ifn%", forename).Replace("%isn%", item));
					plugin.Chat.SendMessage(PreviewMode ? ChatType.Echo : (reaction.SameChannel ? qr.Channel : reaction.Channel), PreviewMode ? (qr.InstigatorName + ": " + text) : text, forename, item, item2);
				}
			}
		});
	}
}
