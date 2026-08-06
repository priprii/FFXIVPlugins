using System.Collections.Generic;
using Ktisis.Actions.Types;

namespace Ktisis.Editor.Posing.Data;

public class MultipleMemento(IReadOnlyList<IMemento?> mementos) : IMemento
{
	public IReadOnlyList<IMemento?> Mementos => mementos;

	public void Restore()
	{
		for (int num = mementos.Count - 1; num >= 0; num--)
		{
			mementos[num]?.Restore();
		}
	}

	public void Apply()
	{
		for (int i = 0; i < mementos.Count; i++)
		{
			mementos[i]?.Apply();
		}
	}
}
