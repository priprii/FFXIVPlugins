using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace PartyRangePyon;

public interface IPlayerData
{
	bool HasStatus(uint statusId);

	uint GetObjectId();

	byte GetLevel();

	byte GetClassJob();

	bool HasClassJob(uint classJobId);

	bool IsDead();

	string GetName();

	float GetStatusTimeRemaining(uint statusId);

	bool HasStatus(params uint[] statuses)
	{
		return statuses.Any(HasStatus);
	}

	bool MissingStatus(uint statusID)
	{
		return !HasStatus(statusID);
	}

	bool MissingStatus(params uint[] statuses)
	{
		return !statuses.Any(HasStatus);
	}

	bool HasClassJob(params uint[] classJobs)
	{
		return classJobs.Any(HasClassJob);
	}

	bool MissingClassJob(uint classJobId)
	{
		return !HasClassJob(classJobId);
	}

	bool MissingClassJob(params uint[] classJobs)
	{
		return !HasClassJob(classJobs);
	}

	bool GameObjectHasStatus(params uint[] statuses)
	{
		return statuses.Any(GameObjectHasStatus);
	}

	bool IsTargetable()
	{
		IGameObject? gameObject = GetGameObject();
		if (gameObject == null)
		{
			return false;
		}
		return gameObject.IsTargetable;
	}

	unsafe bool GameObjectHasStatus(uint statusId)
	{
		Character* characterGameObject = GetCharacterGameObject();
		if (characterGameObject == null)
		{
			return false;
		}
		return ((StatusManager)((Character)characterGameObject).GetStatusManager()).HasStatus(statusId, 3758096384u);
	}

	bool HasPet()
	{
		return (from obj in (IEnumerable<IGameObject>)Plugin.ObjectTable
			where obj.OwnerId == GetObjectId()
			where obj is IBattleNpc && obj.SubKind == 2
			select obj).Any();
	}

	protected IGameObject? GetGameObject()
	{
		return Plugin.ObjectTable.SearchById((ulong)GetObjectId());
	}

	protected unsafe Character* GetCharacterGameObject()
	{
		IGameObject gameObject = GetGameObject();
		if (gameObject == null)
		{
			return null;
		}
		return (Character*)gameObject.Address;
	}
}
