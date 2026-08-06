using Ktisis.Actions.Types;
using Ktisis.Core.Types;
using Ktisis.Interface.Windows;

namespace Ktisis.Actions.Handlers.Toolbar;

public abstract class ToolbarSetWindow : KeyAction
{
	protected ToolbarSetWindow(IPluginContext ctx)
		: base(ctx)
	{
	}

	public override bool Invoke()
	{
		if (base.Context.Editor == null || !base.Context.Editor.Config.Editor.UseToolbar)
		{
			return false;
		}
		ToolbarWindow orCreate = base.Context.Gui.GetOrCreate<ToolbarWindow>(new object[2]
		{
			base.Context.Editor,
			base.Context.Gui
		});
		Call(orCreate);
		return true;
	}

	internal abstract void Call(ToolbarWindow window);
}
