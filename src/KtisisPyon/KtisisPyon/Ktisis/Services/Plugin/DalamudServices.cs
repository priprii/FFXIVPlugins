using System;
using Dalamud.Interface;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Ktisis.Services.Plugin;

public sealed class DalamudServices
{
	[PluginService]
	private IChatGui ChatGui { get; set; }

	[PluginService]
	private IClientState ClientState { get; set; }

	[PluginService]
	private ICommandManager Cmd { get; set; }

	[PluginService]
	private IFramework Framework { get; set; }

	[PluginService]
	private IGameGui Gui { get; set; }

	[PluginService]
	private IGameInteropProvider Interop { get; set; }

	[PluginService]
	private IObjectTable ObjectTable { get; set; }

	[PluginService]
	private IKeyState KeyState { get; set; }

	[PluginService]
	private IDataManager Data { get; set; }

	[PluginService]
	private ITextureProvider Tex { get; set; }

	[PluginService]
	private ISigScanner SigScanner { get; set; }

	[PluginService]
	private ITargetManager TargetManager { get; set; }

	public void Add(IDalamudPluginInterface dpi, IServiceCollection services)
	{
		dpi.Inject((object)this, Array.Empty<object>());
		services.AddSingleton<IDalamudPluginInterface>(dpi).AddSingleton<IUiBuilder>(dpi.UiBuilder).AddSingleton<IClientState>(ClientState)
			.AddSingleton<ICommandManager>(Cmd)
			.AddSingleton<IFramework>(Framework)
			.AddSingleton<IGameGui>(Gui)
			.AddSingleton<IGameInteropProvider>(Interop)
			.AddSingleton<IObjectTable>(ObjectTable)
			.AddSingleton<IKeyState>(KeyState)
			.AddSingleton<IDataManager>(Data)
			.AddSingleton<ITextureProvider>(Tex)
			.AddSingleton<ISigScanner>(SigScanner)
			.AddSingleton<IChatGui>(ChatGui)
			.AddSingleton<ITargetManager>(TargetManager);
	}
}
