using System.Collections.Generic;
using System.Linq;

namespace TriggerPyon;

public class ChannelTextReceiver : ReceiverBase
{
	public Dictionary<StatusType, TriState> Status = new Dictionary<StatusType, TriState>();

	public override TriggerType ObjType => TriggerType.Text;

	public bool MatchAny { get; set; }

	public ChatType Channel { get; set; }

	public bool MeetsChannelCondition(ChatType channel)
	{
		if (!MatchAny)
		{
			if (Channel != ChatType.None)
			{
				return Channel.HasFlag(channel);
			}
			return false;
		}
		return true;
	}

	public bool MeetsStatusConditions()
	{
		if (Status == null || Status.Count == 0)
		{
			return true;
		}
		if (PlayerManager.LocalPlayer == null || PlayerManager.LocalPlayer.Character == null)
		{
			return false;
		}
		bool flag = false;
		foreach (KeyValuePair<StatusType, TriState> item in Status)
		{
			if (item.Value != TriState.Ignored)
			{
				bool flag2 = ((item.Key == StatusType.InCombat) ? PlayerManager.LocalPlayer.InCombat : PlayerManager.LocalPlayer.Character.HasOnlineStatus((OnlineStatusTypeRaw)item.Key));
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
