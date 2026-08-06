using System;
using System.Collections.Generic;
using System.Linq;
using Ktisis.Interop.Hooking;
using Ktisis.Scene.Modules;

namespace Ktisis.Scene;

public class SceneModuleContainer
{
	private readonly HookScope _scope;

	private readonly Dictionary<Type, SceneModule> Modules = new Dictionary<Type, SceneModule>();

	public SceneModuleContainer(HookScope scope)
	{
		_scope = scope;
	}

	public T GetModule<T>() where T : SceneModule
	{
		return (T)Modules[typeof(T)];
	}

	public bool TryGetModule<T>(out T? module) where T : SceneModule
	{
		module = null;
		SceneModule value;
		bool num = Modules.TryGetValue(typeof(T), out value);
		if (num)
		{
			module = value as T;
		}
		return num;
	}

	protected T AddModule<T>(params object[] param) where T : SceneModule
	{
		T val = _scope.Create<T>(param.Prepend(this).ToArray());
		Modules.Add(typeof(T), val);
		return val;
	}

	protected void InitializeModules()
	{
		foreach (SceneModule item in Modules.Values.Where((SceneModule module) => module.Initialize() && module.IsInit))
		{
			try
			{
				item.Setup();
			}
			catch (Exception value)
			{
				Ktisis.Log.Error($"Failed to setup module '{item.GetType().Name}':\n{value}");
			}
		}
	}

	protected void UpdateModules()
	{
		foreach (var (type2, sceneModule2) in Modules)
		{
			try
			{
				sceneModule2.Update();
			}
			catch (Exception value)
			{
				Ktisis.Log.Error($"Failed to handle update for module '{type2.Name}':\n{value}");
			}
		}
	}

	protected void DisposeModules()
	{
		foreach (SceneModule value in Modules.Values)
		{
			value.Dispose();
		}
		Modules.Clear();
	}
}
