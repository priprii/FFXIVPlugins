using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace Ktisis.Editor.Animation.Game;

public class GameAnimationData(IDataManager data)
{
	private readonly List<GameAnimation> Animations = new List<GameAnimation>();

	private ExcelSheet<ActionTimeline>? Timelines;

	public int Count
	{
		get
		{
			lock (Animations)
			{
				return Animations.Count;
			}
		}
	}

	public IEnumerable<GameAnimation> GetAll()
	{
		lock (Animations)
		{
			return Animations.AsReadOnly();
		}
	}

	public async Task Build()
	{
		await Task.Yield();
		FetchEmotes();
		FetchActions();
		FetchTimelines();
	}

	public ActionTimeline? GetTimelineById(uint id)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return Timelines?.GetRow(id);
	}

	private void FetchEmotes()
	{
		IEnumerable<EmoteAnimation> collection = ((IEnumerable<Emote>)data.GetExcelSheet<Emote>((ClientLanguage?)null, (string)null)).Where(delegate(Emote emote)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			ReadOnlySeString name = ((Emote)(ref emote)).Name;
			return !((ReadOnlySeString)(ref name)).IsEmpty;
		}).SelectMany(MapEmotes).DistinctBy((EmoteAnimation emote) => (Name: emote.Name, Slot: emote.Slot));
		lock (Animations)
		{
			Animations.AddRange(collection);
		}
		static IEnumerable<EmoteAnimation> MapEmotes(Emote emote)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < ((Emote)(ref emote)).ActionTimeline.Count; i++)
			{
				RowRef<ActionTimeline> val = ((Emote)(ref emote)).ActionTimeline[i];
				if (val.IsValid && val.RowId != 0)
				{
					yield return new EmoteAnimation(emote, i);
				}
			}
		}
	}

	private void FetchActions()
	{
		IEnumerable<ActionAnimation> collection = from action in ((IEnumerable<Action>)data.GetExcelSheet<Action>((ClientLanguage?)null, (string)null)).Where(delegate(Action action)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				ReadOnlySeString name = ((Action)(ref action)).Name;
				return !((ReadOnlySeString)(ref name)).IsEmpty;
			}).DistinctBy(delegate(Action action)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				ReadOnlySeString name = ((Action)(ref action)).Name;
				return (((ReadOnlySeString)(ref name)).ExtractText(), Icon: ((Action)(ref action)).Icon, RowId: ((Action)(ref action)).AnimationStart.RowId);
			})
			select new ActionAnimation(action);
		lock (Animations)
		{
			Animations.AddRange(collection);
		}
	}

	private void FetchTimelines()
	{
		if (Timelines == null)
		{
			Timelines = data.GetExcelSheet<ActionTimeline>((ClientLanguage?)null, (string)null);
		}
		IEnumerable<TimelineAnimation> collection = from timeline in ((IEnumerable<ActionTimeline>)Timelines).Where(delegate(ActionTimeline timeline)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				ReadOnlySeString key = ((ActionTimeline)(ref timeline)).Key;
				return !((ReadOnlySeString)(ref key)).IsEmpty;
			})
			select new TimelineAnimation(timeline);
		lock (Animations)
		{
			Animations.AddRange(collection);
		}
	}
}
