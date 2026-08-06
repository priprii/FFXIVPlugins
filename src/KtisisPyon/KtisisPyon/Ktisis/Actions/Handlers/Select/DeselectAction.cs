using Dalamud.Game.ClientState.Keys;
using Ktisis.Actions.Attributes;
using Ktisis.Actions.Binds;
using Ktisis.Actions.Types;
using Ktisis.Core.Types;
using Ktisis.Data.Config.Actions;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Selection;

namespace Ktisis.Actions.Handlers.Select;

[Action("Select_None")]
public class DeselectAction(IPluginContext ctx) : KeyAction(ctx)
{
	public override KeybindInfo BindInfo { get; } = new KeybindInfo
	{
		Trigger = KeybindTrigger.OnDown,
		Default = new ActionKeybind
		{
			Enabled = true,
			Combo = new KeyCombo((VirtualKey)27)
		}
	};

	public override bool CanInvoke()
	{
		IEditorContext editor = base.Context.Editor;
		if (editor != null)
		{
			ISelectManager selection = editor.Selection;
			if (selection != null)
			{
				return selection.Count > 0;
			}
		}
		return false;
	}

	public override bool Invoke()
	{
		if (!CanInvoke())
		{
			return false;
		}
		base.Context.Editor.Selection.Clear();
		return true;
	}
}
