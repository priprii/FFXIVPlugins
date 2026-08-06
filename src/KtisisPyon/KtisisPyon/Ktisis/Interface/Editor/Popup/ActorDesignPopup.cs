using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using GLib.Lists;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Types;
using Ktisis.Interop.Ipc;
using Ktisis.Scene.Entities.Game;

namespace Ktisis.Interface.Editor.Popup;

public class ActorDesignPopup : KtisisPopup
{
	private readonly IEditorContext _ctx;

	private readonly ActorEntity _entity;

	private readonly GlamourerIpcProvider _ipc;

	private readonly ListBox<KeyValuePair<Guid, string>> _list;

	private (Guid Id, string Name) _current = (Id: Guid.Empty, Name: string.Empty);

	public ActorDesignPopup(IEditorContext ctx, ActorEntity entity)
		: base("##ActorDesignPopup", (ImGuiWindowFlags)0)
	{
		_ctx = ctx;
		_entity = entity;
		_ipc = ctx.Plugin.Ipc.GetGlamourerIpc();
		_list = new ListBox<KeyValuePair<Guid, string>>("##DesignList", DrawItem);
	}

	protected override void OnDraw()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (!_entity.IsValid || !_ctx.Plugin.Ipc.IsGlamourerActive)
		{
			Close();
			Ktisis.Log.Info("Stale, closing.");
			return;
		}
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(17, 1);
		((ImU8String)(ref val)).AppendLiteral("Apply design for ");
		((ImU8String)(ref val)).AppendFormatted<string>(_entity.Name);
		ImGui.Text(val);
		List<KeyValuePair<Guid, string>> list = (from x in _ipc.GetDesignList()
			orderby x.Value
			select x).ToList();
		if (_list.Draw(list, out KeyValuePair<Guid, string> selected) && _ipc.ApplyDesignToObject(_entity.Actor, selected.Key))
		{
			_entity.Redraw();
		}
	}

	private bool DrawItem(KeyValuePair<Guid, string> item, bool _)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ImGui.Selectable(ImU8String.op_Implicit(item.Value), item.Key == _current.Id, (ImGuiSelectableFlags)0, default(Vector2));
	}
}
