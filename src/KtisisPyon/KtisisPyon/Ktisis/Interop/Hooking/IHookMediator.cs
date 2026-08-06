namespace Ktisis.Interop.Hooking;

public interface IHookMediator
{
	bool IsValid { get; }

	T Create<T>(params object[] param) where T : HookModule;

	bool Init(HookModule module);

	bool Remove(HookModule module);
}
