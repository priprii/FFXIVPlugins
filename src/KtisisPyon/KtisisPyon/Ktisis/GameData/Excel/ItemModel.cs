namespace Ktisis.GameData.Excel;

public class ItemModel(ulong var, bool isWep = false)
{
	public ushort Id = (ushort)var;

	public ushort Base = (ushort)(isWep ? (var >> 16) : 0);

	public ushort Variant = (ushort)(isWep ? (var >> 32) : (var >> 16));

	public bool Matches(ushort id, ushort variant)
	{
		if (Id == id)
		{
			return Variant == variant;
		}
		return false;
	}

	public bool Matches(ushort id, ushort secondId, ushort variant)
	{
		if (Id == id && Base == secondId)
		{
			return Variant == variant;
		}
		return false;
	}
}
