namespace TriggerPyon;

public class Emote(ushort id, string name, string? command, bool isPose = false, bool triggersEmoteHook = true)
{
	public ushort ID { get; set; } = id;

	public string Name { get; set; } = name;

	public string? Command { get; set; } = command;

	public bool IsPose { get; set; } = isPose;

	public bool TriggersEmoteHook { get; set; } = triggersEmoteHook;

	public override string ToString()
	{
		if (string.IsNullOrWhiteSpace(Name))
		{
			return $"#{ID}";
		}
		return $"#{ID} {Name}";
	}
}
