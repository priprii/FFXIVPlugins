using Ktisis.Editor.Actions.Input;

namespace Ktisis.Editor.Actions;

public interface IActionManager
{
	IInputManager Input { get; }

	IHistoryManager History { get; }

	void Initialize();
}
