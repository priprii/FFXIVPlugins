using Ktisis.Actions;
using Ktisis.Data.Config;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface;
using Ktisis.Interop.Ipc;

namespace Ktisis.Core.Types;

public interface IPluginContext
{
	ActionService Actions { get; }

	ConfigManager Config { get; }

	GuiManager Gui { get; }

	IpcManager Ipc { get; }

	IEditorContext? Editor { get; }
}
