using Dalamud.Game.ClientState.Keys;
using Ktisis.Actions.Attributes;
using Ktisis.Actions.Binds;
using Ktisis.Actions.Types;
using Ktisis.Core.Types;
using Ktisis.Data.Config.Actions;
using Ktisis.Data.Config.Sections;

namespace Ktisis.Actions.Handlers.Gizmo;

[Action("Gizmo_Toggle")]
public class GizmoToggleAction(IPluginContext ctx) : KeyAction(ctx)
{
	public override KeybindInfo BindInfo { get; } = new KeybindInfo
	{
		Trigger = KeybindTrigger.OnDown,
		Default = new ActionKeybind
		{
			Enabled = true,
			Combo = new KeyCombo((VirtualKey)71, (VirtualKey)17)
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
		GizmoConfig gizmo = base.Context.Config.File.Gizmo;
		gizmo.Visible = !gizmo.Visible;
		return true;
	}
}
