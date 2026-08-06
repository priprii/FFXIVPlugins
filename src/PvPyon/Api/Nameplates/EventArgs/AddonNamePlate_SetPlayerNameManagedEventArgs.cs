using Dalamud.Game.Text.SeStringHandling;
using PvPyon.Api.Nameplates.Model;

namespace PvPyon.Api.Nameplates.EventArgs;

public class AddonNamePlate_SetPlayerNameManagedEventArgs : HookWithResultManagedBaseEventArgs<nint>
{
	public new AddonNamePlate_SetPlayerNameEventArgs OriginalEventArgs
	{
		get
		{
			return base.OriginalEventArgs as AddonNamePlate_SetPlayerNameEventArgs;
		}
		set
		{
			base.OriginalEventArgs = value;
		}
	}

	public SafeNameplateObject SafeNameplateObject { get; set; }

	public SeString Title { get; set; }

	public SeString Name { get; set; }

	public SeString FreeCompany { get; set; }

	public bool IsTitleAboveName
	{
		get
		{
			return OriginalEventArgs.IsTitleAboveName;
		}
		set
		{
			OriginalEventArgs.IsTitleAboveName = value;
		}
	}

	public bool IsTitleVisible
	{
		get
		{
			return OriginalEventArgs.IsTitleVisible;
		}
		set
		{
			OriginalEventArgs.IsTitleVisible = value;
		}
	}

	public int IconID
	{
		get
		{
			return OriginalEventArgs.IconID;
		}
		set
		{
			OriginalEventArgs.IconID = value;
		}
	}
}
