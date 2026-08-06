namespace TriggerPyon;

public class EmoteReaction : ReactionBase
{
	public override TriggerType ObjType => TriggerType.Emote;

	public bool PerformEmote { get; set; } = true;

	public ushort ID { get; set; }

	public DurationType DurationType { get; set; }

	public bool CopyInstigator { get; set; }

	public ReactionTargetType TargetType { get; set; }

	public ReactionLookAtType LookAtType { get; set; }
}
