using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Interface.Types;
using Ktisis.Localization;
using Ktisis.Scene.Entities.Game;

namespace Ktisis.Interface.Editor.Popup;

public class PresetSaveModal(ActorEntity entity, LocaleManager locale) : KtisisPopup("##PresetSave", (ImGuiWindowFlags)134217728)
{
	private bool _isFirstDraw = true;

	private string Name = "";

	protected override void OnDraw()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(13, 0);
		((ImU8String)(ref val)).AppendLiteral("Save Preset':");
		ImGui.Text(val);
		ImGui.InputText(ImU8String.op_Implicit("##NameInput"), ref Name, 100, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null);
		bool num = Name.Length > 0;
		if (num && ImGui.IsKeyPressed((ImGuiKey)525) && ImGui.IsItemDeactivated())
		{
			Confirm();
		}
		if (_isFirstDraw)
		{
			_isFirstDraw = false;
			ImGui.SetKeyboardFocusHere(-1);
		}
		ImGui.Spacing();
		DisabledDisposable val2 = ImRaii.Disabled(!num);
		try
		{
			if (ImGui.Button(ImU8String.op_Implicit(locale.Translate("preset_edit.add.save")), default(Vector2)))
			{
				Confirm();
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		if (ImGui.Button(ImU8String.op_Implicit(locale.Translate("preset_edit.add.cancel")), default(Vector2)))
		{
			Close();
		}
	}

	private void Confirm()
	{
		entity.SavePreset(Name);
		Close();
	}
}
