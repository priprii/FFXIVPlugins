using Dalamud.Bindings.ImGuizmo;
using Dalamud.Game.ClientState.Keys;
using Ktisis.Actions.Attributes;
using Ktisis.Actions.Binds;
using Ktisis.Core.Types;
using Ktisis.Data.Config.Actions;

namespace Ktisis.Actions.Handlers.Gizmo;

[Action("Gizmo_SetUniversalMode")]
public class OpUniversalAction(IPluginContext ctx) : GizmoOpAction(ctx)
{
	protected override ImGuizmoOperation TargetOp { get; init; } = (ImGuizmoOperation)14463;

	public override KeybindInfo BindInfo { get; } = new KeybindInfo
	{
		Trigger = KeybindTrigger.OnDown,
		Default = new ActionKeybind
		{
			Enabled = true,
			Combo = new KeyCombo((VirtualKey)85, (VirtualKey)17)
		}
	};
}
