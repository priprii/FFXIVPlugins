using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace PartyRangePyon;

public class CharacterPlayerData : IPlayerData
{
	private unsafe readonly Character* character;

	public unsafe CharacterPlayerData(Character* characterPointer)
	{
		character = characterPointer;
	}

	public unsafe bool HasStatus(uint statusId)
	{
		return ((StatusManager)((Character)character).GetStatusManager()).HasStatus(statusId, 3758096384u);
	}

	public unsafe uint GetObjectId()
	{
		return ((GameObject)(&((Character)character).GameObject)).EntityId;
	}

	public unsafe string GetName()
	{
		return ((GameObject)(&((Character)character).GameObject)).NameString;
	}

	public unsafe float GetStatusTimeRemaining(uint statusId)
	{
		if (HasStatus(statusId))
		{
			int statusIndex = ((StatusManager)((Character)character).GetStatusManager()).GetStatusIndex(statusId, 3758096384u);
			return ((StatusManager)((Character)character).GetStatusManager()).GetRemainingTime(statusIndex);
		}
		return 0f;
	}

	public unsafe byte GetLevel()
	{
		return ((CharacterData)(&((Character)character).CharacterData)).Level;
	}

	public unsafe byte GetClassJob()
	{
		return ((CharacterData)(&((Character)character).CharacterData)).ClassJob;
	}

	public unsafe bool HasClassJob(uint classJobId)
	{
		return ((CharacterData)(&((Character)character).CharacterData)).ClassJob == classJobId;
	}

	public unsafe bool IsDead()
	{
		return ((CharacterData)(&((Character)character).CharacterData)).Health == 0;
	}
}
