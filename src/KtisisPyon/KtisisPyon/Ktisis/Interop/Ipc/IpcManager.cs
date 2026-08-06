using System.Linq;
using Dalamud.Plugin;
using Ktisis.Core.Attributes;

namespace Ktisis.Interop.Ipc;

[Singleton]
public class IpcManager
{
	private readonly IDalamudPluginInterface _dpi;

	public bool IsAnyMcdfActive
	{
		get
		{
			if (!IsPenumbraActive && !IsCustomizeActive)
			{
				return IsGlamourerActive;
			}
			return true;
		}
	}

	public bool IsPenumbraActive => GetPluginInstalled("Penumbra");

	public bool IsCustomizeActive => GetPluginInstalled("CustomizePlus");

	public bool IsGlamourerActive => GetPluginInstalled("Glamourer");

	public bool IsBrioActive => GetPluginInstalled("Brio");

	public IpcManager(IDalamudPluginInterface dpi)
	{
		_dpi = dpi;
	}

	public PenumbraIpcProvider GetPenumbraIpc()
	{
		return new PenumbraIpcProvider(_dpi);
	}

	public CustomizeIpcProvider GetCustomizeIpc()
	{
		return new CustomizeIpcProvider(_dpi);
	}

	public GlamourerIpcProvider GetGlamourerIpc()
	{
		return new GlamourerIpcProvider(_dpi);
	}

	private bool GetPluginInstalled(string internalName)
	{
		return _dpi.InstalledPlugins.Any((IExposedPlugin p) => p.IsLoaded && (p.InternalName == internalName || p.Name == internalName));
	}
}
