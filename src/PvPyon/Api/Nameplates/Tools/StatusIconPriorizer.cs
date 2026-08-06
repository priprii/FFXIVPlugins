using System.Collections.Generic;
using System.Linq;
using PvPyon.Api.ActivityContexts;
using PvPyon.Api.Nameplates.Model;

namespace PvPyon.Api.Nameplates.Tools;

public class StatusIconPriorizer
{
	private static StatusIconPriorizerSettings DefaultSettings { get; } = new StatusIconPriorizerSettings();

	public StatusIconPriorizerSettings Settings { get; init; }

	public StatusIconPriorizer()
		: this(DefaultSettings)
	{
	}

	public StatusIconPriorizer(StatusIconPriorizerSettings settings)
	{
		Settings = settings;
	}

	public bool IsPriorityIcon(int iconId, ActivityContext activityContext)
	{
		if (!Settings.UsePriorizedIcons && iconId != 61503 && iconId != 61553)
		{
			return false;
		}
		IEnumerable<int> priorityIcons = GetPriorityIcons(activityContext);
		return priorityIcons.Contains(iconId) || priorityIcons.Contains(iconId + 50);
	}

	private IEnumerable<int> GetPriorityIcons(ActivityContext activityContext)
	{
		StatusIconPriorizerConditionSets set = ((activityContext.ZoneType == ZoneType.Foray) ? StatusIconPriorizerConditionSets.InForay : (activityContext.IsInDuty ? StatusIconPriorizerConditionSets.InDuty : StatusIconPriorizerConditionSets.Overworld));
		return from n in Settings.GetConditionSet(set)
			select (int)n;
	}
}
