using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using PyonPix.Config;
using PyonPix.Services.Core;
using PyonPix.Services.Game;
using PyonPix.Ui;

namespace PyonPix.Services;

public sealed class ServiceContext : IServiceContext
{
	private readonly ConcurrentDictionary<Type, object> _services = new ConcurrentDictionary<Type, object>();

	[PluginService]
	public IClientState ClientState { get; private set; }

	[PluginService]
	public ICommandManager CommandManager { get; private set; }

	[PluginService]
	public ICondition Condition { get; private set; }

	[PluginService]
	public IDataManager DataManager { get; private set; }

	[PluginService]
	public IFramework Framework { get; private set; }

	[PluginService]
	public IGameConfig GameConfig { get; private set; }

	[PluginService]
	public IGameInteropProvider GameInteropProvider { get; private set; }

	[PluginService]
	public IPluginLog Log { get; private set; }

	[PluginService]
	public IObjectTable Objects { get; private set; }

	[PluginService]
	public IPlayerState PlayerState { get; private set; }

	[PluginService]
	public IDalamudPluginInterface PluginInterface { get; private set; }

	[PluginService]
	public ITextureProvider TextureProvider { get; private set; }

	public static ServiceContext Instance { get; private set; }

	public ServiceContext(IDalamudPluginInterface pi)
	{
		pi.Inject((object)this, Array.Empty<object>());
		Instance = this;
	}

	public TService Register<TService>(TService service) where TService : class
	{
		_services[typeof(TService)] = service;
		return service;
	}

	public TService? Get<TService>() where TService : class
	{
		if (TryGet<TService>(out TService service) && service != null)
		{
			return service;
		}
		Log.Error("Service Failure: " + typeof(TService).Name, Array.Empty<object>());
		return null;
	}

	public bool TryGet<TService>(out TService? service) where TService : class
	{
		if (_services.TryGetValue(typeof(TService), out object value) && value is TService val)
		{
			service = val;
			return true;
		}
		if (typeof(TService).Name switch
		{
			"IClientState" => ClientState, 
			"ICommandManager" => CommandManager, 
			"ICondition" => Condition, 
			"IDataManager" => DataManager, 
			"IFramework" => Framework, 
			"IGameConfig" => GameConfig, 
			"IGameInteropProvider" => GameInteropProvider, 
			"IPluginLog" => Log, 
			"IObjectTable" => Objects, 
			"IPlayerState" => PlayerState, 
			"IDalamudPluginInterface" => PluginInterface, 
			"ITextureProvider" => TextureProvider, 
			_ => null, 
		} is TService val2)
		{
			service = val2;
			return true;
		}
		service = null;
		return false;
	}

	public async Task Initialize(Configuration config, IWindowContext windows)
	{
		Register(new StateService(config, this, windows));
		Register(new SyncService(config, this, windows));
		Register(new PixService(config, this, windows));
		Register(new DXService(config, this, windows));
		Register(new ExtensionsService(config, this, windows));
		Register(new DataService(config, this, windows));
		Register(new BrowserService(config, this, windows));
		Register(new LightService(config, this, windows));
		Register(new RendererService(config, this, windows));
		Register(new AudioService(config, this, windows));
		Register(new PixInputService(config, this, windows));
		await Get<StateService>().Initialize();
		await Get<SyncService>().Initialize();
		await Get<PixService>().Initialize();
		await Get<DXService>().Initialize();
		await Get<ExtensionsService>().Initialize();
		await Get<DataService>().Initialize();
		await Get<BrowserService>().Initialize();
		await Get<LightService>().Initialize();
		await Get<RendererService>().Initialize();
		await Get<AudioService>().Initialize();
		await Get<PixInputService>().Initialize();
	}

	public void InitializeUpdate()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		Framework.Update += new OnUpdateDelegate(Update);
	}

	public void Update(IFramework framework)
	{
		if (TryGet<StateService>(out StateService service))
		{
			service.Update();
		}
		if (TryGet<SyncService>(out SyncService service2))
		{
			service2.Update();
		}
		if (TryGet<BrowserService>(out BrowserService service3))
		{
			service3.Update();
		}
		if (TryGet<AudioService>(out AudioService service4))
		{
			service4.Update();
		}
		if (TryGet<PixInputService>(out PixInputService service5))
		{
			service5.Update();
		}
	}

	public async Task Dispose()
	{
		IFramework framework = Framework;
		ServiceContext serviceContext = this;
		framework.Update -= new OnUpdateDelegate(serviceContext.Update);
		if (TryGet<StateService>(out StateService service))
		{
			await service.Dispose();
		}
		if (TryGet<SyncService>(out SyncService service2))
		{
			await service2.Dispose();
		}
		if (TryGet<AudioService>(out AudioService service3))
		{
			await service3.Dispose();
		}
		if (TryGet<RendererService>(out RendererService service4))
		{
			await service4.Dispose();
		}
		if (TryGet<LightService>(out LightService service5))
		{
			await service5.Dispose();
		}
		if (TryGet<BrowserService>(out BrowserService service6))
		{
			await service6.Dispose();
		}
		if (TryGet<ExtensionsService>(out ExtensionsService service7))
		{
			await service7.Dispose();
		}
		if (TryGet<DataService>(out DataService service8))
		{
			await service8.Dispose();
		}
		if (TryGet<PixInputService>(out PixInputService service9))
		{
			await service9.Dispose();
		}
	}
}
