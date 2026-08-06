using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PvPyon.Api.Nameplates.Model;

[JsonConverter(typeof(StringEnumConverter))]
public enum StatusIcons
{
	Disconnecting = 61503,
	InDuty = 61506,
	ViewingCutscene = 61508,
	Busy = 61509,
	Idle = 61511,
	DutyFinder = 61517,
	PartyLeader = 61521,
	PartyMember = 61522,
	RolePlaying = 61545,
	GroupPose = 61546,
	NewAdventurer = 61523,
	Mentor = 61540,
	MentorPvE = 61542,
	MentorCrafting = 61543,
	MentorPvP = 61544,
	Returner = 61547
}
