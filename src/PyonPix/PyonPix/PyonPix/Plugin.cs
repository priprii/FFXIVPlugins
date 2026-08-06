using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin;
using PyonPix.Config;
using PyonPix.Services;
using PyonPix.Ui;

namespace PyonPix;

public class Plugin : IAsyncDalamudPlugin, IAsyncDisposable
{
	public const string Name = "PyonPix";

	private readonly Configuration Config;

	private readonly ServiceContext Services;

	private readonly WindowContext Windows;

	public static Version Version { get; private set; }

	public Plugin(IDalamudPluginInterface pi)
	{
		Services = new ServiceContext(pi);
		Windows = new WindowContext();
		Version = Services.PluginInterface.Manifest.AssemblyVersion;
		Config = (Services.PluginInterface.GetPluginConfig() as Configuration) ?? new Configuration();
		Config.Initialize(Services.PluginInterface);
	}

	public async Task LoadAsync(CancellationToken cancellationToken)
	{
		await Services.Initialize(Config, Windows);
		UIShared.Initialize(Config, Services);
		Windows.Initialize(Config, Services);
		Services.InitializeUpdate();
	}

	public async ValueTask DisposeAsync()
	{
		await Services.Dispose();
		Windows.Dispose();
		UIShared.Dispose();
	}
}
