using PyonPix.Shared.Structs.Territory;

namespace PyonPix.Shared.Structs.Pix.Properties;

public class TerritoryPixProperties : ILocal<SyncedTerritoryPixProperties>
{
	public ushort WorldId;

	public uint TerritoryId;

	public short Ward;

	public short Plot;

	public short Room;

	public Floor Floor;

	public bool Persistent = true;

	public TerritoryPixProperties()
	{
	}

	public TerritoryPixProperties(ushort worldId, uint territoryId, short ward, short plot, short room, Floor floor)
	{
		WorldId = worldId;
		TerritoryId = territoryId;
		Ward = ward;
		Plot = plot;
		Room = room;
		Floor = floor;
	}

	public override string ToString()
	{
		return $"{WorldId}:{TerritoryId}:{Ward}:{Plot}:{Room}:{(ushort)Floor}";
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

	public SyncedTerritoryPixProperties ToSynced()
	{
		return new SyncedTerritoryPixProperties
		{
			WorldId = (short)WorldId,
			TerritoryId = (short)TerritoryId,
			Ward = Ward,
			Plot = Plot,
			Room = Room,
			Floor = (short)Floor,
			Persistent = Persistent
		};
	}
}
