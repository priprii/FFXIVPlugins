using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility.Numerics;
using GLib.Popups;
using GLib.Widgets;
using Ktisis.Core.Attributes;
using Ktisis.GameData.Excel.Types;
using Ktisis.Localization;
using Ktisis.Services.Data;
using Ktisis.Structs.Characters;

namespace Ktisis.Interface.Components.Chara.Select;

[Transient]
public class NpcSelect
{
	private enum NpcLoadState
	{
		Waiting,
		Success,
		Failed
	}

	private readonly NpcService _npc;

	private readonly LocaleManager _locale;

	private readonly PopupList<INpcBase> _popup;

	private NpcLoadState _npcLoadState;

	private readonly List<INpcBase> _npcList = new List<INpcBase>();

	private List<INpcBase> _monsterList = new List<INpcBase>();

	public INpcBase? Selected { get; set; }

	public event OnNpcSelected? OnSelected;

	public NpcSelect(NpcService npc, LocaleManager locale)
	{
		_npc = npc;
		_locale = locale;
		_popup = new PopupList<INpcBase>("##NpcImportPopup", DrawItem).WithSearch(MatchQuery);
		Fetch();
	}

	public void Fetch()
	{
		if (_npcLoadState == NpcLoadState.Success)
		{
			return;
		}
		_npc.GetNpcList().ContinueWith(delegate(Task<IEnumerable<INpcBase>> task)
		{
			if (task.Exception != null)
			{
				Ktisis.Log.Error($"Failed to fetch NPC list:\n{task.Exception}");
				_npcLoadState = NpcLoadState.Failed;
			}
			else
			{
				_npcList.Clear();
				_npcList.AddRange(task.Result);
				_monsterList.Clear();
				_monsterList.AddRange(from entry in task.Result
					where entry.GetModelId() != 0
					orderby entry.GetModelId()
					select entry);
				_npcLoadState = NpcLoadState.Success;
			}
		});
	}

	public void Draw()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		switch (_npcLoadState)
		{
		case NpcLoadState.Waiting:
			ImGui.Text(ImU8String.op_Implicit("Loading NPCs..."));
			break;
		case NpcLoadState.Failed:
			ImGui.Text(ImU8String.op_Implicit("Failed to load NPCs.\nCheck your error log for more information."));
			break;
		case NpcLoadState.Success:
			DrawSelect();
			break;
		default:
			throw new InvalidEnumArgumentException($"Invalid value: {_npcLoadState}");
		}
		DisabledDisposable val = ImRaii.Disabled(Selected == null);
		try
		{
			ImGui.SameLine();
			if (Buttons.IconButton((FontAwesomeIcon)62186))
			{
				Selected = null;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void DrawSearchIcon()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		switch (_npcLoadState)
		{
		case NpcLoadState.Waiting:
			ImGui.Text(ImU8String.op_Implicit("Loading NPCs..."));
			break;
		case NpcLoadState.Failed:
			ImGui.Text(ImU8String.op_Implicit("Failed to load NPCs.\nCheck your error log for more information."));
			break;
		case NpcLoadState.Success:
		{
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)61442, "Browse NPCs..."))
			{
				_popup.Open();
			}
			float itemHeight = ImGui.GetFontSize() * 2f;
			if (_popup.Draw(_monsterList, out INpcBase selected, itemHeight) && selected != null)
			{
				Select(selected);
			}
			break;
		}
		default:
			throw new InvalidEnumArgumentException($"Invalid value: {_npcLoadState}");
		}
	}

	private void DrawSelect()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		string text = ((Selected != null) ? Selected.Name : "Select...");
		if (ImGui.BeginCombo(ImU8String.op_Implicit("##NpcCombo"), ImU8String.op_Implicit(text), (ImGuiComboFlags)0))
		{
			ImGui.CloseCurrentPopup();
			ImGui.EndCombo();
		}
		if (ImGui.IsItemActivated())
		{
			_popup.Open();
		}
		float itemHeight = ImGui.GetFontSize() * 2f;
		if (_popup.Draw(_npcList, out INpcBase selected, itemHeight) && selected != null)
		{
			Select(selected);
		}
	}

	private void Select(INpcBase npc)
	{
		Selected = npc;
		this.OnSelected?.Invoke(npc);
	}

	private bool DrawItem(INpcBase npc, bool isFocus)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float fontSize = ImGui.GetFontSize();
		bool result = ImGui.Selectable(ImU8String.op_Implicit("##"), isFocus, (ImGuiSelectableFlags)0, VectorExtensions.WithY(ImGui.GetContentRegionAvail(), fontSize * 2f));
		ImGui.SameLine(((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X, 0f);
		ImGui.Text(ImU8String.op_Implicit(npc.Name));
		ushort modelId = npc.GetModelId();
		ImGui.SameLine(((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X, 0f);
		ImGui.SetCursorPosY(ImGui.GetCursorPosY() + fontSize);
		if (modelId == 0)
		{
			CustomizeContainer? customize = npc.GetCustomize();
			if (customize.HasValue && customize.Value.Tribe != 0)
			{
				string text = ((customize.Value.Gender == Gender.Masculine) ? "♂" : "♀");
				string text2 = _locale.Translate($"{customize.Value.Tribe}");
				ImU8String val = default(ImU8String);
				((ImU8String)(ref val))._002Ector(1, 2);
				((ImU8String)(ref val)).AppendFormatted<string>(text);
				((ImU8String)(ref val)).AppendLiteral(" ");
				((ImU8String)(ref val)).AppendFormatted<string>(text2);
				ImGui.TextDisabled(val);
				return result;
			}
			ImGui.TextDisabled(ImU8String.op_Implicit("Unknown"));
			return result;
		}
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(7, 1);
		((ImU8String)(ref val2)).AppendLiteral("Model #");
		((ImU8String)(ref val2)).AppendFormatted<ushort>(modelId);
		ImGui.TextDisabled(val2);
		return result;
	}

	private static bool MatchQuery(INpcBase npc, string query)
	{
		return npc.Name.Contains(query, StringComparison.OrdinalIgnoreCase);
	}
}
