using System.Collections.Generic;
using System.Numerics;

namespace PyonPix.Config.UI.Properties;

public class MainUIProperties
{
	public bool IsOpen;

	public bool Collapsed;

	public Vector2 ExpandedSize;

	public HashSet<string> ExpandedTerritories = new HashSet<string>();
}
