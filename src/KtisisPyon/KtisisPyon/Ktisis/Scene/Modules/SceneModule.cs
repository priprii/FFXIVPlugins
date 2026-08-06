using Ktisis.Interop.Hooking;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Modules;

public abstract class SceneModule : HookModule
{
	protected readonly ISceneManager Scene;

	public SceneModule(IHookMediator hook, ISceneManager scene)
		: base(hook)
	{
		Scene = scene;
	}

	protected bool CheckValid()
	{
		bool isValid = Scene.IsValid;
		if (!isValid)
		{
			DisableAll();
			Ktisis.Log.Warning("Hook called from '" + GetType().Name + "' with invalid scene state, disabling.");
		}
		return isValid;
	}

	public virtual void Setup()
	{
	}

	public virtual void Update()
	{
	}
}
