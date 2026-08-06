using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;

namespace TriggerPyon;

public class Instigator
{
	public Dictionary<StatusType, TriState> Status = new Dictionary<StatusType, TriState>();

	public PlayerType Type { get; set; } = PlayerType.Others;

	public bool RequireNearby { get; set; } = true;

	public PlayerCondition Condition { get; set; }

	public bool RequireAllConditions { get; set; }

	public GenderCondition Gender { get; set; }

	public RaceCondition Race { get; set; }

	public List<string> Names { get; set; } = new List<string>();

	public List<string> BlacklistNames { get; set; } = new List<string>();

	public bool PlayerNameMatches(string playerName)
	{
		if (Names.Count != 0)
		{
			return Names.Any((string x) => playerName.Equals(x, StringComparison.OrdinalIgnoreCase));
		}
		return false;
	}

	public bool BlacklistNameMatches(string playerName)
	{
		if (BlacklistNames.Count != 0)
		{
			return BlacklistNames.Any((string x) => playerName.Equals(x, StringComparison.OrdinalIgnoreCase));
		}
		return false;
	}

	public bool MeetsRelationConditions(EntityInfo? entity)
	{
		if (Condition == PlayerCondition.None)
		{
			return true;
		}
		if (entity == null || entity.Character == null)
		{
			return false;
		}
		if (entity.IsBlocked)
		{
			return false;
		}
		if (Type != PlayerType.Self)
		{
			if (Type == PlayerType.All)
			{
				IntPtr address = ((IGameObject)entity.Character).Address;
				EntityInfo? localPlayer = PlayerManager.LocalPlayer;
				if ((IntPtr?)(nint)address == (IntPtr?)(nint)((localPlayer != null) ? new nint?(((IGameObject)localPlayer.Character).Address) : ((nint?)null)))
				{
					goto IL_009d;
				}
			}
			(PlayerCondition, Func<EntityInfo, bool>)[] source = new(PlayerCondition, Func<EntityInfo, bool>)[3]
			{
				(PlayerCondition.Friend, (EntityInfo e) => e.IsFriend),
				(PlayerCondition.Party, (EntityInfo e) => e.IsInParty),
				(PlayerCondition.MareSynced, (EntityInfo e) => e.IsMareSynced)
			}.Where(((PlayerCondition Flag, Func<EntityInfo, bool> Check) c) => Condition.HasFlag(c.Flag)).ToArray();
			if (!RequireAllConditions)
			{
				return source.Any(((PlayerCondition Flag, Func<EntityInfo, bool> Check) c) => c.Check(entity));
			}
			return source.All(((PlayerCondition Flag, Func<EntityInfo, bool> Check) c) => c.Check(entity));
		}
		goto IL_009d;
		IL_009d:
		return true;
	}

	public bool MeetsGenderCondition(EntityInfo? entity)
	{
		if (Gender == GenderCondition.Any)
		{
			return true;
		}
		if (entity == null || entity.Character == null)
		{
			return false;
		}
		if (Type != PlayerType.Self)
		{
			if (Type == PlayerType.All)
			{
				IntPtr address = ((IGameObject)entity.Character).Address;
				EntityInfo? localPlayer = PlayerManager.LocalPlayer;
				if ((IntPtr?)(nint)address == (IntPtr?)(nint)((localPlayer != null) ? new nint?(((IGameObject)localPlayer.Character).Address) : ((nint?)null)))
				{
					goto IL_006b;
				}
			}
			if (entity.Gender != TriggerPyon.Gender.Male)
			{
				return Gender.HasFlag(GenderCondition.Female);
			}
			return Gender.HasFlag(GenderCondition.Male);
		}
		goto IL_006b;
		IL_006b:
		return true;
	}

	public bool MeetsRaceCondition(EntityInfo? entity)
	{
		if (Race == RaceCondition.Any)
		{
			return true;
		}
		if (entity == null || entity.Character == null)
		{
			return false;
		}
		if (Type != PlayerType.Self)
		{
			if (Type == PlayerType.All)
			{
				IntPtr address = ((IGameObject)entity.Character).Address;
				EntityInfo? localPlayer = PlayerManager.LocalPlayer;
				if ((IntPtr?)(nint)address == (IntPtr?)(nint)((localPlayer != null) ? new nint?(((IGameObject)localPlayer.Character).Address) : ((nint?)null)))
				{
					goto IL_008e;
				}
			}
			return new(RaceCondition, Func<EntityInfo, bool>)[9]
			{
				(RaceCondition.Midlander, (EntityInfo e) => e.Race == TriggerPyon.Race.Midlander),
				(RaceCondition.Highlander, (EntityInfo e) => e.Race == TriggerPyon.Race.Highlander),
				(RaceCondition.Elezen, (EntityInfo e) => e.Race == TriggerPyon.Race.Elezen),
				(RaceCondition.Miqote, (EntityInfo e) => e.Race == TriggerPyon.Race.Miqote),
				(RaceCondition.Roegadyn, (EntityInfo e) => e.Race == TriggerPyon.Race.Roegadyn),
				(RaceCondition.Lalafell, (EntityInfo e) => e.Race == TriggerPyon.Race.Lalafell),
				(RaceCondition.AuRa, (EntityInfo e) => e.Race == TriggerPyon.Race.AuRa),
				(RaceCondition.Hrothgar, (EntityInfo e) => e.Race == TriggerPyon.Race.Hrothgar),
				(RaceCondition.Viera, (EntityInfo e) => e.Race == TriggerPyon.Race.Viera)
			}.Where(((RaceCondition Flag, Func<EntityInfo, bool> Check) c) => Race.HasFlag(c.Flag)).ToArray().Any(((RaceCondition Flag, Func<EntityInfo, bool> Check) c) => c.Check(entity));
		}
		goto IL_008e;
		IL_008e:
		return true;
	}

	public bool MeetsStatusConditions(EntityInfo? entity)
	{
		if (Status == null || Status.Count == 0)
		{
			return true;
		}
		if (entity == null || entity.Character == null)
		{
			return false;
		}
		bool flag = false;
		foreach (KeyValuePair<StatusType, TriState> item in Status)
		{
			if (item.Value != TriState.Ignored)
			{
				bool flag2 = ((item.Key == StatusType.InCombat) ? entity.InCombat : entity.Character.HasOnlineStatus((OnlineStatusTypeRaw)item.Key));
				if (item.Value == TriState.Disallow && flag2)
				{
					return false;
				}
				if (item.Value == TriState.Allow && flag2)
				{
					flag = true;
				}
			}
		}
		return !Status.Values.Any((TriState v) => v == TriState.Allow) || flag;
	}
}
