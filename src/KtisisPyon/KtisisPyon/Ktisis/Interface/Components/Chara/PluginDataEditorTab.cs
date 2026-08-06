using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using GLib.Widgets;
using Ktisis.Core.Attributes;
using Ktisis.Editor.Context.Types;
using Ktisis.Interop.Ipc;
using Ktisis.Scene.Entities.Game;

namespace Ktisis.Interface.Components.Chara;

[Transient]
public class PluginDataEditorTab
{
	private readonly IpcManager _ipcManager;

	private readonly IEditorContext _ctx;

	private readonly IDalamudPluginInterface _dpi;

	private ActorEntity? _actor;

	private readonly IList<(Guid UniqueId, string Name, string VirtualPath, List<(string Name, ushort WorldId, byte CharacterType, ushort CharacterSubType)> Characters, int Priority, bool IsEnabled)> _cPlusProfiles = new List<(Guid, string, string, List<(string, ushort, byte, ushort)>, int, bool)>();

	private readonly Dictionary<Guid, string> _penumbraCollections = new Dictionary<Guid, string>();

	private readonly Dictionary<Guid, string> _glamourerCollections = new Dictionary<Guid, string>();

	private (Guid Id, string Name) _currentPenumbra = (Id: Guid.Empty, Name: string.Empty);

	private Guid? _selectedGlamourer;

	private ImGuiTextFilter _glamourerFilter;

	public PluginDataEditorTab(IEditorContext ctx, IDalamudPluginInterface dpi)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		_ctx = ctx;
		_ipcManager = ctx.Plugin.Ipc;
		_dpi = dpi;
		_actor = null;
		_glamourerFilter = default(ImGuiTextFilter);
		if (_ipcManager.IsCustomizeActive)
		{
			_cPlusProfiles = (from x in _ipcManager.GetCustomizeIpc().GetProfileList()
				orderby x.Name
				select x).ToList();
		}
		if (_ipcManager.IsPenumbraActive)
		{
			_penumbraCollections = _ipcManager.GetPenumbraIpc().GetCollections();
		}
		if (_ipcManager.IsGlamourerActive)
		{
			_glamourerCollections = _ipcManager.GetGlamourerIpc().GetDesignList();
		}
	}

	public void SetTarget(ActorEntity actor)
	{
		_actor = actor;
	}

	public unsafe void Draw()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		if (_actor == null)
		{
			ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.ipc.warn_actor")));
			return;
		}
		DisabledDisposable val = ImRaii.Disabled(!_ipcManager.IsAnyMcdfActive && _actor.GetHuman() != null);
		try
		{
			if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.ipc.mcdf_load")), default(Vector2)))
			{
				_ctx.Interface.OpenMcdfFile(delegate(string path)
				{
					ImportMcdf(_actor, path);
				});
			}
			if (ImGui.IsItemHovered((ImGuiHoveredFlags)128))
			{
				TooltipDisposable val2 = ImRaii.Tooltip();
				try
				{
					ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.ipc.mcdf_tip")));
				}
				finally
				{
					((TooltipDisposable)(ref val2)).Dispose();
				}
			}
			ImGui.SameLine();
			if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("workspace.entity_menu.ipc.revert")), default(Vector2)))
			{
				_actor.AssignedProfile = null;
				_ctx.Characters.Mcdf.Revert(_actor.Actor);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		if (_ipcManager.IsCustomizeActive)
		{
			Separators.SeparatorText(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.ipc.customize.title")), 0u, 0.3f, 5f, Separators.LineHeight.Bottom, ImGui.GetColorU32((ImGuiCol)1));
			DrawCustomizePlus(_actor);
		}
		if (_ipcManager.IsPenumbraActive)
		{
			Separators.SeparatorText(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.ipc.penumbra.title")), 0u, 0.3f, 5f, Separators.LineHeight.Bottom, ImGui.GetColorU32((ImGuiCol)1));
			DrawPenumbra(_actor);
		}
		if (_ipcManager.IsGlamourerActive)
		{
			Separators.SeparatorText(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.ipc.glamourer.title")), 0u, 0.3f, 5f, Separators.LineHeight.Bottom, ImGui.GetColorU32((ImGuiCol)1));
			DrawGlamourer(_actor);
		}
	}

	private unsafe void DrawCustomizePlus(ActorEntity actor)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		CustomizeIpcProvider customizeIpc = _ipcManager.GetCustomizeIpc();
		Guid? currentId = customizeIpc.GetActiveProfileId(actor.Actor.ObjectIndex).Id;
		if (actor.AssignedProfile.HasValue)
		{
			currentId = actor.AssignedProfile;
		}
		if (ImGui.BeginCombo(ImU8String.op_Implicit("##CPlus"), ImU8String.op_Implicit(currentId.HasValue ? _cPlusProfiles.FirstOrDefault<(Guid, string, string, List<(string, ushort, byte, ushort)>, int, bool)>(delegate((Guid UniqueId, string Name, string VirtualPath, List<(string Name, ushort WorldId, byte CharacterType, ushort CharacterSubType)> Characters, int Priority, bool IsEnabled) p)
		{
			Guid item3 = p.UniqueId;
			Guid? guid2 = currentId;
			return item3 == guid2;
		}).Item2 : ""), (ImGuiComboFlags)0))
		{
			foreach (var cPlusProfile in _cPlusProfiles)
			{
				Guid item = cPlusProfile.UniqueId;
				Guid? guid = currentId;
				bool flag = item == guid;
				if (ImGui.Selectable(ImU8String.op_Implicit(cPlusProfile.Name), flag, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					if (flag)
					{
						customizeIpc.DeleteTemporaryProfile(((Character)actor.Character).ObjectIndex);
						actor.AssignedProfile = null;
						break;
					}
					customizeIpc.DeleteTemporaryProfile(((Character)actor.Character).ObjectIndex);
					string item2 = customizeIpc.GetProfileByUniqueId(cPlusProfile.UniqueId).Data;
					if (item2 != null)
					{
						customizeIpc.SetTemporaryProfile(((Character)actor.Character).ObjectIndex, item2);
						actor.AssignedProfile = cPlusProfile.UniqueId;
					}
				}
			}
			ImGui.EndCombo();
		}
		ImGui.SameLine();
		ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.ipc.customize.profile")));
		ImGui.SameLine();
		ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - Buttons.CalcSize() - 3f);
		if (Buttons.IconButton((FontAwesomeIcon)61582))
		{
			_dpi.InstalledPlugins.FirstOrDefault((IExposedPlugin p) => p != null && p.InternalName == "CustomizePlus" && p.IsLoaded).OpenMainUi();
		}
	}

	private unsafe void DrawPenumbra(ActorEntity actor)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		PenumbraIpcProvider penumbraIpc = _ipcManager.GetPenumbraIpc();
		(Guid, string) collectionForObject = penumbraIpc.GetCollectionForObject(actor.Actor);
		if (collectionForObject.Item1 != Guid.Empty)
		{
			foreach (KeyValuePair<Guid, string> penumbraCollection in _penumbraCollections)
			{
				if (!(penumbraCollection.Key != collectionForObject.Item1))
				{
					_currentPenumbra = (Id: penumbraCollection.Key, Name: penumbraCollection.Value);
				}
			}
		}
		ImU8String val = ImU8String.op_Implicit("##Penumbra");
		(Guid, string) currentPenumbra = _currentPenumbra;
		if (ImGui.BeginCombo(val, ImU8String.op_Implicit((currentPenumbra.Item1 != default(Guid) || currentPenumbra.Item2 != null) ? _currentPenumbra.Name : ""), (ImGuiComboFlags)0))
		{
			foreach (KeyValuePair<Guid, string> penumbraCollection2 in _penumbraCollections)
			{
				bool flag = penumbraCollection2.Key == _currentPenumbra.Id;
				if (!ImGui.Selectable(ImU8String.op_Implicit(penumbraCollection2.Value), flag, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					continue;
				}
				if (flag)
				{
					if (penumbraIpc.SetCollectionForObject(actor.Actor, null))
					{
						actor.Redraw();
					}
					break;
				}
				if (penumbraIpc.SetCollectionForObject(actor.Actor, penumbraCollection2.Key))
				{
					actor.Redraw();
				}
				_currentPenumbra.Id = penumbraCollection2.Key;
				_currentPenumbra.Name = penumbraCollection2.Value;
			}
			ImGui.EndCombo();
		}
		ImGui.SameLine();
		ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.ipc.penumbra.collection")));
		ImGui.SameLine();
		ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - Buttons.CalcSize() - 3f);
		if (Buttons.IconButton((FontAwesomeIcon)61582))
		{
			_dpi.InstalledPlugins.FirstOrDefault((IExposedPlugin p) => p != null && p.InternalName == "Penumbra" && p.IsLoaded).OpenMainUi();
		}
		DisabledDisposable val2 = ImRaii.Disabled(actor.GetHuman() == null);
		try
		{
			if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.ipc.penumbra.invisible_skin")), default(Vector2)))
			{
				_ctx.Characters.Mcdf.SetInvisibleSkin(actor);
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private void DrawGlamourer(ActorEntity actor)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		GlamourerIpcProvider glamourerIpc = _ipcManager.GetGlamourerIpc();
		GroupDisposable val = ImRaii.Group();
		try
		{
			((ImGuiTextFilter)(ref _glamourerFilter)).Draw(ImU8String.op_Implicit("##Filter"), 0f);
			ListBoxDisposable val2 = ImRaii.ListBox(ImU8String.op_Implicit("##Glamourer"));
			try
			{
				foreach (KeyValuePair<Guid, string> item in _glamourerCollections.OrderBy<KeyValuePair<Guid, string>, string>((KeyValuePair<Guid, string> p) => p.Value))
				{
					if (!((ImGuiTextFilter)(ref _glamourerFilter)).PassFilter(ImU8String.op_Implicit(item.Value)))
					{
						continue;
					}
					ImU8String val3 = ImU8String.op_Implicit(item.Value);
					Guid key = item.Key;
					Guid? selectedGlamourer = _selectedGlamourer;
					if (ImGui.Selectable(val3, key == selectedGlamourer, (ImGuiSelectableFlags)0, default(Vector2)))
					{
						if (_selectedGlamourer.HasValue && _selectedGlamourer.Value == item.Key)
						{
							_selectedGlamourer = null;
						}
						else
						{
							_selectedGlamourer = item.Key;
						}
					}
				}
			}
			finally
			{
				((ListBoxDisposable)(ref val2)).Dispose();
			}
		}
		finally
		{
			((GroupDisposable)(ref val)).Dispose();
		}
		ImGui.SameLine();
		GroupDisposable val4 = ImRaii.Group();
		try
		{
			DisabledDisposable val5 = ImRaii.Disabled(!_selectedGlamourer.HasValue);
			try
			{
				ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.ipc.glamourer.design.select")));
				ImU8String val6 = new ImU8String(0, 1);
				((ImU8String)(ref val6)).AppendFormatted<string>((!_selectedGlamourer.HasValue) ? Ktisis.Locale.Translate("chara_edit.ipc.glamourer.design.none") : _glamourerCollections[_selectedGlamourer.Value]);
				ImGui.TextWrapped(val6);
				if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.ipc.glamourer.design.apply")), default(Vector2)))
				{
					glamourerIpc.ApplyDesignToObject(actor.Actor, _selectedGlamourer.Value);
					_selectedGlamourer = null;
				}
			}
			finally
			{
				((IDisposable)val5)?.Dispose();
			}
		}
		finally
		{
			((GroupDisposable)(ref val4)).Dispose();
		}
		ImGui.SameLine();
		ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - Buttons.CalcSize() - 3f);
		if (Buttons.IconButton((FontAwesomeIcon)61582))
		{
			_dpi.InstalledPlugins.FirstOrDefault((IExposedPlugin p) => p != null && p.InternalName == "Glamourer" && p.IsLoaded).OpenMainUi();
		}
	}

	private void ImportMcdf(ActorEntity actor, string path)
	{
		_ctx.Characters.Mcdf.LoadAndApplyTo(path, actor.Actor);
	}
}
