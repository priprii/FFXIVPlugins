using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Interface.Types;
using Ktisis.Scene.Entities;

namespace Ktisis.Interface.Editor.Popup;

public class EntityRenameModal(SceneEntity entity) : KtisisPopup("##EntityRename", (ImGuiWindowFlags)134217728)
{
	private bool _isFirstDraw = true;

	private string Name = entity.Name;

	protected override void OnDraw()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(10, 1);
		((ImU8String)(ref val)).AppendLiteral("Rename '");
		((ImU8String)(ref val)).AppendFormatted<string>(entity.Name);
		((ImU8String)(ref val)).AppendLiteral("':");
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
			if (ImGui.Button(ImU8String.op_Implicit("Confirm"), default(Vector2)))
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
		if (ImGui.Button(ImU8String.op_Implicit("Cancel"), default(Vector2)))
		{
			Close();
		}
	}

	private void Confirm()
	{
		entity.Name = Name;
		Close();
	}
}
