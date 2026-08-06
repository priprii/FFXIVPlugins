using PvPyon.Api.ActivityContexts;

namespace PvPyon;

public static class ActivityContextHelper
{
	public static bool GetIsVisible(ActivityType playerContext, bool desiredPveDutyVisibility, bool desiredPvpDutyVisibility, bool desiredOthersVisibility)
	{
		bool flag = false;
		if (playerContext.HasFlag(ActivityType.PveDuty))
		{
			flag = flag || desiredPveDutyVisibility;
		}
		if (playerContext.HasFlag(ActivityType.PvpDuty))
		{
			flag = flag || desiredPvpDutyVisibility;
		}
		if (playerContext == ActivityType.None)
		{
			flag = flag || desiredOthersVisibility;
		}
		return flag;
	}
}
