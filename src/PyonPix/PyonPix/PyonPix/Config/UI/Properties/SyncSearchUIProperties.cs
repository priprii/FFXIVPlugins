using System.Collections.Generic;
using System.Numerics;
using PyonPix.Shared.Structs.Pix;

namespace PyonPix.Config.UI.Properties;

public class SyncSearchUIProperties
{
	public bool IsOpen;

	public bool Collapsed;

	public Vector2 ExpandedSize;

	public bool ShowNsfw = true;

	public bool SameTerritoryOnly;

	public HashSet<PixType> TypeFilters = new HashSet<PixType>();

	public int RegionActiveTabIndex;

	public HashSet<ushort> WorldFilters = new HashSet<ushort>();
}
