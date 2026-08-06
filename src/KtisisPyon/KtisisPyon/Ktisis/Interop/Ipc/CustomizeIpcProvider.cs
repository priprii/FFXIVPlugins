using System;
using System.Collections.Generic;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace Ktisis.Interop.Ipc;

public class CustomizeIpcProvider
{
	private readonly ICallGateSubscriber<(int, int)> _getApiVersion;

	private readonly ICallGateSubscriber<IList<(Guid UniqueId, string Name, string VirtualPath, List<(string Name, ushort WorldId, byte CharacterType, ushort CharacterSubType)> Characters, int Priority, bool IsEnabled)>> _getProfileList;

	private readonly ICallGateSubscriber<ushort, (int, Guid?)> _getActiveProfileId;

	private readonly ICallGateSubscriber<Guid, (int, string?)> _getProfileByUId;

	private readonly ICallGateSubscriber<ushort, string, (int, Guid?)> _setTemporaryProfile;

	private readonly ICallGateSubscriber<ushort, int> _unsetTemporaryProfile;

	private readonly ICallGateSubscriber<int, int, int> _setCsParentIndex;

	private readonly ICallGateSubscriber<Guid, int> _unsetTemporaryProfileGuid;

	public CustomizeIpcProvider(IDalamudPluginInterface dpi)
	{
		_getApiVersion = dpi.GetIpcSubscriber<(int, int)>("CustomizePlus.General.GetApiVersion");
		_getProfileList = dpi.GetIpcSubscriber<IList<(Guid, string, string, List<(string, ushort, byte, ushort)>, int, bool)>>("CustomizePlus.Profile.GetList");
		_getActiveProfileId = dpi.GetIpcSubscriber<ushort, (int, Guid?)>("CustomizePlus.Profile.GetActiveProfileIdOnCharacter");
		_getProfileByUId = dpi.GetIpcSubscriber<Guid, (int, string)>("CustomizePlus.Profile.GetByUniqueId");
		_setTemporaryProfile = dpi.GetIpcSubscriber<ushort, string, (int, Guid?)>("CustomizePlus.Profile.SetTemporaryProfileOnCharacter");
		_unsetTemporaryProfile = dpi.GetIpcSubscriber<ushort, int>("CustomizePlus.Profile.DeleteTemporaryProfileOnCharacter");
		_setCsParentIndex = dpi.GetIpcSubscriber<int, int, int>("CustomizePlus.GameState.SetCutsceneParentIndex");
		_unsetTemporaryProfileGuid = dpi.GetIpcSubscriber<Guid, int>("CustomizePlus.Profile.DeleteTemporaryProfileByUniqueId");
	}

	public bool IsCompatible()
	{
		(int, int) tuple = _getApiVersion.InvokeFunc();
		var (num, _) = tuple;
		if (num <= 5)
		{
			if (num == 5 && tuple.Item2 >= 1)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public IList<(Guid UniqueId, string Name, string VirtualPath, List<(string Name, ushort WorldId, byte CharacterType, ushort CharacterSubType)> Characters, int Priority, bool IsEnabled)> GetProfileList()
	{
		return _getProfileList.InvokeFunc();
	}

	public (int, Guid? Id) GetActiveProfileId(ushort gameObjectIndex)
	{
		return _getActiveProfileId.InvokeFunc(gameObjectIndex);
	}

	public (int, string? Data) GetProfileByUniqueId(Guid id)
	{
		return _getProfileByUId.InvokeFunc(id);
	}

	public (int, Guid? Id) SetTemporaryProfile(ushort gameObjectIndex, string profileJson)
	{
		return _setTemporaryProfile.InvokeFunc(gameObjectIndex, profileJson);
	}

	public int DeleteTemporaryProfile(ushort gameObjectIndex)
	{
		return _unsetTemporaryProfile.InvokeFunc(gameObjectIndex);
	}

	public int DeleteTemporaryProfileGuid(Guid id)
	{
		return _unsetTemporaryProfileGuid.InvokeFunc(id);
	}

	public int SetCutsceneParentIndex(int copyIndex, int newParentIndex)
	{
		return _setCsParentIndex.InvokeFunc(copyIndex, newParentIndex);
	}
}
