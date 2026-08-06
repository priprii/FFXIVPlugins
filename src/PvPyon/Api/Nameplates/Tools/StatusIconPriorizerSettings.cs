using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PvPyon.Api.Nameplates.Model;

namespace PvPyon.Api.Nameplates.Tools;

public class StatusIconPriorizerSettings
{
	[JsonProperty("IconConditionSets")]
	private Dictionary<StatusIconPriorizerConditionSets, List<StatusIcons>> iconConditionSets = new Dictionary<StatusIconPriorizerConditionSets, List<StatusIcons>>();

	public bool UsePriorizedIcons { get; set; } = true;

	[JsonConstructor]
	private StatusIconPriorizerSettings(JsonConstructorAttribute dummy)
	{
	}

	public StatusIconPriorizerSettings()
		: this(fillWithDefaultSettings: false)
	{
	}

	public StatusIconPriorizerSettings(bool fillWithDefaultSettings)
	{
		foreach (StatusIconPriorizerConditionSets value in Enum.GetValues(typeof(StatusIconPriorizerConditionSets)))
		{
			iconConditionSets.Add(value, new List<StatusIcons>());
		}
		if (fillWithDefaultSettings)
		{
			FillWithDefaultSettings();
		}
	}

	public List<StatusIcons> GetConditionSet(StatusIconPriorizerConditionSets set)
	{
		return iconConditionSets[set];
	}

	public void ResetToEmpty()
	{
		foreach (KeyValuePair<StatusIconPriorizerConditionSets, List<StatusIcons>> iconConditionSet in iconConditionSets)
		{
			iconConditionSet.Value.Clear();
		}
	}

	public void ResetToDefault()
	{
		ResetToEmpty();
		FillWithDefaultSettings();
	}

	private void FillWithDefaultSettings()
	{
		GetConditionSet(StatusIconPriorizerConditionSets.Overworld).AddRange(new StatusIcons[16]
		{
			StatusIcons.Disconnecting,
			StatusIcons.InDuty,
			StatusIcons.ViewingCutscene,
			StatusIcons.Busy,
			StatusIcons.Idle,
			StatusIcons.DutyFinder,
			StatusIcons.PartyLeader,
			StatusIcons.PartyMember,
			StatusIcons.RolePlaying,
			StatusIcons.GroupPose,
			StatusIcons.Mentor,
			StatusIcons.MentorCrafting,
			StatusIcons.MentorPvE,
			StatusIcons.MentorPvP,
			StatusIcons.Returner,
			StatusIcons.NewAdventurer
		});
		GetConditionSet(StatusIconPriorizerConditionSets.InDuty).AddRange(new StatusIcons[10]
		{
			StatusIcons.Disconnecting,
			StatusIcons.ViewingCutscene,
			StatusIcons.Idle,
			StatusIcons.GroupPose,
			StatusIcons.Mentor,
			StatusIcons.MentorCrafting,
			StatusIcons.MentorPvE,
			StatusIcons.MentorPvP,
			StatusIcons.Returner,
			StatusIcons.NewAdventurer
		});
		GetConditionSet(StatusIconPriorizerConditionSets.InForay).AddRange(new StatusIcons[11]
		{
			StatusIcons.InDuty,
			StatusIcons.Disconnecting,
			StatusIcons.ViewingCutscene,
			StatusIcons.Idle,
			StatusIcons.GroupPose,
			StatusIcons.Mentor,
			StatusIcons.MentorCrafting,
			StatusIcons.MentorPvE,
			StatusIcons.MentorPvP,
			StatusIcons.Returner,
			StatusIcons.NewAdventurer
		});
	}
}
