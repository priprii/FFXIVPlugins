using System;
using System.Collections.Generic;
using System.Linq;
using Hypostasis.Dalamud;

namespace Hypostasis;

public static class PluginModuleManager
{
	private static readonly Dictionary<Type, PluginModule> pluginModules = new Dictionary<Type, PluginModule>();

	public static IEnumerable<PluginModule> PluginModules => pluginModules.Values;

	public static bool Initialize()
	{
		bool result = true;
		foreach (Type type in Util.Assembly.GetTypes<PluginModule>())
		{
			PluginModule pluginModule = (PluginModule)Activator.CreateInstance(type);
			if (pluginModule == null)
			{
				continue;
			}
			if (pluginModule.IsValid)
			{
				if (pluginModule.ShouldEnable)
				{
					ToggleOrInvalidateModule(pluginModule, Hypostasis.IsDebug);
				}
			}
			else
			{
				DalamudApi.LogWarning($"{type} failed to load!");
				result = false;
			}
			pluginModules.Add(type, pluginModule);
		}
		return result;
	}

	public static T GetModule<T>() where T : PluginModule
	{
		return (T)pluginModules[typeof(T)];
	}

	public static void CheckModules()
	{
		foreach (PluginModule item in pluginModules.Values.Where((PluginModule pluginModule) => pluginModule.IsValid && pluginModule.ShouldEnable != pluginModule.IsEnabled))
		{
			ToggleOrInvalidateModule(item, logInfo: true);
		}
	}

	public static void ToggleOrInvalidateModule(PluginModule pluginModule, bool logInfo)
	{
		try
		{
			pluginModule.Toggle();
			if (logInfo)
			{
				DalamudApi.LogInfo(pluginModule.IsEnabled ? $"Enabled plugin module: {pluginModule}" : $"Disabled plugin module: {pluginModule}");
			}
		}
		catch (Exception exception)
		{
			DalamudApi.LogError($"Error in plugin module: {pluginModule}", exception);
			pluginModule.IsValid = false;
		}
	}

	public static void Dispose()
	{
		foreach (PluginModule item in pluginModules.Values.Where((PluginModule pluginModule) => pluginModule.IsValid))
		{
			if (item.IsEnabled)
			{
				item.Toggle();
			}
			item.Dispose();
		}
	}
}
