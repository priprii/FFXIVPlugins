using System;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;

namespace PvPyon;

public static class PlayerContextHelper
{
	public static PlayerContext GetPlayerContext(PlayerCharacter playerCharacter)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		PlayerContext playerContext = PlayerContext.None;
		if ((GameObject)(object)PluginServices.ClientState.LocalPlayer == (GameObject)(object)playerCharacter)
		{
			playerContext |= PlayerContext.Self;
		}
		if (((Enum)((Character)playerCharacter).StatusFlags).HasFlag((Enum)(object)(StatusFlags)64))
		{
			playerContext |= PlayerContext.Friend;
		}
		if (((Enum)((Character)playerCharacter).StatusFlags).HasFlag((Enum)(object)(StatusFlags)16))
		{
			playerContext |= PlayerContext.Party;
		}
		if (((Enum)((Character)playerCharacter).StatusFlags).HasFlag((Enum)(object)(StatusFlags)32))
		{
			playerContext |= PlayerContext.Alliance;
		}
		if (((Enum)((Character)playerCharacter).StatusFlags).HasFlag((Enum)(object)(StatusFlags)1))
		{
			playerContext |= PlayerContext.Enemy;
		}
		return playerContext;
	}

	public static bool GetIsVisible(PlayerContext playerContext, bool desiredSelfVisibility, bool desiredFriendsVisibility, bool desiredPartyVisibility, bool desiredAllianceVisibility, bool desiredEnemiesVisibility, bool desiredOthersVisibility)
	{
		if (playerContext.HasFlag(PlayerContext.Self))
		{
			return desiredSelfVisibility;
		}
		bool flag = false;
		if (playerContext.HasFlag(PlayerContext.Friend))
		{
			flag = flag || desiredFriendsVisibility;
		}
		if (playerContext.HasFlag(PlayerContext.Party))
		{
			flag = flag || desiredPartyVisibility;
		}
		if (!playerContext.HasFlag(PlayerContext.Party) && playerContext.HasFlag(PlayerContext.Alliance))
		{
			flag = flag || desiredAllianceVisibility;
		}
		if (playerContext.HasFlag(PlayerContext.Enemy))
		{
			flag = flag || desiredEnemiesVisibility;
		}
		if (playerContext == PlayerContext.None)
		{
			flag = flag || desiredOthersVisibility;
		}
		return flag;
	}

	public static bool GetIsVisible(PlayerCharacter playerCharacter, bool desiredSelfVisibility, bool desiredFriendsVisibility, bool desiredPartyVisibility, bool desiredAllianceVisibility, bool desiredEnemiesVisibility, bool desiredOthersVisibility)
	{
		return GetIsVisible(GetPlayerContext(playerCharacter), desiredSelfVisibility, desiredFriendsVisibility, desiredPartyVisibility, desiredAllianceVisibility, desiredEnemiesVisibility, desiredOthersVisibility);
	}
}
