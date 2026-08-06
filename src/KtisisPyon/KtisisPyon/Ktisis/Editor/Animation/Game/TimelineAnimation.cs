using Ktisis.Editor.Animation.Types;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace Ktisis.Editor.Animation.Game;

public class TimelineAnimation(ActionTimeline timeline) : GameAnimation
{
	public override string Name
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			ReadOnlySeString key = ((ActionTimeline)(ref timeline)).Key;
			return ((ReadOnlySeString)(ref key)).ExtractText();
		}
	}

	public override uint Icon => 0u;

	public override uint TimelineId => ((ActionTimeline)(ref timeline)).RowId;

	public override TimelineSlot Slot => (TimelineSlot)((ActionTimeline)(ref timeline)).Stance;
}
