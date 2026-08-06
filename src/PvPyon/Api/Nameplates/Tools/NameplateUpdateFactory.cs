using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using PvPyon.Api.ActivityContexts;
using PvPyon.Api.Nameplates.Model;
using PvPyon.Api.Tools;
using PvPyon.Api.Tools.Strings;

namespace PvPyon.Api.Nameplates.Tools;

public static class NameplateUpdateFactory
{
	public static void ApplyNameplateChanges(NameplateChangesProps props)
	{
		foreach (NameplateElements value in Enum.GetValues(typeof(NameplateElements)))
		{
			StringUpdateFactory.ApplyStringChanges(props.Changes.GetProps(value));
		}
	}

	public static bool ApplyStatusIconWithPrio(ref int statusIcon, int newStatusIcon, StringChange stringChange, ActivityContext activityContext, StatusIconPriorizer priorizer, bool moveIconToNameplateIfPossible)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		bool? flag = null;
		BitmapFontIcon? bitmapFontIconFromStatusIcon = StatusIconFontConverter.GetBitmapFontIconFromStatusIcon((StatusIcons)statusIcon);
		if (moveIconToNameplateIfPossible && bitmapFontIconFromStatusIcon.HasValue)
		{
			IconPayload item = new IconPayload(bitmapFontIconFromStatusIcon.Value);
			stringChange.Payloads.Insert(0, (Payload)(object)item);
			flag = false;
		}
		bool valueOrDefault = flag == true;
		if (!flag.HasValue)
		{
			valueOrDefault = priorizer.IsPriorityIcon(statusIcon, activityContext);
			flag = valueOrDefault;
		}
		if (!flag.Value)
		{
			statusIcon = newStatusIcon;
		}
		return flag.Value;
	}
}
