namespace PvPyon.Api.Nameplates.EventArgs;

public class AddonNamePlate_SetPlayerNameEventArgs : HookWithResultBaseEventArgs<nint>
{
	public nint PlayerNameplateObjectPtr { get; set; }

	public nint TitlePtr { get; set; }

	public nint NamePtr { get; set; }

	public nint FreeCompanyPtr { get; set; }

	public bool IsTitleAboveName { get; set; }

	public bool IsTitleVisible { get; set; }

	public int IconID { get; set; }
}
