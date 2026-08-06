using Ktisis.Actions.Binds;
using Ktisis.Core.Types;
using Ktisis.Data.Config.Actions;

namespace Ktisis.Actions.Types;

public abstract class KeyAction : ActionBase, IKeybind
{
	public abstract KeybindInfo BindInfo { get; }

	protected KeyAction(IPluginContext ctx)
		: base(ctx)
	{
	}

	public ActionKeybind GetKeybind()
	{
		return base.Context.Config.File.Keybinds.GetOrSetDefault(GetName(), BindInfo.Default);
	}
}
