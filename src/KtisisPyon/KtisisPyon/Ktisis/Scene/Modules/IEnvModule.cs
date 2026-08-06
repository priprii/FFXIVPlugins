using System;
using Ktisis.Interop.Hooking;

namespace Ktisis.Scene.Modules;

public interface IEnvModule : IHookModule, IDisposable
{
	EnvOverride Override { get; set; }

	float Time { get; set; }

	int Day { get; set; }

	byte Weather { get; set; }
}
