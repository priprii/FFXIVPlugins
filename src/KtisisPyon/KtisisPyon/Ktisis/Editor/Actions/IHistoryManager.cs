using System.Collections.Generic;
using Ktisis.Actions.Types;

namespace Ktisis.Editor.Actions;

public interface IHistoryManager
{
	int Count { get; }

	bool CanUndo { get; }

	bool CanRedo { get; }

	void Add(IMemento item);

	void Clear();

	IEnumerable<IMemento> GetTimeline();

	void Undo();

	void Redo();
}
