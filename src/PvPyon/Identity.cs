using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PvPyon;

public class Identity : IComparable<Identity>, IEquatable<Identity>
{
	public string Name { get; init; }

	public uint? WorldId { get; set; }

	public List<Guid> CustomTagIds { get; init; } = new List<Guid>();

	[JsonIgnore]
	public string? WorldName => WorldHelper.GetWorldName(WorldId);

	public Identity(string name)
	{
		Name = name;
	}

	public override string ToString()
	{
		string text = Name;
		if (WorldId.HasValue)
		{
			text = text + "@" + WorldName;
		}
		return text;
	}

	public int CompareTo(Identity? other)
	{
		string strB = null;
		if ((object)other != null)
		{
			strB = other.ToString();
		}
		return ToString().CompareTo(strB);
	}

	public override bool Equals(object? obj)
	{
		if (obj is Identity obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public bool Equals(Identity? obj)
	{
		if ((object)obj == null)
		{
			return false;
		}
		return this == obj;
	}

	public static bool operator ==(Identity? first, Identity? second)
	{
		if ((object)first == second)
		{
			return true;
		}
		if ((object)first == null && (object)second == null)
		{
			return true;
		}
		if ((object)first == null || (object)second == null)
		{
			return false;
		}
		bool num = first.Name.ToLower().Trim() == second.Name.ToLower().Trim();
		bool flag = !first.WorldId.HasValue || !second.WorldId.HasValue || first.WorldId == second.WorldId;
		return num && flag;
	}

	public static bool operator !=(Identity? first, Identity? second)
	{
		return !(first == second);
	}

	public override int GetHashCode()
	{
		int num = Name.GetHashCode();
		if (WorldName != null)
		{
			num *= 0x11 ^ WorldName.GetHashCode();
		}
		return num;
	}
}
