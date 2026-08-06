using Dalamud.Bindings.ImGuizmo;
using Ktisis.Actions.Types;
using Ktisis.Core.Types;

namespace Ktisis.Actions.Handlers.Gizmo;

public abstract class GizmoOpAction : KeyAction
{
	protected abstract ImGuizmoOperation TargetOp { get; init; }

	protected GizmoOpAction(IPluginContext ctx)
		: base(ctx)
	{
	}

	public override bool Invoke()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (base.Context.Editor == null || base.Context.Editor.Selection.Count == 0)
		{
			return false;
		}
		base.Context.Config.File.Gizmo.Operation = TargetOp;
		return true;
	}
}
