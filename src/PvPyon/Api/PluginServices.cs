using System;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace PvPyon.Api;

public class PluginServices
{
	[PluginService]
	public static IGameGui GameGui { get; set; }

	[PluginService]
	public static DalamudPluginInterface PluginInterface { get; set; }

	[PluginService]
	public static IClientState ClientState { get; set; }

	[PluginService]
	public static IDataManager DataManager { get; set; }

	[PluginService]
	public static IObjectTable ObjectTable { get; set; }

	[PluginService]
	public static IGameInteropProvider GameInteropProvider { get; set; }

	public static void Initialize(DalamudPluginInterface dalamudPluginInterface)
	{
		dalamudPluginInterface.Create<PluginServices>(Array.Empty<object>());
	}
}
