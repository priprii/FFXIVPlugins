using System;
using Dalamud.Hooking;

namespace Ktisis.Interop.Hooking;

public class HookWrapper<T> : IHookWrapper, IDalamudHook, IDisposable where T : Delegate
{
	private readonly Hook<T> _hook;

	public string Name { get; }

	public nint Address => _hook.Address;

	public bool IsEnabled => _hook.IsEnabled;

	public bool IsDisposed => _hook.IsDisposed;

	public string BackendName => _hook.BackendName;

	public HookWrapper(Hook<T> hook)
	{
		_hook = hook;
		Name = GetType().GetGenericArguments()[0].Name;
	}

	public void Enable()
	{
		_hook.Enable();
	}

	public void Disable()
	{
		_hook.Disable();
	}

	public void Dispose()
	{
		Ktisis.Log.Debug("Disposing hook: '" + Name + "'");
		if (_hook.IsEnabled)
		{
			_hook.Disable();
		}
		_hook.Dispose();
		GC.SuppressFinalize(this);
	}
}
