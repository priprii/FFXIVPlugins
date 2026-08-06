using System;
using PyonPix.Shared.Sync.Dto.Client;

namespace PyonPix.Shared.Structs.Territory;

public class TerritoryData : IEquatable<TerritoryData>
{
	public ushort WorldId;

	public uint TerritoryId;

	public short Ward;

	public short Plot;

	public short Room;

	public Floor Floor;

	public string WorldName = string.Empty;

	public string TerritoryName = string.Empty;

	public string TerritorySubName = string.Empty;

	public uint RawTerritoryId;

	public TerritoryData()
	{
	}

	public TerritoryData(TerritoryData other)
	{
		WorldId = other.WorldId;
		TerritoryId = other.TerritoryId;
		Ward = other.Ward;
		Plot = other.Plot;
		Room = other.Room;
		Floor = other.Floor;
		WorldName = other.WorldName;
		TerritoryName = other.TerritoryName;
		TerritorySubName = other.TerritorySubName;
		RawTerritoryId = other.RawTerritoryId;
	}

	public override string ToString()
	{
		return $"{WorldId}:{TerritoryId}:{Ward}:{Plot}:{Room}:{(ushort)Floor}";
	}

	public TerritoryDto ToDto()
	{
		return new TerritoryDto((short)WorldId, (short)TerritoryId, Ward, Plot, Room);
	}

	public static TerritoryData Parse(string value)
	{
		string[] array = value.Split(':');
		return new TerritoryData
		{
			WorldId = (ushort)((array.Length >= 1) ? ushort.Parse(array[0]) : 0),
			TerritoryId = ((array.Length >= 2) ? uint.Parse(array[1]) : 0u),
			Ward = (short)((array.Length >= 3) ? short.Parse(array[2]) : 0),
			Plot = (short)((array.Length >= 4) ? short.Parse(array[3]) : 0),
			Room = (short)((array.Length >= 5) ? short.Parse(array[4]) : 0),
			Floor = (Floor)((array.Length >= 6) ? ushort.Parse(array[5]) : 0)
		};
	}

	public bool Matches(TerritoryData? other, bool persistent)
	{
		if (other == null)
		{
			return false;
		}
		if (WorldId != other.WorldId)
		{
			return false;
		}
		if (TerritoryId != other.TerritoryId)
		{
			return false;
		}
		if (Ward != other.Ward)
		{
			return false;
		}
		if (Room != other.Room)
		{
			return false;
		}
		if (Floor != Floor.None)
		{
			if (Plot != other.Plot)
			{
				return false;
			}
			if (!persistent && Floor != other.Floor)
			{
				return false;
			}
		}
		else
		{
			if (!persistent && Plot != other.Plot)
			{
				return false;
			}
			if (Floor != other.Floor)
			{
				return false;
			}
		}
		return true;
	}

	public bool MatchesWTWP(TerritoryData? other)
	{
		if (other == null)
		{
			return false;
		}
		if (WorldId != other.WorldId)
		{
			return false;
		}
		if (TerritoryId != other.TerritoryId)
		{
			return false;
		}
		if (Ward != other.Ward)
		{
			return false;
		}
		if (Plot != other.Plot)
		{
			return false;
		}
		return true;
	}

	public bool Equals(TerritoryData? other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (WorldId == other.WorldId && TerritoryId == other.TerritoryId && Ward == other.Ward && Plot == other.Plot && Room == other.Room)
		{
			return Floor == other.Floor;
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as TerritoryData);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(WorldId, TerritoryId, Ward, Plot, Room, (ushort)Floor);
	}

	public static bool operator ==(TerritoryData? left, TerritoryData? right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(TerritoryData? left, TerritoryData? right)
	{
		return !object.Equals(left, right);
	}
}
