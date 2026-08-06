using System;
using System.Collections.Concurrent;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using PyonCam.Config;

namespace PyonCam.Services;

public sealed class ServiceContext : IServiceContext
{
	private readonly ConcurrentDictionary<Type, object> _services = new ConcurrentDictionary<Type, object>();

	[PluginService]
	public IClientState ClientState { get; private set; }

	[PluginService]
	public ICommandManager CommandManager { get; private set; }

	[PluginService]
	public IDataManager DataManager { get; private set; }

	[PluginService]
	public IFramework Framework { get; private set; }

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
	public ISigScanner SigScanner { get; private set; }

	[PluginService]
	public ICondition Condition { get; private set; }

	[PluginService]
	public IGameGui GameGui { get; private set; }

	[PluginService]
	public ITargetManager TargetManager { get; private set; }

	public static ServiceContext Instance { get; private set; }

	public ServiceContext(IDalamudPluginInterface pi)
	{
		pi.Inject((object)this, Array.Empty<object>());
		Instance = this;
	}

	public void Register<TService>(TService service) where TService : class
	{
		_services[typeof(TService)] = service;
	}

	public TService Get<TService>() where TService : class
	{
		if (TryGet<TService>(out TService service) && service != null)
		{
			return service;
		}
		throw new InvalidOperationException("Service not registered: " + typeof(TService).Name);
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
			"IDataManager" => DataManager, 
			"IFramework" => Framework, 
			"IGameInteropProvider" => GameInteropProvider, 
			"IPluginLog" => Log, 
			"IObjectTable" => Objects, 
			"IPlayerState" => PlayerState, 
			"IDalamudPluginInterface" => PluginInterface, 
			"ISigScanner" => SigScanner, 
			"ICondition" => Condition, 
			"IGameGui" => GameGui, 
			"ITargetManager" => TargetManager, 
			_ => null, 
		} is TService val2)
		{
			service = val2;
			return true;
		}
		service = null;
		return false;
	}

	public void Initialize(Configuration config)
	{
		Register(new PresetService(config, this));
		Register(new InputService(config, this));
		Register(new CameraService(config, this));
		Register(new IpcService(config, this));
		Get<InputService>().Initialize();
		Get<CameraService>().Initialize();
		Get<IpcService>().Initialize();
		Get<PresetService>().Initialize();
	}

	public void Update(IFramework framework)
	{
		Get<CameraService>().Update();
	}

	public void Dispose()
	{
		if (TryGet<IpcService>(out IpcService service))
		{
			service?.Dispose();
		}
		if (TryGet<InputService>(out InputService service2))
		{
			service2?.Dispose();
		}
		if (TryGet<CameraService>(out CameraService service3))
		{
			service3?.Dispose();
		}
	}
}
