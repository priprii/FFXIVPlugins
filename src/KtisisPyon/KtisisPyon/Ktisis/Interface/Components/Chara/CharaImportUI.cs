using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Core.Attributes;
using Ktisis.Data.Files;
using Ktisis.Editor.Characters;
using Ktisis.Editor.Context.Types;
using Ktisis.GameData.Excel.Types;
using Ktisis.Interface.Components.Chara.Select;
using Ktisis.Interface.Components.Files;
using Ktisis.Scene.Entities.Game;

namespace Ktisis.Interface.Components.Chara;

[Transient]
public class CharaImportUI
{
	public Action<CharaImportUI>? OnNpcSelected;

	private readonly NpcSelect _npcs;

	private readonly FileSelect<CharaFile> _select;

	public IEditorContext Context { private get; set; }

	public LoadMethod Method { get; set; }

	public bool HasSelection => Method switch
	{
		LoadMethod.File => _select.IsFileOpened, 
		LoadMethod.Npc => _npcs.Selected != null, 
		_ => false, 
	};

	private bool DisableModes => Method switch
	{
		LoadMethod.File => !HasSelection, 
		LoadMethod.Npc => !HasSelection && !Context.Config.File.ImportNpcApplyOnSelect, 
		_ => false, 
	};

	public CharaImportUI(NpcSelect npcs, FileSelect<CharaFile> select)
	{
		_npcs = npcs;
		_npcs.OnSelected += OnNpcSelect;
		_select = select;
		FileSelect<CharaFile> fileSelect = _select;
		fileSelect.OnOpenDialog = (FileSelect<CharaFile>.OpenDialogHandler)Delegate.Combine(fileSelect.OnOpenDialog, new FileSelect<CharaFile>.OpenDialogHandler(OnFileDialogOpen));
	}

	private void OnNpcSelect(INpcBase _)
	{
		if (Context.Config.File.ImportNpcApplyOnSelect)
		{
			OnNpcSelected?.Invoke(this);
		}
	}

	private void OnFileDialogOpen(FileSelect<CharaFile> sender)
	{
		Context.Interface.OpenCharaFile(sender.SetFile);
	}

	public void ApplyTo(ActorEntity actor)
	{
		switch (Method)
		{
		case LoadMethod.File:
			ApplyCharaFile(actor);
			break;
		case LoadMethod.Npc:
			ApplyNpc(actor);
			break;
		default:
			throw new ArgumentOutOfRangeException(Method.ToString());
		}
	}

	private void ApplyCharaFile(ActorEntity actor)
	{
		CharaFile charaFile = _select.Selected?.File;
		if (charaFile != null)
		{
			Context.Characters.ApplyCharaFile(actor, charaFile, Context.Config.File.ImportCharaModes);
		}
	}

	public void ApplyNpc(ActorEntity actor)
	{
		INpcBase selected = _npcs.Selected;
		if (selected != null)
		{
			Context.Characters.ApplyNpc(actor, selected, Context.Config.File.ImportCharaModes);
		}
	}

	public void DrawImport()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		switch (Method)
		{
		case LoadMethod.File:
			_select.Draw();
			break;
		case LoadMethod.Npc:
			_npcs.Draw();
			ImGui.Spacing();
			ImGui.Checkbox(ImU8String.op_Implicit(Ktisis.Locale.Translate("file.chara.apply")), ref Context.Config.File.ImportNpcApplyOnSelect);
			break;
		default:
			throw new ArgumentOutOfRangeException(Method.ToString());
		}
	}

	public void DrawLoadMethods(float cursorY = -1f)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		bool num = cursorY > -1f;
		if (num)
		{
			ImGui.SetCursorPosY(cursorY);
		}
		DrawMethodRadio(Ktisis.Locale.Translate("file.chara.file"), LoadMethod.File);
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		if (num)
		{
			ImGui.SetCursorPosY(cursorY);
		}
		DrawMethodRadio("NPC", LoadMethod.Npc);
	}

	private void DrawMethodRadio(string label, LoadMethod method)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (ImGui.RadioButton(ImU8String.op_Implicit(label), Method == method))
		{
			Method = method;
		}
	}

	public void DrawModesSelect()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		DisabledDisposable val = ImRaii.Disabled(DisableModes);
		try
		{
			ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("common.appearance")));
			DrawModeSwitch(Ktisis.Locale.Translate("common.chara_parts.body"), SaveModes.AppearanceBody);
			ImGui.SameLine();
			DrawModeSwitch(Ktisis.Locale.Translate("common.chara_parts.face"), SaveModes.AppearanceFace);
			ImGui.SameLine();
			DrawModeSwitch(Ktisis.Locale.Translate("common.chara_parts.hair"), SaveModes.AppearanceHair);
			ImGui.Spacing();
			ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("common.equipment")));
			DrawModeSwitch(Ktisis.Locale.Translate("common.chara_parts.gear"), SaveModes.EquipmentGear);
			ImGui.SameLine();
			DrawModeSwitch(Ktisis.Locale.Translate("common.chara_parts.accessories"), SaveModes.EquipmentAccessories);
			ImGui.SameLine();
			DrawModeSwitch(Ktisis.Locale.Translate("common.chara_parts.weapons"), SaveModes.EquipmentWeapons);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DrawModeSwitch(string label, SaveModes mode)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		bool flag = Context.Config.File.ImportCharaModes.HasFlag(mode);
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(20, 2);
		((ImU8String)(ref val)).AppendFormatted<string>(label);
		((ImU8String)(ref val)).AppendLiteral("##CharaImportDialog_");
		((ImU8String)(ref val)).AppendFormatted<SaveModes>(mode);
		if (ImGui.Checkbox(val, ref flag))
		{
			Context.Config.File.ImportCharaModes ^= mode;
		}
	}
}
