using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace PvPyon.Api.ActivityContexts;

public class ActivityContextManager : IDisposable
{
	public delegate void ActivityContextChangedEventHandler(ActivityContextManager sender, ActivityContext activityContext);

	private readonly ExcelSheet<ContentFinderCondition> contentFinderConditionsSheet;

	public ActivityContext CurrentActivityContext { get; protected set; }

	public event ActivityContextChangedEventHandler ActivityContextChanged;

	public ActivityContextManager()
	{
		contentFinderConditionsSheet = PluginServices.DataManager.GameData.GetExcelSheet<ContentFinderCondition>();
		CheckCurrentTerritory();
		PluginServices.ClientState.TerritoryChanged += ClientState_TerritoryChanged;
	}

	public void Dispose()
	{
		PluginServices.ClientState.TerritoryChanged -= ClientState_TerritoryChanged;
	}

	private void ClientState_TerritoryChanged(ushort e)
	{
		CheckCurrentTerritory();
	}

	private void CheckCurrentTerritory()
	{
		ContentFinderCondition val = ((IEnumerable<ContentFinderCondition>)contentFinderConditionsSheet).FirstOrDefault((ContentFinderCondition c) => c.TerritoryType.Row == PluginServices.ClientState.TerritoryType);
		ActivityType activityType;
		ZoneType zoneType;
		if (val == null)
		{
			activityType = ActivityType.None;
			zoneType = ZoneType.Overworld;
		}
		else
		{
			activityType = ((!val.PvP) ? ActivityType.PveDuty : ActivityType.PvpDuty);
			uint num = val.ContentMemberType.Row;
			if (((ExcelRow)val).RowId == 16 || ((ExcelRow)val).RowId == 15)
			{
				num = 2u;
			}
			else if (((ExcelRow)val).RowId == 735 || ((ExcelRow)val).RowId == 778)
			{
				num = 127u;
			}
			zoneType = num switch
			{
				2u => ZoneType.Dungeon, 
				3u => ZoneType.Raid, 
				4u => ZoneType.AllianceRaid, 
				127u => ZoneType.Foray, 
				_ => ZoneType.Dungeon, 
			};
		}
		CurrentActivityContext = new ActivityContext(activityType, zoneType);
		this.ActivityContextChanged?.Invoke(this, CurrentActivityContext);
	}
}
