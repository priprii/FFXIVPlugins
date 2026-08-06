using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using PyonPix.Config;
using PyonPix.Ui;

namespace PyonPix.Services;

public abstract class BaseService(Configuration config, IServiceContext services, IWindowContext windows) : IService
{
	protected readonly Configuration Config = config;

	protected readonly IServiceContext Services = services;

	protected readonly IWindowContext Windows = windows;

	public abstract Task Initialize();

	public virtual void Update(IFramework framework)
	{
	}

	public virtual Task Dispose()
	{
		return Task.CompletedTask;
	}
}
