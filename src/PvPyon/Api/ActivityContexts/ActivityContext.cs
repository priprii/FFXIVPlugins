namespace PvPyon.Api.ActivityContexts;

public class ActivityContext
{
	public ActivityType ActivityType { get; init; }

	public ZoneType ZoneType { get; init; }

	public bool IsInDuty => ZoneType != ZoneType.Overworld;

	public ActivityContext(ActivityType activityType, ZoneType zoneType)
	{
		ActivityType = activityType;
		ZoneType = zoneType;
	}
}
