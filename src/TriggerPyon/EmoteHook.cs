using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace TriggerPyon;

public class EmoteHook
{
	public delegate void OnEmoteFuncDelegate(ulong unk, ulong instigatorAddr, ushort emoteId, ulong targetId, ulong unk2);

	public unsafe delegate bool CancelEmoteFuncDelegate(EmoteController* emoteController, nint unknown);

	private Plugin plugin;

	private Hook<OnEmoteFuncDelegate>? HookEmote { get; init; }

	private Hook<CancelEmoteFuncDelegate>? HookCancelEmote { get; init; }

	public event Action<EntityInfo, EntityInfo?, ushort, Trigger>? OnEmote;

	public EmoteHook(Plugin plugin)
	{
		this.plugin = plugin;
		try
		{
			HookEmote = Plugin.GameInteropProvider.HookFromSignature<OnEmoteFuncDelegate>("E8 ?? ?? ?? ?? 48 8D 8B ?? ?? ?? ?? 4C 89 74 24", (OnEmoteFuncDelegate)OnEmoteDetour, (HookBackend)0);
			HookEmote.Enable();
		}
		catch (Exception ex)
		{
			Plugin.Log.Error(ex, "EmoteHook Exception", Array.Empty<object>());
		}
	}

	public bool IsEmoteCancelPreventionEnabled()
	{
		if (HookCancelEmote != null)
		{
			return HookCancelEmote.IsEnabled;
		}
		return false;
	}

	public void DisableEmoteCancelPrevention()
	{
		if (HookCancelEmote != null && HookCancelEmote.IsEnabled)
		{
			HookCancelEmote.Disable();
		}
	}

	public void EnableEmoteCancelPrevention()
	{
		if (HookCancelEmote != null && !HookCancelEmote.IsEnabled)
		{
			HookCancelEmote.Enable();
		}
	}

	public void PerformEmote(Emote? emote, Trigger trigger, EmoteReaction emoteReaction, EntityInfo? instigator, EntityInfo? receiver)
	{
		if (emote == null)
		{
			return;
		}
		EntityInfo localPlayer = PlayerManager.LocalPlayer;
		if (localPlayer != null)
		{
			switch (emoteReaction.TargetType)
			{
			case ReactionTargetType.Untarget:
				Plugin.Targets.Target = null;
				break;
			case ReactionTargetType.TargetInstigator:
				instigator?.SetAsTarget();
				break;
			case ReactionTargetType.TargetReceiver:
				receiver?.SetAsTarget();
				break;
			case ReactionTargetType.TargetSelf:
				localPlayer.SetAsTarget();
				break;
			}
			if (emoteReaction.LookAtType != ReactionLookAtType.Target)
			{
				ForbidEmoteLookAtChange();
			}
			switch (emoteReaction.LookAtType)
			{
			case ReactionLookAtType.Instigator:
				localPlayer.FaceTowardsEntity(instigator);
				break;
			case ReactionLookAtType.Receiver:
				localPlayer.FaceTowardsEntity(receiver);
				break;
			case ReactionLookAtType.InstigatorInverse:
				localPlayer.FaceTowardsEntity(instigator, inverse: true);
				break;
			case ReactionLookAtType.ReceiverInverse:
				localPlayer.FaceTowardsEntity(receiver, inverse: true);
				break;
			case ReactionLookAtType.InstigatorDirection:
				localPlayer.FaceSameAsEntity(instigator);
				break;
			case ReactionLookAtType.ReceiverDirection:
				localPlayer.FaceSameAsEntity(receiver);
				break;
			case ReactionLookAtType.InstigatorDirectionInverse:
				localPlayer.FaceSameAsEntity(instigator, inverse: true);
				break;
			case ReactionLookAtType.ReceiverDirectionInverse:
				localPlayer.FaceSameAsEntity(receiver, inverse: true);
				break;
			}
			if (!string.IsNullOrWhiteSpace(emote.Command))
			{
				PerformEmoteCommand(emote.Command, trigger, emoteReaction);
			}
			else if (emote.IsPose)
			{
				PerformPoseEmote(emote, trigger, emoteReaction);
			}
			if (emoteReaction.LookAtType != ReactionLookAtType.Target)
			{
				AllowEmoteLookAtChange();
			}
		}
	}

	private void PerformPoseEmote(Emote emote, Trigger trigger, EmoteReaction emoteReaction)
	{
	}

	public void PerformEmoteCommand(string command, Trigger trigger, EmoteReaction emoteReaction)
	{
		plugin.Chat.SendMessage(ChatType.Emote, command);
	}

	public void ForbidEmoteLookAtChange()
	{
		Game.ForceDisableMovement++;
	}

	public void AllowEmoteLookAtChange()
	{
		if (Game.ForceDisableMovement > 0)
		{
			Game.ForceDisableMovement--;
		}
	}

	public void Dispose()
	{
		HookEmote?.Dispose();
	}

	private void OnEmoteDetour(ulong unk, ulong instigatorAddr, ushort emoteId, ulong targetId, ulong unk2)
	{
		if (Plugin.Config.Enabled && PlayerManager.LocalPlayer != null)
		{
			EntityInfo entityInfo = PlayerManager.NearbyPlayers.FirstOrDefault((EntityInfo x) => (ulong)(nint)((IGameObject)x.Character).Address == instigatorAddr);
			if (entityInfo != null)
			{
				EntityInfo localPlayer = PlayerManager.LocalPlayer;
				EntityInfo entityInfo2 = null;
				foreach (Trigger trigger in Plugin.Config.Triggers)
				{
					if (!trigger.Enabled || trigger.Type != TriggerType.Emote || trigger.Instigator == null)
					{
						continue;
					}
					Instigator? instigator = trigger.Instigator;
					if (instigator != null && instigator.Type == PlayerType.None)
					{
						continue;
					}
					Instigator? instigator2 = trigger.Instigator;
					if ((instigator2 == null || instigator2.Type != PlayerType.Ignore) && trigger.ReceivedAction is EmoteAction emoteAction && (emoteAction.MatchAny || emoteAction.IDs.Contains(emoteId)) && (trigger.Instigator == null || ((trigger.Instigator.Type == PlayerType.All || ((trigger.Instigator.Type != PlayerType.Self || ((IGameObject)entityInfo.Character).Address == ((IGameObject)localPlayer.Character).Address) && (trigger.Instigator.Type != PlayerType.Others || ((IGameObject)entityInfo.Character).Address != ((IGameObject)localPlayer.Character).Address) && (trigger.Instigator.Type != PlayerType.Player || trigger.Instigator.PlayerNameMatches(entityInfo.Name)) && (trigger.Instigator.Type != PlayerType.Target || (localPlayer.IsTargetValid && ((IGameObject)entityInfo.Character).Address == localPlayer.Target.Address)) && (trigger.Instigator.Type != PlayerType.Targeter || (entityInfo.IsTargetValid && entityInfo.Target.Address == ((IGameObject)localPlayer.Character).Address)))) && (trigger.Instigator.Type == PlayerType.Self || !trigger.Instigator.BlacklistNameMatches(entityInfo.Name)) && (trigger.Instigator.Type == PlayerType.None || ((trigger.Instigator.Type == PlayerType.Self || (trigger.Instigator.MeetsRelationConditions(entityInfo) && trigger.Instigator.MeetsGenderCondition(entityInfo) && trigger.Instigator.MeetsRaceCondition(entityInfo))) && trigger.Instigator.MeetsStatusConditions(entityInfo))))))
					{
						entityInfo2 = PlayerManager.NearbyPlayers.FirstOrDefault((EntityInfo x) => x.GameObject.GameObjectId == targetId);
						if ((!(trigger.Receiver is EmoteTargetReceiver emoteTargetReceiver) || ((emoteTargetReceiver.Type != PlayerType.All || entityInfo2 != null) && (emoteTargetReceiver.Type == PlayerType.All || ((emoteTargetReceiver.Type != PlayerType.None || entityInfo2 == null) && (emoteTargetReceiver.Type != PlayerType.Self || ((entityInfo2 != null) ? new nint?(((IGameObject)entityInfo2.Character).Address) : ((nint?)null)) == (nint?)(nint)((IGameObject)localPlayer.Character).Address) && (emoteTargetReceiver.Type != PlayerType.Others || (entityInfo2 != null && ((entityInfo2 != null) ? new nint?(((IGameObject)entityInfo2.Character).Address) : ((nint?)null)) != (nint?)(nint)((IGameObject)localPlayer.Character).Address)) && (emoteTargetReceiver.Type != PlayerType.Player || entityInfo2 != null) && (emoteTargetReceiver.Type != PlayerType.Player || entityInfo2 == null || emoteTargetReceiver.PlayerNameMatches(entityInfo2.Name)) && (emoteTargetReceiver.Type != PlayerType.Target || (localPlayer.IsTargetValid && ((entityInfo2 != null) ? new nint?(((IGameObject)entityInfo2.Character).Address) : ((nint?)null)) == (nint?)(nint)localPlayer.Target.Address)))) && (emoteTargetReceiver.Type == PlayerType.Ignore || emoteTargetReceiver.Type == PlayerType.None || ((emoteTargetReceiver.Type == PlayerType.Self || (emoteTargetReceiver.MeetsRelationConditions(entityInfo2) && emoteTargetReceiver.MeetsGenderCondition(entityInfo2) && emoteTargetReceiver.MeetsRaceCondition(entityInfo2))) && emoteTargetReceiver.MeetsStatusConditions(entityInfo2))))) && (trigger.ReactionOptions == null || !trigger.ReactionOptions.PassthroughRestrictions || (localPlayer.CanReactionInterruptCurrentState(trigger.ReactionOptions) && (!trigger.ReactionOptions.RestrictRange || ((entityInfo.IsLocalPlayer || entityInfo.IsWithinReactionAngleAndDistanceToLocalPlayer(trigger.ReactionOptions)) && (!entityInfo.IsLocalPlayer || entityInfo2 == null || entityInfo2.IsWithinReactionAngleAndDistanceToLocalPlayer(trigger.ReactionOptions)))) && trigger.ReactionOptions.MeetsTerritoryConditions())))
						{
							this.OnEmote?.Invoke(entityInfo, entityInfo2, emoteId, trigger);
							break;
						}
					}
				}
			}
		}
		HookEmote?.Original(unk, instigatorAddr, emoteId, targetId, unk2);
	}
}
