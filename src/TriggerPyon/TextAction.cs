using System;
using System.Collections.Generic;
using System.Linq;

namespace TriggerPyon;

public class TextAction : ActionBase
{
	public override TriggerType ObjType => TriggerType.Text;

	public List<string> Inputs { get; set; } = new List<string>();

	public bool MatchAll { get; set; }

	public bool CaseSensitive { get; set; }

	public bool MessageContainsInputs(string message)
	{
		if (Inputs.Count != 0)
		{
			if (!MatchAll)
			{
				return Inputs.Any((string x) => message.Contains(x, CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));
			}
			return Inputs.All((string x) => message.Contains(x, CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));
		}
		return false;
	}
}
