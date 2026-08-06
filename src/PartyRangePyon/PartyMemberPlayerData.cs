using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Group;

namespace PartyRangePyon;

public class PartyMemberPlayerData : IPlayerData
{
	private unsafe readonly PartyMember* partyMember;

	public unsafe PartyMemberPlayerData(PartyMember* partyMemberPointer)
	{
		partyMember = partyMemberPointer;
	}

	public unsafe bool HasStatus(uint statusId)
	{
		return ((StatusManager)(&((PartyMember)partyMember).StatusManager)).HasStatus(statusId, 3758096384u);
	}

	public unsafe uint GetObjectId()
	{
		return ((PartyMember)partyMember).EntityId;
	}

	public unsafe string GetName()
	{
		return ((PartyMember)partyMember).NameString;
	}

	public unsafe float GetStatusTimeRemaining(uint statusId)
	{
		if (HasStatus(statusId))
		{
			int statusIndex = ((StatusManager)(&((PartyMember)partyMember).StatusManager)).GetStatusIndex(statusId, 3758096384u);
			return ((StatusManager)(&((PartyMember)partyMember).StatusManager)).GetRemainingTime(statusIndex);
		}
		return 0f;
	}

	public unsafe byte GetLevel()
	{
		return ((PartyMember)partyMember).Level;
	}

	public unsafe bool HasClassJob(uint classJobId)
	{
		return ((PartyMember)partyMember).ClassJob == classJobId;
	}

	public unsafe bool IsDead()
	{
		return ((PartyMember)partyMember).CurrentHP == 0;
	}

	public unsafe byte GetClassJob()
	{
		return ((PartyMember)partyMember).ClassJob;
	}
}
