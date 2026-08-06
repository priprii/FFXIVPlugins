using System.Collections.Generic;
using System.Linq;

namespace TriggerPyon;

public class ReactionOptions
{
	public bool PassthroughRestrictions { get; set; }

	public bool CountFailedConditions { get; set; }

	public int ReactionCooldown { get; set; }

	public ReactionInterruptType InterruptType { get; set; } = ReactionInterruptType.Any;

	public StateConditionType StateConditions { get; set; }

	public RestoreType RestoreType { get; set; }

	public bool RestrictRange { get; set; }

	public float RestrictedDistanceMin { get; set; }

	public float RestrictedDistanceMax { get; set; } = 2f;

	public int RestrictedAngleDirection { get; set; }

	public float RestrictedAngleArea { get; set; } = 1f;

	public bool RestrictTerritory { get; set; }

	public List<Territory> AllowedTerritories { get; set; } = new List<Territory>();

	public bool MeetsTerritoryConditions()
	{
		if (!RestrictTerritory)
		{
			return true;
		}
		if (AllowedTerritories.Count == 0)
		{
			return false;
		}
		Territory terMatch = AllowedTerritories.FirstOrDefault((Territory x) => x.Id == Plugin.ClientState.TerritoryType && x.Id != 0);
		if (terMatch == null)
		{
			return false;
		}
		if (terMatch.Ward != 0 || terMatch.Plot != 0 || terMatch.Room != 0)
		{
			ResidentialTerritory residentialTerritory = Plugin.ResidentialTerritories.FirstOrDefault((ResidentialTerritory x) => x.Id == terMatch.Id);
			if (residentialTerritory == null)
			{
				return false;
			}
			switch (residentialTerritory.ResidentialType)
			{
			case ResidentialType.Ward:
				if (terMatch.Ward != 0 && terMatch.Ward != PlayerManager.CurrentWard)
				{
					return false;
				}
				if (terMatch.Plot != 0 && terMatch.Plot != PlayerManager.CurrentPlot)
				{
					return false;
				}
				break;
			case ResidentialType.House:
				if (terMatch.Ward != 0 && terMatch.Ward != PlayerManager.CurrentWard)
				{
					return false;
				}
				if (terMatch.Plot != 0 && terMatch.Plot != PlayerManager.CurrentPlot)
				{
					return false;
				}
				break;
			case ResidentialType.Chambers:
				if (terMatch.Ward != 0 && terMatch.Ward != PlayerManager.CurrentWard)
				{
					return false;
				}
				if (terMatch.Plot != 0 && terMatch.Plot != PlayerManager.CurrentPlot)
				{
					return false;
				}
				if (terMatch.Room != 0 && terMatch.Room != PlayerManager.CurrentRoom)
				{
					return false;
				}
				break;
			case ResidentialType.Apartment:
				if (terMatch.Ward != 0 && terMatch.Ward != PlayerManager.CurrentWard)
				{
					return false;
				}
				if (terMatch.Room != 0 && terMatch.Room != PlayerManager.CurrentRoom)
				{
					return false;
				}
				break;
			case ResidentialType.ApartmentLobby:
				if (terMatch.Ward != 0 && terMatch.Ward != PlayerManager.CurrentWard)
				{
					return false;
				}
				break;
			}
		}
		return true;
	}
}
