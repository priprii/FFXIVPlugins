using Dalamud.Game.ClientState.Keys;
using Ktisis.Actions.Attributes;
using Ktisis.Actions.Binds;
using Ktisis.Actions.Types;
using Ktisis.Core.Types;
using Ktisis.Data.Config.Actions;

namespace Ktisis.Actions.Handlers.Select;

[Action("Select_All")]
public class SelectAllAction(IPluginContext ctx) : KeyAction(ctx)
{
	public override KeybindInfo BindInfo { get; } = new KeybindInfo
	{
		Trigger = KeybindTrigger.OnDown,
		Default = new ActionKeybind
		{
			Enabled = true,
			Combo = new KeyCombo((VirtualKey)189)
		}
	};

	public override bool Invoke()
	{
		base.Context.Editor?.Interface.SelectAllEntities();
		return true;
	}
}
