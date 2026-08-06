using Dalamud.Bindings.ImGuizmo;
using Dalamud.Game.ClientState.Keys;
using Ktisis.Actions.Attributes;
using Ktisis.Actions.Binds;
using Ktisis.Actions.Types;
using Ktisis.Core.Types;
using Ktisis.Data.Config.Actions;
using Ktisis.Data.Config.Sections;

namespace Ktisis.Actions.Handlers.Gizmo;

[Action("Gizmo_ToggleMode")]
public class GizmoModeAction(IPluginContext ctx) : KeyAction(ctx)
{
	public override KeybindInfo BindInfo { get; } = new KeybindInfo
	{
		Trigger = KeybindTrigger.OnDown,
		Default = new ActionKeybind
		{
			Enabled = true,
			Combo = new KeyCombo((VirtualKey)88, (VirtualKey)17)
		}
	};

	public override bool Invoke()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (base.Context.Editor == null || base.Context.Editor.Selection.Count == 0)
		{
			return false;
		}
		GizmoConfig gizmo = base.Context.Config.File.Gizmo;
		gizmo.Mode = (ImGuizmoMode)(gizmo.Mode ^ 1);
		return true;
	}
}
