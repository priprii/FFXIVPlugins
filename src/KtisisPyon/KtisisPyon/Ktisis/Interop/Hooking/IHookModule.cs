using System;

namespace Ktisis.Interop.Hooking;

public interface IHookModule : IDisposable
{
	bool IsInit { get; }

	void EnableAll();

	void DisableAll();

	void SetEnabled(bool enabled);

	bool TryGetHook<T>(out HookWrapper<T>? result) where T : Delegate;

	bool Initialize();
}
