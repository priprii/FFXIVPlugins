using System;
using System.Linq;
using Dalamud.Game.Chat;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace TriggerPyon;

public class Chat
{
	private Plugin plugin;

	public event Action<string, string, EntityInfo?, ChatType, Trigger>? OnChat;

	internal Chat(Plugin plugin)
	{
		this.plugin = plugin;
	}

	internal void OnChatMessage(IHandleableChatMessage m)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		OnChatMessage(((IChatMessage)m).LogKind, ((IChatMessage)m).Timestamp, ((IMutableChatMessage)m).Sender, ((IMutableChatMessage)m).Message, ((IChatMessage)m).IsHandled);
	}

	internal void OnChatMessage(XivChatType type, int timestamp, SeString sender, SeString message, bool isHandled)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		ChatType chatType = ConvertXIVChatTypeToChatType(type);
		EntityInfo localPlayer = PlayerManager.LocalPlayer;
		if (!Plugin.Config.Enabled || localPlayer == null || chatType == ChatType.None)
		{
			return;
		}
		string text;
		if ((int)type == 12)
		{
			text = localPlayer.Name;
		}
		else
		{
			bool flag = ((type - 14 <= 1 || (int)type == 32) ? true : false);
			text = ((flag && sender.TextValue.Length > 0) ? sender.TextValue.Substring(1) : sender.TextValue);
		}
		string messageSender = text;
		messageSender = ((messageSender.Length <= 1) ? messageSender : (char.IsLetter(messageSender[0]) ? messageSender : messageSender.Substring(1)));
		string textValue = message.TextValue;
		EntityInfo entityInfo = null;
		foreach (Trigger trigger in Plugin.Config.Triggers)
		{
			if (!trigger.Enabled || trigger.Type != TriggerType.Text)
			{
				continue;
			}
			Instigator? instigator = trigger.Instigator;
			if (instigator != null && instigator.Type == PlayerType.None)
			{
				continue;
			}
			Instigator? instigator2 = trigger.Instigator;
			if ((instigator2 != null && instigator2.Type == PlayerType.Ignore) || !(trigger.ReceivedAction is TextAction textAction) || !(trigger.Receiver is ChannelTextReceiver channelTextReceiver) || !channelTextReceiver.MeetsChannelCondition(chatType) || !channelTextReceiver.MeetsStatusConditions() || !textAction.MessageContainsInputs(textValue))
			{
				continue;
			}
			if (trigger.Instigator != null)
			{
				entityInfo = PlayerManager.NearbyPlayers.FirstOrDefault((EntityInfo x) => x.Character != null && messageSender.StartsWith(((IGameObject)x.Character).Name.TextValue, StringComparison.OrdinalIgnoreCase));
				if ((trigger.Instigator.RequireNearby && entityInfo == null) || (trigger.Instigator.Type != PlayerType.All && ((trigger.Instigator.Type == PlayerType.Self && !messageSender.StartsWith(localPlayer.Name, StringComparison.OrdinalIgnoreCase)) || (trigger.Instigator.Type == PlayerType.Others && messageSender.StartsWith(localPlayer.Name, StringComparison.OrdinalIgnoreCase)) || (trigger.Instigator.Type == PlayerType.Player && !trigger.Instigator.PlayerNameMatches(messageSender)) || (trigger.Instigator.Type == PlayerType.Target && (entityInfo == null || !localPlayer.IsTargetValid || localPlayer.Target.Address != ((IGameObject)entityInfo.Character).Address)) || (trigger.Instigator.Type == PlayerType.Targeter && (entityInfo == null || !entityInfo.IsTargetValid || entityInfo.Target.Address != ((IGameObject)localPlayer.Character).Address)))) || (trigger.Instigator.Type != PlayerType.Self && trigger.Instigator.BlacklistNameMatches(messageSender)) || (entityInfo != null && ((trigger.Instigator.Type != PlayerType.Self && (!trigger.Instigator.MeetsRelationConditions(entityInfo) || !trigger.Instigator.MeetsGenderCondition(entityInfo) || !trigger.Instigator.MeetsRaceCondition(entityInfo))) || !trigger.Instigator.MeetsStatusConditions(entityInfo))))
				{
					continue;
				}
			}
			if (trigger.ReactionOptions == null || !trigger.ReactionOptions.PassthroughRestrictions || (localPlayer.CanReactionInterruptCurrentState(trigger.ReactionOptions) && (!trigger.ReactionOptions.RestrictRange || entityInfo == null || entityInfo.IsLocalPlayer || entityInfo.IsWithinReactionAngleAndDistanceToLocalPlayer(trigger.ReactionOptions)) && trigger.ReactionOptions.MeetsTerritoryConditions()))
			{
				this.OnChat?.Invoke(messageSender, textValue, entityInfo, chatType, trigger);
				break;
			}
		}
	}

	private ChatType ConvertXIVChatTypeToChatType(XivChatType channel)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected I4, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Invalid comparison between Unknown and I4
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected I4, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected I4, but got Unknown
		if ((int)channel <= 32)
		{
			switch (channel - 10)
			{
			case 0:
				return ChatType.Say;
			case 1:
				return ChatType.Shout;
			case 4:
				return ChatType.Party;
			case 5:
				return ChatType.Alliance;
			case 2:
				return ChatType.Tell;
			case 3:
				return ChatType.Tell;
			}
			switch (channel - 24)
			{
			case 5:
				return ChatType.Emote;
			case 4:
				return ChatType.CustomEmote;
			case 6:
				return ChatType.Yell;
			case 8:
				return ChatType.Party;
			case 0:
				return ChatType.FC;
			}
		}
		else
		{
			if ((int)channel == 37)
			{
				return ChatType.CWLS1;
			}
			if ((int)channel == 56)
			{
				return ChatType.Echo;
			}
			switch (channel - 101)
			{
			case 0:
				return ChatType.CWLS2;
			case 1:
				return ChatType.CWLS3;
			case 2:
				return ChatType.CWLS4;
			case 3:
				return ChatType.CWLS5;
			case 4:
				return ChatType.CWLS6;
			case 5:
				return ChatType.CWLS7;
			case 6:
				return ChatType.CWLS8;
			}
		}
		return ChatType.None;
	}

	private string GetCommandPrefixForChannel(ChatType channel)
	{
		return channel switch
		{
			ChatType.Command => "", 
			ChatType.Emote => "", 
			ChatType.CustomEmote => "/em ", 
			ChatType.Echo => "/echo ", 
			ChatType.Say => "/say ", 
			ChatType.Yell => "/yell ", 
			ChatType.Shout => "/shout ", 
			ChatType.Party => "/p ", 
			ChatType.Alliance => "/a ", 
			ChatType.FC => "/fc ", 
			ChatType.Tell => "/tell ", 
			ChatType.CWLS1 => "/cwl1 ", 
			ChatType.CWLS2 => "/cwl2 ", 
			ChatType.CWLS3 => "/cwl3 ", 
			ChatType.CWLS4 => "/cwl4 ", 
			ChatType.CWLS5 => "/cwl5 ", 
			ChatType.CWLS6 => "/cwl6 ", 
			ChatType.CWLS7 => "/cwl7 ", 
			ChatType.CWLS8 => "/cwl8 ", 
			_ => "/echo ", 
		};
	}

	public unsafe void SendMessage(ChatType channel, string message, string targetForename = "", string targetSurname = "", string? targetWorld = "")
	{
		string commandPrefixForChannel = GetCommandPrefixForChannel(channel);
		string msg = "";
		if (channel == ChatType.Tell)
		{
			string value = targetForename + " " + targetSurname;
			string value2 = (string.IsNullOrWhiteSpace(targetWorld) ? string.Empty : ("@" + targetWorld));
			msg = $"{commandPrefixForChannel}{value}{value2} {message}";
		}
		else
		{
			msg = commandPrefixForChannel + message;
		}
		Plugin.Framework.RunOnFrameworkThread((Action)delegate
		{
			((UIModule)UIModule.Instance()).ProcessChatBoxEntry(Utf8String.FromString(msg), (IntPtr)0, false);
		});
	}

	public unsafe void SendEcho(string message)
	{
		Plugin.Framework.RunOnFrameworkThread((Action)delegate
		{
			((UIModule)UIModule.Instance()).ProcessChatBoxEntry(Utf8String.FromString("/echo " + message), (IntPtr)0, false);
		});
	}
}
