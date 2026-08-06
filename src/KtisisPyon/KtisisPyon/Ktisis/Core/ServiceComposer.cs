using System;
using System.Linq;
using System.Reflection;
using Dalamud.Plugin;
using Ktisis.Core.Attributes;
using Ktisis.Services.Plugin;
using Microsoft.Extensions.DependencyInjection;

namespace Ktisis.Core;

public sealed class ServiceComposer
{
	private readonly ServiceCollection _services = new ServiceCollection();

	public ServiceComposer AddSingleton<T>(T inst) where T : class
	{
		_services.AddSingleton(inst);
		return this;
	}

	public ServiceComposer AddDalamudServices(IDalamudPluginInterface dpi)
	{
		dpi.Create<DalamudServices>(Array.Empty<object>()).Add(dpi, _services);
		return this;
	}

	public ServiceComposer AddFromAttributes()
	{
		foreach (Type item in from t in Assembly.GetExecutingAssembly().GetTypes()
			where t.CustomAttributes.Any((CustomAttributeData attr) => attr.AttributeType.IsAssignableTo(typeof(ServiceAttribute)))
			select t)
		{
			Attribute attribute = item.GetCustomAttributes().First((Attribute attr) => attr is ServiceAttribute);
			if (!(attribute is SingletonAttribute))
			{
				if (attribute is TransientAttribute)
				{
					_services.AddTransient(item);
				}
			}
			else
			{
				_services.AddSingleton(item);
			}
		}
		_services.BuildServiceProvider(new ServiceProviderOptions());
		return this;
	}

	public ServiceProvider BuildProvider()
	{
		return _services.BuildServiceProvider();
	}
}
