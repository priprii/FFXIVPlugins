using System;
using Ktisis.Actions.Binds;
using Ktisis.Data.Config.Actions;

namespace Ktisis.Editor.Actions.Input;

public interface IInputManager : IDisposable
{
	void Initialize();

	void Register(ActionKeybind keybind, KeyInvokeHandler handler, KeybindTrigger trigger);
}
