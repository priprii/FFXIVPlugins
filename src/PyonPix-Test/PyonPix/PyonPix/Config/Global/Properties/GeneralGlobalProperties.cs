using System.Numerics;
using PyonPix.Utility;

namespace PyonPix.Config.Global.Properties;

public class GeneralGlobalProperties
{
	public int PixSpawnLimit = 5;

	public Vector4 AccentBg = UiUtil.RGBA(230, 150, 230, 250f);

	public Vector4 AccentTitle = UiUtil.RGBA(180, 140, 220, 240f);

	public Vector4 AccentHovered = UiUtil.RGBA(150, 110, 190, 240f);

	public Vector4 AccentActive = UiUtil.RGBA(180, 140, 220, 240f);
}
