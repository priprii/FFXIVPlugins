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

public class ActorCollectionPopup : KtisisPopup
{
	private readonly IEditorContext _ctx;

	private readonly ActorEntity _entity;

	private readonly PenumbraIpcProvider _ipc;

	private readonly ListBox<KeyValuePair<Guid, string>> _list;

	private (Guid Id, string Name) _current = (Id: Guid.Empty, Name: string.Empty);

	public ActorCollectionPopup(IEditorContext ctx, ActorEntity entity)
		: base("##ActorCollectionPopup", (ImGuiWindowFlags)0)
	{
		_ctx = ctx;
		_entity = entity;
		_ipc = ctx.Plugin.Ipc.GetPenumbraIpc();
		_list = new ListBox<KeyValuePair<Guid, string>>("##CollectionList", DrawItem);
	}

	protected override void OnDraw()
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (!_entity.IsValid || !_ctx.Plugin.Ipc.IsPenumbraActive)
		{
			Close();
			Ktisis.Log.Info("Stale, closing.");
			return;
		}
		_current = _ipc.GetCollectionForObject(_entity.Actor);
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(25, 1);
		((ImU8String)(ref val)).AppendLiteral("Assigning collection for ");
		((ImU8String)(ref val)).AppendFormatted<string>(_entity.Name);
		ImGui.Text(val);
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(18, 1);
		((ImU8String)(ref val2)).AppendLiteral("Currently set to: ");
		((ImU8String)(ref val2)).AppendFormatted<(Guid, string)>(_current);
		ImGui.TextDisabled(val2);
		List<KeyValuePair<Guid, string>> list = (from x in _ipc.GetCollections()
			orderby x.Value
			select x).ToList();
		if (_list.Draw(list, out KeyValuePair<Guid, string> selected) && _ipc.SetCollectionForObject(_entity.Actor, selected.Key))
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
