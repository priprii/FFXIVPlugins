using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling;
using PvPyon.Api.Nameplates.EventArgs;

namespace PvPyon;

public class PlayerNameplateUpdatedArgs
{
	private readonly AddonNamePlate_SetPlayerNameManagedEventArgs eventArgs;

	public PlayerCharacter PlayerCharacter { get; }

	public SeString Name => eventArgs.Name;

	public SeString Title => eventArgs.Title;

	public SeString FreeCompany => eventArgs.FreeCompany;

	public bool IsTitleVisible
	{
		get
		{
			return eventArgs.IsTitleVisible;
		}
		set
		{
			eventArgs.IsTitleVisible = value;
		}
	}

	public bool IsTitleAboveName
	{
		get
		{
			return eventArgs.IsTitleAboveName;
		}
		set
		{
			eventArgs.IsTitleAboveName = value;
		}
	}

	public int IconId
	{
		get
		{
			return eventArgs.IconID;
		}
		set
		{
			eventArgs.IconID = value;
		}
	}

	public PlayerNameplateUpdatedArgs(PlayerCharacter playerCharacter, AddonNamePlate_SetPlayerNameManagedEventArgs eventArgs)
	{
		PlayerCharacter = playerCharacter;
		this.eventArgs = eventArgs;
	}
}
