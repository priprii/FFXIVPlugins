using System;
using System.Collections.Generic;

namespace Ktisis.Interop.Hooking;

public class HookScope : IHookModule, IDisposable
{
	private readonly IHookMediator _hook;

	private readonly List<HookModule> Modules = new List<HookModule>();

	private bool _init;

	public bool IsInit => _init;

	public HookScope(IHookMediator hook)
	{
		_hook = hook;
	}

	public void EnableAll()
	{
		Modules.ForEach(delegate(HookModule mod)
		{
			mod.EnableAll();
		});
	}

	public void DisableAll()
	{
		Modules.ForEach(delegate(HookModule mod)
		{
			mod.DisableAll();
		});
	}

	public void SetEnabled(bool enabled)
	{
		if (enabled)
		{
			EnableAll();
		}
		else
		{
			DisableAll();
		}
	}

	public bool TryGetHook<T>(out HookWrapper<T>? result) where T : Delegate
	{
		foreach (HookModule module in Modules)
		{
			if (module.TryGetHook(out HookWrapper<T> result2))
			{
				result = result2;
				return true;
			}
		}
		result = null;
		return false;
	}

	public T Create<T>(params object[] param) where T : HookModule
	{
		T val = _hook.Create<T>(param);
		Modules.Add(val);
		return val;
	}

	public bool Initialize()
	{
		bool flag = false;
		foreach (HookModule module in Modules)
		{
			flag |= module.Initialize();
		}
		return _init = flag;
	}

	public void Dispose()
	{
		Modules.ForEach(delegate(HookModule mod)
		{
			mod.Dispose();
		});
		Modules.Clear();
	}
}
