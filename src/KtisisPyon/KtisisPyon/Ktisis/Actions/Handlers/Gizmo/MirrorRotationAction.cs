using Dalamud.Game.ClientState.Keys;
using Ktisis.Actions.Attributes;
using Ktisis.Actions.Binds;
using Ktisis.Core.Types;
using Ktisis.Data.Config.Actions;

namespace Ktisis.Actions.Handlers.Gizmo;

[Action("Gizmo_MirrorRotation")]
public class MirrorRotationAction(IPluginContext ctx) : GizmoModeAction(ctx)
{
	public override KeybindInfo BindInfo { get; } = new KeybindInfo
	{
		Trigger = KeybindTrigger.OnDown,
		Default = new ActionKeybind
		{
			Enabled = false,
			Combo = new KeyCombo((VirtualKey)0)
		}
	};

	public override bool Invoke()
	{
		if (base.Context.Editor == null)
		{
			return false;
		}
		base.Context.Editor.Config.Gizmo.SetNextMirrorRotation();
		return true;
	}
}
