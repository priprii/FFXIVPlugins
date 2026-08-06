using System;
using System.Collections.Generic;

namespace SoundPyon;

[Serializable]
public class FilterGroup
{
	public Guid Guid = Guid.NewGuid();

	public string Name = "New Group";

	public bool Enabled = true;

	public List<string> Globs { get; set; } = new List<string>();
}
