using System;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace Ktisis.Interop.Ipc;

public class MareIpcProvider
{
	private readonly ICallGateSubscriber<string, IGameObject, bool> _mareApplyMcdf;

	public MareIpcProvider(IDalamudPluginInterface dpi)
	{
		_mareApplyMcdf = dpi.GetIpcSubscriber<string, IGameObject, bool>("MareSynchronos.LoadMcdf");
	}

	public bool LoadMcdfAsync(string fileName, IGameObject target)
	{
		try
		{
			return _mareApplyMcdf.InvokeFunc(fileName, target);
		}
		catch (Exception exception)
		{
			Ktisis.Log.Error(exception, "Failed to Invoke MareSynchronos.LoadMcdfAsync IPC");
			return false;
		}
	}
}
