using System;
using Dalamud.Plugin.Ipc;
using PyonCam.Config;

namespace PyonCam.Services;

public class IpcService : IDisposable
{
	private readonly Configuration _config;

	private readonly IServiceContext _services;

	public const int IPCVersion = 2;

	private readonly ICallGateProvider<object> InitializedProvider;

	private readonly ICallGateProvider<object> DisposedProvider;

	private readonly ICallGateProvider<int> GetIPCVersionProvider;

	private readonly ICallGateProvider<string> GetVersionProvider;

	private readonly ICallGateProvider<ulong, bool> SetCamTargetObjectProvider;

	private readonly ICallGateProvider<ulong> GetCamTargetObjectProvider;

	private readonly ICallGateProvider<bool> ResetCamTargetObjectProvider;

	public IpcService(Configuration config, IServiceContext services)
	{
		_config = config;
		_services = services;
		InitializedProvider = _services.PluginInterface.GetIpcProvider<object>("PyonCam.Initialized");
		DisposedProvider = _services.PluginInterface.GetIpcProvider<object>("PyonCam.Disposed");
		GetIPCVersionProvider = _services.PluginInterface.GetIpcProvider<int>("PyonCam.GetIPCVersion");
		GetVersionProvider = _services.PluginInterface.GetIpcProvider<string>("PyonCam.GetVersion");
		SetCamTargetObjectProvider = _services.PluginInterface.GetIpcProvider<ulong, bool>("PyonCam.SetCamTargetObject");
		GetCamTargetObjectProvider = _services.PluginInterface.GetIpcProvider<ulong>("PyonCam.GetCamTargetObject");
		ResetCamTargetObjectProvider = _services.PluginInterface.GetIpcProvider<bool>("PyonCam.ResetCamTargetObject");
	}

	public void Initialize()
	{
		GetIPCVersionProvider.RegisterFunc((Func<int>)(() => 2));
		GetVersionProvider.RegisterFunc((Func<string>)(() => Plugin.Version.ToString()));
		SetCamTargetObjectProvider.RegisterFunc((Func<ulong, bool>)delegate(ulong target)
		{
			if (!_services.TryGet<CameraService>(out CameraService service))
			{
				return false;
			}
			if (_services.Objects.SearchById(target) != null)
			{
				service.SetOrbitTarget(target);
				return true;
			}
			return false;
		});
		GetCamTargetObjectProvider.RegisterFunc((Func<ulong>)(() => _services.TryGet<CameraService>(out CameraService service) ? service.GetOrbitTarget() : 0));
		ResetCamTargetObjectProvider.RegisterFunc((Func<bool>)delegate
		{
			if (!_services.TryGet<CameraService>(out CameraService service))
			{
				return false;
			}
			service.RevertOrbitTarget();
			return true;
		});
		InitializedProvider.SendMessage();
	}

	public void Dispose()
	{
		DisposedProvider.SendMessage();
		((ICallGateProvider)GetIPCVersionProvider).UnregisterFunc();
		((ICallGateProvider)GetVersionProvider).UnregisterFunc();
		((ICallGateProvider)SetCamTargetObjectProvider).UnregisterFunc();
		((ICallGateProvider)GetCamTargetObjectProvider).UnregisterFunc();
		((ICallGateProvider)ResetCamTargetObjectProvider).UnregisterFunc();
		((ICallGateProvider)DisposedProvider).UnregisterFunc();
	}
}
