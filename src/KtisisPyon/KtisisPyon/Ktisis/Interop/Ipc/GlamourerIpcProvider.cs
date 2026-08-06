using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using Glamourer.Api.Enums;
using Glamourer.Api.IpcSubscribers;

namespace Ktisis.Interop.Ipc;

public class GlamourerIpcProvider
{
	private readonly GetDesignList _getDesignList;

	private readonly ApplyDesign _applyDesign;

	private readonly ApplyState _applyState;

	private readonly RevertState _revertState;

	private readonly RevertStateName _revertStateName;

	private readonly UnlockState _unlockState;

	private readonly UnlockStateName _unlockStateName;

	private readonly UnlockAll _unlockAll;

	private readonly DeletePlayerState _deleteState;

	private readonly GetStateBase64 _getState;

	private readonly uint Key = 128007u;

	public GlamourerIpcProvider(IDalamudPluginInterface dpi)
	{
		_getDesignList = new GetDesignList(dpi);
		_applyState = new ApplyState(dpi);
		_revertState = new RevertState(dpi);
		_revertStateName = new RevertStateName(dpi);
		_applyDesign = new ApplyDesign(dpi);
		_unlockState = new UnlockState(dpi);
		_unlockStateName = new UnlockStateName(dpi);
		_unlockAll = new UnlockAll(dpi);
		_deleteState = new DeletePlayerState(dpi);
		_getState = new GetStateBase64(dpi);
	}

	public Dictionary<Guid, string> GetDesignList()
	{
		return _getDesignList.Invoke();
	}

	public bool ApplyDesignToObject(IGameObject gameObject, Guid designId)
	{
		Ktisis.Log.Debug($"Setting design for '{gameObject.Name}' ({gameObject.ObjectIndex}) to '{designId}'");
		GlamourerApiEc glamourerApiEc = _applyDesign.Invoke(designId, gameObject.ObjectIndex, 0u, ApplyFlag.Once | ApplyFlag.Equipment | ApplyFlag.Customization);
		bool num = glamourerApiEc == GlamourerApiEc.Success;
		if (!num)
		{
			Ktisis.Log.Warning($"Glamourer design application failed with return code: {glamourerApiEc}");
		}
		return num;
	}

	public bool RevertObject(IGameObject gameObject)
	{
		Ktisis.Log.Debug($"Reverting state for '{gameObject.Name}' ({gameObject.ObjectIndex})");
		GlamourerApiEc glamourerApiEc = RevertStateName(gameObject.Name.TextValue);
		if (glamourerApiEc != GlamourerApiEc.Success)
		{
			Ktisis.Log.Warning($"Glamourer revert failed with return code: {glamourerApiEc}, trying by index...");
			glamourerApiEc = RevertState(gameObject.ObjectIndex);
		}
		UnlockObject(gameObject);
		return glamourerApiEc == GlamourerApiEc.Success;
	}

	public bool DeleteState(IGameObject gameObject, IPlayerCharacter? localPlayer)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (localPlayer == null)
		{
			return false;
		}
		Ktisis.Log.Debug($"Deleting state for '{gameObject.Name}' ({gameObject.ObjectIndex})");
		GlamourerApiEc glamourerApiEc = _deleteState.Invoke(gameObject.Name.TextValue, (ushort)localPlayer.CurrentWorld.RowId, Key);
		if (glamourerApiEc != GlamourerApiEc.Success)
		{
			UnlockObject(gameObject);
			Ktisis.Log.Warning($"Glamourer delete failed with return code: {glamourerApiEc}!");
		}
		return glamourerApiEc == GlamourerApiEc.Success;
	}

	public void CopyState(int sourceIndex, int targetIndex)
	{
		(GlamourerApiEc, string) tuple = _getState.Invoke(sourceIndex);
		if (tuple.Item1 == GlamourerApiEc.Success && tuple.Item2 != null)
		{
			_applyState.Invoke(tuple.Item2, targetIndex, 0u, ApplyFlag.Equipment | ApplyFlag.Customization | ApplyFlag.Lock);
		}
	}

	public void ApplyState(string state, int index)
	{
		_applyState.Invoke(state, index, Key, ApplyFlag.Equipment | ApplyFlag.Customization | ApplyFlag.Lock);
	}

	public void Unlock()
	{
		_unlockAll.Invoke(Key);
	}

	private void UnlockObject(IGameObject gameObject)
	{
		Ktisis.Log.Debug($"Unlocking for '{gameObject.Name}' ({gameObject.ObjectIndex})");
		if (_unlockStateName.Invoke(gameObject.Name.TextValue, Key) != GlamourerApiEc.Success)
		{
			_unlockState.Invoke(gameObject.ObjectIndex, Key);
		}
	}

	private GlamourerApiEc RevertState(int index)
	{
		return _revertState.Invoke(index, Key, ApplyFlag.Equipment | ApplyFlag.Customization | ApplyFlag.Lock);
	}

	private GlamourerApiEc RevertStateName(string playerName)
	{
		return _revertStateName.Invoke(playerName, Key, ApplyFlag.Equipment | ApplyFlag.Customization | ApplyFlag.Lock);
	}
}
