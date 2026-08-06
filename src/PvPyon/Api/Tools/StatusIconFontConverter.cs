using Dalamud.Game.Text.SeStringHandling;
using PvPyon.Api.Nameplates.Model;

namespace PvPyon.Api.Tools;

public static class StatusIconFontConverter
{
	public static StatusIcons? GetStatusIconFromBitmapFontIcon(BitmapFontIcon fontIcon)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected I4, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		switch (fontIcon - 77)
		{
		default:
			if ((int)fontIcon == 95)
			{
				return StatusIcons.Returner;
			}
			return null;
		case 0:
			return StatusIcons.NewAdventurer;
		case 1:
			return StatusIcons.Mentor;
		case 2:
			return StatusIcons.MentorPvE;
		case 3:
			return StatusIcons.MentorCrafting;
		case 4:
			return StatusIcons.MentorPvP;
		}
	}

	public static BitmapFontIcon? GetBitmapFontIconFromStatusIcon(StatusIcons icon)
	{
		return icon switch
		{
			StatusIcons.NewAdventurer => (BitmapFontIcon)77, 
			StatusIcons.Mentor => (BitmapFontIcon)78, 
			StatusIcons.MentorPvE => (BitmapFontIcon)79, 
			StatusIcons.MentorCrafting => (BitmapFontIcon)80, 
			StatusIcons.MentorPvP => (BitmapFontIcon)81, 
			StatusIcons.Returner => (BitmapFontIcon)95, 
			_ => null, 
		};
	}
}
