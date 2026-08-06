namespace TriggerPyon;

public class TextReaction : ReactionBase
{
	public override TriggerType ObjType => TriggerType.Text;

	public string Template { get; set; } = string.Empty;

	public bool SameChannel { get; set; } = true;

	public ChatType Channel { get; set; }

	public bool CopyInstigator { get; set; }
}
