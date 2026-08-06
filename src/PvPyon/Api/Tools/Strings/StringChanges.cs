using System.Collections.Generic;
using System.Linq;

namespace PvPyon.Api.Tools.Strings;

public class StringChanges
{
	private readonly Dictionary<StringPosition, StringChange> changes = new Dictionary<StringPosition, StringChange>();

	public StringChanges()
	{
		changes.Add(StringPosition.Before, new StringChange());
		changes.Add(StringPosition.After, new StringChange());
		changes.Add(StringPosition.Replace, new StringChange());
	}

	public StringChange GetChange(StringPosition position)
	{
		return changes[position];
	}

	public bool Any()
	{
		return changes.Sum<KeyValuePair<StringPosition, StringChange>>((KeyValuePair<StringPosition, StringChange> n) => n.Value.Payloads.Count) != 0;
	}
}
