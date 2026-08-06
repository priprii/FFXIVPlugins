using System;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using PvPyon.Api.Nameplates;
using PvPyon.Api.Nameplates.EventArgs;

namespace PvPyon;

public class Nameplate : IDisposable
{
	public NameplateManager NameplateManager { get; init; }

	public bool IsValid
	{
		get
		{
			if (NameplateManager != null)
			{
				return NameplateManager.IsValid;
			}
			return false;
		}
	}

	public event PlayerNameplateUpdatedDelegate? PlayerNameplateUpdated;

	public Nameplate()
	{
		NameplateManager = new NameplateManager();
		NameplateManager.Hooks.AddonNamePlate_SetPlayerNameManaged += Hooks_AddonNamePlate_SetPlayerNameManaged;
	}

	public void Dispose()
	{
		NameplateManager.Hooks.AddonNamePlate_SetPlayerNameManaged -= Hooks_AddonNamePlate_SetPlayerNameManaged;
		NameplateManager.Dispose();
	}

	private void Hooks_AddonNamePlate_SetPlayerNameManaged(AddonNamePlate_SetPlayerNameManagedEventArgs eventArgs)
	{
		try
		{
			PlayerCharacter nameplateGameObject = global::PvPyon.Api.Nameplates.NameplateManager.GetNameplateGameObject<PlayerCharacter>(eventArgs.SafeNameplateObject);
			if ((GameObject)(object)nameplateGameObject != (GameObject)null)
			{
				PlayerNameplateUpdatedArgs args = new PlayerNameplateUpdatedArgs(nameplateGameObject, eventArgs);
				this.PlayerNameplateUpdated?.Invoke(args);
			}
		}
		catch (Exception ex)
		{
			PluginServices.PluginLog.Error(ex, "SetPlayerNameplateDetour", Array.Empty<object>());
		}
	}
}
