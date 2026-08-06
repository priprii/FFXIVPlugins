using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using GLib.Lists;
using Ktisis.Common.Extensions;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Types;
using Ktisis.Scene.Modules.Actors;
using Ktisis.Services.Game;

namespace Ktisis.Interface.Editor.Popup;

public class OverworldActorPopup : KtisisPopup
{
	private readonly ActorService _actors;

	private readonly IEditorContext _ctx;

	private readonly ListBox<IGameObject> _list;

	public OverworldActorPopup(ActorService actors, IEditorContext ctx)
		: base("##OverworldActorPopup", (ImGuiWindowFlags)0)
	{
		_actors = actors;
		_ctx = ctx;
		_list = new ListBox<IGameObject>("##OverworldActorList", DrawActorName);
	}

	protected override void OnDraw()
	{
		if (!_ctx.IsValid)
		{
			Close();
			return;
		}
		List<IGameObject> list = _actors.GetOverworldActors().ToList();
		if (_list.Draw(list, out IGameObject selected) && selected.IsEnabled())
		{
			AddActor(selected);
		}
	}

	private async void AddActor(IGameObject actor)
	{
		await _ctx.Scene.GetModule<ActorModule>().AddFromOverworld(actor);
	}

	private bool DrawActorName(IGameObject actor, bool isFocus)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return ImGui.Selectable(ImU8String.op_Implicit(actor.GetNameOrFallback(_ctx)), isFocus, (ImGuiSelectableFlags)0, default(Vector2));
	}
}
