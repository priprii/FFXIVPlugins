using Ktisis.Editor.Animation.Types;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace Ktisis.Editor.Animation.Game;

public class EmoteAnimation(Emote emote, int index = 0) : GameAnimation
{
	public override string Name
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			ReadOnlySeString name = ((Emote)(ref emote)).Name;
			return ((ReadOnlySeString)(ref name)).ExtractText();
		}
	}

	public override uint Icon => ((Emote)(ref emote)).Icon;

	public override uint TimelineId => Timeline.RowId;

	public override TimelineSlot Slot
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			if (!Timeline.IsValid)
			{
				return TimelineSlot.FullBody;
			}
			ActionTimeline value = Timeline.Value;
			return (TimelineSlot)((ActionTimeline)(ref value)).Stance;
		}
	}

	public int Index => index;

	public uint EmoteId => ((Emote)(ref emote)).RowId;

	public bool IsExpression => ((Emote)(ref emote)).EmoteCategory.RowId == 3;

	private RowRef<ActionTimeline> Timeline => ((Emote)(ref emote)).ActionTimeline[index];
}
