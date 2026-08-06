using Dalamud.Game.ClientState.Keys;
using Ktisis.Actions.Attributes;
using Ktisis.Actions.Binds;
using Ktisis.Actions.Types;
using Ktisis.Core.Types;
using Ktisis.Data.Config.Actions;
using Ktisis.Editor.Actions;
using Ktisis.Editor.Context.Types;

namespace Ktisis.Actions.Handlers.History;

[Action("History_Redo")]
public class RedoAction(IPluginContext ctx) : KeyAction(ctx)
{
	public override KeybindInfo BindInfo { get; } = new KeybindInfo
	{
		Trigger = KeybindTrigger.OnDown,
		Default = new ActionKeybind
		{
			Enabled = true,
			Combo = new KeyCombo((VirtualKey)90, (VirtualKey)17, (VirtualKey)16)
		}
	};

	public override bool CanInvoke()
	{
		IEditorContext editor = base.Context.Editor;
		if (editor != null)
		{
			IActionManager actions = editor.Actions;
			if (actions != null)
			{
				IHistoryManager history = actions.History;
				if (history != null)
				{
					return history.CanRedo;
				}
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
		base.Context.Editor.Actions.History.Redo();
		return true;
	}
}
