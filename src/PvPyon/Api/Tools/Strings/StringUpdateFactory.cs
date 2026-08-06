using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace PvPyon.Api.Tools.Strings;

public static class StringUpdateFactory
{
	public static void ApplyStringChanges(StringChangesProps props)
	{
		if (props.StringChanges == null || !props.StringChanges.Any())
		{
			return;
		}
		SeString destination = props.Destination;
		foreach (StringPosition orderedStringPosition in GetOrderedStringPositions(props))
		{
			StringChange change = props.StringChanges.GetChange(orderedStringPosition);
			if (change == null || !change.Payloads.Any())
			{
				continue;
			}
			AddSpacesBetweenTextPayloads(change.Payloads, orderedStringPosition);
			switch (orderedStringPosition)
			{
			case StringPosition.Before:
			{
				Payload val2 = (Payload)(change.ForceUsingSingleAnchorPayload ? ((object)props.AnchorPayload) : ((object)props.AnchorPayloads?.FirstOrDefault()));
				if (val2 != null)
				{
					int index2 = destination.Payloads.IndexOf(val2);
					destination.Payloads.InsertRange(index2, change.Payloads);
				}
				else
				{
					destination.Payloads.InsertRange(0, change.Payloads);
				}
				break;
			}
			case StringPosition.After:
			{
				Payload val = (Payload)(change.ForceUsingSingleAnchorPayload ? ((object)props.AnchorPayload) : ((object)props.AnchorPayloads?.LastOrDefault()));
				if (val != null)
				{
					int num = destination.Payloads.IndexOf(val);
					destination.Payloads.InsertRange(num + 1, change.Payloads);
				}
				else
				{
					destination.Payloads.AddRange(change.Payloads);
				}
				break;
			}
			case StringPosition.Replace:
			{
				Payload anchorPayload = props.AnchorPayload;
				if (anchorPayload != null)
				{
					int index = destination.Payloads.IndexOf(anchorPayload);
					destination.Payloads.InsertRange(index, change.Payloads);
					destination.Remove(anchorPayload);
				}
				else
				{
					destination.Payloads.Clear();
					destination.Payloads.AddRange(change.Payloads);
				}
				break;
			}
			}
		}
	}

	private static void AddSpacesBetweenTextPayloads(List<Payload> payloads, StringPosition tagPosition)
	{
		if (payloads == null || !payloads.Any())
		{
			return;
		}
		List<int> list = new List<int>();
		int num = -1;
		foreach (Payload item in Enumerable.Reverse(payloads))
		{
			if (item is IconPayload)
			{
				num = -1;
				continue;
			}
			TextPayload val = (TextPayload)(object)((item is TextPayload) ? item : null);
			if (val != null)
			{
				if (num != -1)
				{
					list.Add(payloads.IndexOf((Payload)(object)val) + 1);
				}
				num = payloads.IndexOf((Payload)(object)val);
			}
		}
		foreach (int item2 in list)
		{
			payloads.Insert(item2, (Payload)(object)getNewTextPayload());
		}
		switch (tagPosition)
		{
		case StringPosition.Before:
			if (payloads.Where((Payload payload) => payload is TextPayload || payload is IconPayload).Last() is TextPayload)
			{
				payloads.Add((Payload)(object)getNewTextPayload());
			}
			break;
		case StringPosition.After:
			if (payloads.Where((Payload payload) => payload is TextPayload || payload is IconPayload).First() is TextPayload)
			{
				payloads.Insert(0, (Payload)(object)getNewTextPayload());
			}
			break;
		}
		static TextPayload getNewTextPayload()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			return new TextPayload(" ");
		}
	}

	private static List<StringPosition> GetOrderedStringPositions(StringChangesProps props)
	{
		List<StringPosition> list = new List<StringPosition>();
		if (props.AnchorPayloads == null || !props.AnchorPayloads.Any())
		{
			list.Add(StringPosition.Replace);
		}
		list.Add(StringPosition.Before);
		list.Add(StringPosition.After);
		if (props.AnchorPayloads != null && props.AnchorPayloads.Any())
		{
			list.Add(StringPosition.Replace);
		}
		return list;
	}
}
