using System;
using Dalamud.Hooking;

namespace Ktisis.Interop.Hooking;

public interface IHookWrapper : IDalamudHook, IDisposable
{
	string Name { get; }

	void Enable();

	void Disable();
}
