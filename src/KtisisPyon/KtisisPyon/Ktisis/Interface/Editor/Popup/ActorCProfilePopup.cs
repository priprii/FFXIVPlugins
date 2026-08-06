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

public class ActorCProfilePopup : KtisisPopup
{
	private readonly IEditorContext _ctx;

	private readonly ActorEntity _entity;

	private readonly CustomizeIpcProvider _ipc;

	private readonly ListBox<(Guid UniqueId, string Name, string VirtualPath, List<(string Name, ushort WorldId, byte CharacterType, ushort CharacterSubType)> Characters, int Priority, bool IsEnabled)> _list;

	private bool _isOpening = true;

	private List<(Guid UniqueId, string Name, string VirtualPath, List<(string Name, ushort WorldId, byte CharacterType, ushort CharacterSubType)> Characters, int Priority, bool IsEnabled)> _profiles = new List<(Guid, string, string, List<(string, ushort, byte, ushort)>, int, bool)>();

	private (Guid Id, string Name) _current = (Id: Guid.Empty, Name: string.Empty);

	public ActorCProfilePopup(IEditorContext ctx, ActorEntity entity)
		: base("##ActorCProfilePopup", (ImGuiWindowFlags)0)
	{
		_ctx = ctx;
		_entity = entity;
		_ipc = ctx.Plugin.Ipc.GetCustomizeIpc();
		_list = new ListBox<(Guid, string, string, List<(string, ushort, byte, ushort)>, int, bool)>("##CProfileList", DrawItem);
	}

	protected override void OnDraw()
	{
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		if (!_entity.IsValid || !_ctx.Plugin.Ipc.IsCustomizeActive)
		{
			Close();
			return;
		}
		if (_isOpening)
		{
			_isOpening = false;
			_profiles = (from x in _ipc.GetProfileList()
				orderby x.Name
				select x).ToList();
			Ktisis.Log.Info($"Fetched {_profiles.Count} profiles");
		}
		Guid? guid = _ipc.GetActiveProfileId(_entity.Actor.ObjectIndex).Id;
		if (_entity.AssignedProfile.HasValue)
		{
			guid = _entity.AssignedProfile;
		}
		if (guid.HasValue)
		{
			foreach (var profile in _profiles)
			{
				Guid item = profile.UniqueId;
				Guid? guid2 = guid;
				if (!(item != guid2))
				{
					_current = (Id: profile.UniqueId, Name: profile.Name);
				}
			}
		}
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(25, 1);
		((ImU8String)(ref val)).AppendLiteral("Assigning collection for ");
		((ImU8String)(ref val)).AppendFormatted<string>(_entity.Name);
		ImGui.Text(val);
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(18, 1);
		((ImU8String)(ref val2)).AppendLiteral("Currently set to: ");
		((ImU8String)(ref val2)).AppendFormatted<string>(_current.Name);
		ImGui.TextDisabled(val2);
		if (_list.Draw(_profiles, out (Guid, string, string, List<(string, ushort, byte, ushort)>, int, bool) selected))
		{
			(int, string) profileByUniqueId = _ipc.GetProfileByUniqueId(selected.Item1);
			if (profileByUniqueId.Item2 != null)
			{
				SetProfile(profileByUniqueId.Item2);
			}
			_entity.AssignedProfile = selected.Item1;
		}
	}

	private void SetProfile(string data)
	{
		ushort objectIndex = _entity.Actor.ObjectIndex;
		_ipc.DeleteTemporaryProfile(objectIndex);
		_ipc.SetTemporaryProfile(objectIndex, data);
		if (!_ctx.Posing.IsEnabled)
		{
			_entity.Redraw();
		}
	}

	private bool DrawItem((Guid UniqueId, string Name, string VirtualPath, List<(string Name, ushort WorldId, byte CharacterType, ushort CharacterSubType)> Characters, int Priority, bool IsEnabled) item, bool _)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ImGui.Selectable(ImU8String.op_Implicit(item.Name), item.UniqueId == _current.Id, (ImGuiSelectableFlags)0, default(Vector2));
	}
}
