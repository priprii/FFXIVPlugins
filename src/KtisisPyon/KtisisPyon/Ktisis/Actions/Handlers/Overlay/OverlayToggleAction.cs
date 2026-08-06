using Dalamud.Game.ClientState.Keys;
using Ktisis.Actions.Attributes;
using Ktisis.Actions.Binds;
using Ktisis.Actions.Types;
using Ktisis.Core.Types;
using Ktisis.Data.Config.Actions;
using Ktisis.Data.Config.Sections;

namespace Ktisis.Actions.Handlers.Overlay;

[Action("Overlay_Toggle")]
public class OverlayToggleAction(IPluginContext ctx) : KeyAction(ctx)
{
	public override KeybindInfo BindInfo { get; } = new KeybindInfo
	{
		Trigger = KeybindTrigger.OnDown,
		Default = new ActionKeybind
		{
			Enabled = true,
			Combo = new KeyCombo((VirtualKey)79, (VirtualKey)17)
		}
	};

	public override bool CanInvoke()
	{
		return base.Context.Editor != null;
	}

	public override bool Invoke()
	{
		if (!CanInvoke())
		{
			return false;
		}
		OverlayConfig overlay = base.Context.Config.File.Overlay;
		overlay.Visible = !overlay.Visible;
		return true;
	}
}
