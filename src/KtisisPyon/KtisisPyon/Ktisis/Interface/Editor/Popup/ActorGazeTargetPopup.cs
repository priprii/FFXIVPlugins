using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using GLib.Lists;
using Ktisis.Common.Extensions;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Types;
using Ktisis.Scene.Entities.Game;

namespace Ktisis.Interface.Editor.Popup;

public class ActorGazeTargetPopup : KtisisPopup
{
	private readonly IEditorContext _ctx;

	private readonly ListBox<ActorEntity> _list;

	private uint ForActorGazeTarget;

	private ActorEntity ForActor;

	public ActorGazeTargetPopup(IEditorContext ctx, ActorEntity actor)
		: base("##ActorGazeTargetPopup", (ImGuiWindowFlags)0)
	{
		_ctx = ctx;
		ForActor = actor;
		ForActorGazeTarget = actor.GetActorGazeTarget();
		_list = new ListBox<ActorEntity>("##ActorGazeTargetList", DrawActorName);
	}

	protected override void OnDraw()
	{
		if (!_ctx.IsValid)
		{
			Close();
			return;
		}
		List<ActorEntity> list = (from actor in _ctx.Scene.Children.OfType<ActorEntity>()
			where actor != ForActor
			select actor).ToList();
		if (_list.Draw(list, out ActorEntity selected) && selected.Actor.IsEnabled())
		{
			ForActor.SetActorGazeTarget(selected);
			Close();
		}
	}

	private bool DrawActorName(ActorEntity actor, bool _)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ImGui.Selectable(ImU8String.op_Implicit(actor.Name), actor.Actor.ObjectIndex == ForActorGazeTarget, (ImGuiSelectableFlags)0, default(Vector2));
	}
}
