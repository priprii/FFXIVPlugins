using System.Collections.Generic;
using System.Linq;
using Ktisis.Actions.Types;

namespace Ktisis.Editor.Actions;

public class HistoryManager : IHistoryManager
{
	private const int TimelineMax = 100;

	private readonly List<IMemento> Timeline = new List<IMemento>();

	private int Cursor;

	public int Count => Timeline.Count;

	public bool CanUndo => Cursor > 0;

	public bool CanRedo => Cursor < Timeline.Count;

	public void Add(IMemento item)
	{
		int num = Timeline.Count();
		if (Cursor < num)
		{
			Ktisis.Log.Verbose($"If history must be unwritten, let it be unwritten. ({Cursor} <- {num})");
			Timeline.RemoveRange(Cursor, num - Cursor);
		}
		Timeline.Add(item);
		Cursor++;
	}

	public void Clear()
	{
		Timeline.Clear();
		Cursor = 0;
	}

	public IEnumerable<IMemento> GetTimeline()
	{
		return Timeline;
	}

	public void Undo()
	{
		if (CanUndo)
		{
			Ktisis.Log.Info("Undoing");
			Cursor--;
			Timeline[Cursor].Restore();
		}
	}

	public void Redo()
	{
		if (CanRedo)
		{
			Ktisis.Log.Info("Redoing");
			Timeline[Cursor].Apply();
			Cursor++;
		}
	}
}
