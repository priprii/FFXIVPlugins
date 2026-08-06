using Dalamud.Game.ClientState.Keys;
using Ktisis.Actions.Attributes;
using Ktisis.Actions.Binds;
using Ktisis.Core.Types;
using Ktisis.Data.Config.Actions;
using Ktisis.Interface.Windows;

namespace Ktisis.Actions.Handlers.Toolbar;

[Action("Toolbar_ToggleConfig")]
public class ToggleConfig(IPluginContext ctx) : ToolbarSetWindow(ctx)
{
	public override KeybindInfo BindInfo { get; } = new KeybindInfo
	{
		Trigger = KeybindTrigger.OnDown,
		Default = new ActionKeybind
		{
			Enabled = true,
			Combo = new KeyCombo((VirtualKey)119)
		}
	};

	internal override void Call(ToolbarWindow window)
	{
		window.DrawConfigWindow();
	}
}
