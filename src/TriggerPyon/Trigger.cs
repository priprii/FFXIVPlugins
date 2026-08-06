using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace TriggerPyon;

[Serializable]
public class Trigger
{
	public Guid Guid { get; set; } = Guid.NewGuid();

	public string Name { get; set; } = "New Trigger";

	public string Description { get; set; } = "";

	public bool Enabled { get; set; }

	public TriggerType Type { get; set; }

	public ActionBase? ReceivedAction { get; set; }

	public Instigator? Instigator { get; set; }

	public ReceiverBase? Receiver { get; set; }

	public bool UseSharedCounter { get; set; }

	public CounterBase? Counter { get; set; }

	public ReactionOptions? ReactionOptions { get; set; }

	public List<ReactionBase>? Reactions { get; set; }

	[JsonIgnore]
	public long LastReactionTime { get; set; }

	public Counter? GetCounter()
	{
		if (UseSharedCounter)
		{
			CounterBase counter = Counter;
			SharedCounter sharedCounter = counter as SharedCounter;
			if (sharedCounter != null)
			{
				return Plugin.Config.Triggers.FirstOrDefault(delegate(Trigger x)
				{
					Guid guid = x.Guid;
					Guid? triggerGuid = sharedCounter.TriggerGuid;
					return guid == triggerGuid;
				})?.Counter as Counter;
			}
		}
		return Counter as Counter;
	}
}
