namespace TriggerPyon;

public class SpecialEmote(ushort id, string name, bool triggersEmoteHook)
{
	public ushort ID { get; set; } = id;

	public string Name { get; set; } = name;

	public bool TriggersEmoteHook { get; set; } = triggersEmoteHook;
}
