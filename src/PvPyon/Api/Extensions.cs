using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;

namespace PvPyon.Api;

public static class Extensions
{
	public static void Remove(this SeString seString, Payload payload)
	{
		Remove(seString.Payloads, payload);
	}

	public static void Remove(this List<Payload> payloads, Payload payload)
	{
		for (int i = 0; i < payloads.Count; i++)
		{
			if (payloads[i] == payload)
			{
				payloads.RemoveAt(i);
				break;
			}
		}
	}
}
