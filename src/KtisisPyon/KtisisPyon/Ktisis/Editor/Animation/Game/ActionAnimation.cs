using Ktisis.Editor.Animation.Types;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace Ktisis.Editor.Animation.Game;

public class ActionAnimation(Action action) : GameAnimation
{
	public override string Name
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			ReadOnlySeString name = ((Action)(ref action)).Name;
			return ((ReadOnlySeString)(ref name)).ExtractText();
		}
	}

	public override uint Icon => ((Action)(ref action)).Icon;

	public override uint TimelineId
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			if (!((Action)(ref action)).AnimationEnd.IsValid)
			{
				return 0u;
			}
			return ((Action)(ref action)).AnimationEnd.RowId;
		}
	}

	public override TimelineSlot Slot
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			if (!((Action)(ref action)).AnimationEnd.IsValid)
			{
				return TimelineSlot.FullBody;
			}
			ActionTimeline value = ((Action)(ref action)).AnimationEnd.Value;
			return (TimelineSlot)((ActionTimeline)(ref value)).Stance;
		}
	}
}
