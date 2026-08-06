using System;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace PvPyon;

public class PluginServices
{
	[PluginService]
	public static IPluginLog PluginLog { get; set; }

	[PluginService]
	public static IGameConfig GameConfig { get; set; }

	[PluginService]
	public static IChatGui ChatGui { get; set; }

	[PluginService]
	public static IClientState ClientState { get; set; }

	[PluginService]
	public static ICommandManager CommandManager { get; set; }

	[PluginService]
	public static DalamudPluginInterface DalamudPluginInterface { get; set; }

	[PluginService]
	public static IDataManager DataManager { get; set; }

	[PluginService]
	public static IFramework Framework { get; set; }

	[PluginService]
	public static IGameGui GameGui { get; set; }

	[PluginService]
	public static IObjectTable ObjectTable { get; set; }

	[PluginService]
	public static IPartyList PartyList { get; set; }

	[PluginService]
	public static IGameInteropProvider GameInteropProvider { get; set; }

	public static void Initialize(DalamudPluginInterface pluginInterface)
	{
		pluginInterface.Create<PluginServices>(Array.Empty<object>());
	}
}
