using PyonPix.Shared.Structs.Territory;

namespace PyonPix.Shared.Structs.Pix.Properties;

public class SyncedTerritoryPixProperties : ISynced<TerritoryPixProperties>
{
	public short WorldId { get; set; }

	public short TerritoryId { get; set; }

	public short Ward { get; set; }

	public short Plot { get; set; }

	public short Room { get; set; }

	public short Floor { get; set; }

	public bool Persistent { get; set; }

	public void ApplyTo(TerritoryPixProperties target)
	{
		target.WorldId = (ushort)WorldId;
		target.TerritoryId = (uint)TerritoryId;
		target.Ward = Ward;
		target.Plot = Plot;
		target.Room = Room;
		target.Floor = (Floor)Floor;
		target.Persistent = Persistent;
	}
}
