using System.Collections.Generic;
using System.Drawing;

namespace Ktisis.Data.Config.Sections;

public class PyonConfig
{
	public int DefaultStyle;

	public Point DefaultPosition;

	public Size DefaultSize;

	public Size DefaultDeviceSize;

	public Size HiResSize;

	public List<Size> Resolutions = new List<Size>();
}
