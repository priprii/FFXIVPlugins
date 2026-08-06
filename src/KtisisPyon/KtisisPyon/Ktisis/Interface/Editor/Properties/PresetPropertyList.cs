using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using GLib.Widgets;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Editor.Properties.Types;
using Ktisis.Localization;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;

namespace Ktisis.Interface.Editor.Properties;

public class PresetPropertyList(IEditorContext ctx, LocaleManager locale) : ObjectPropertyList
{
	private string _name = "";

	public override void Invoke(IPropertyListBuilder builder, SceneEntity entity)
	{
		SceneEntity root = entity.Root;
		ActorEntity actor = root as ActorEntity;
		if (actor != null)
		{
			builder.AddHeader(locale.Translate("preset_edit.title"), delegate
			{
				DrawPresets(actor);
			}, 100);
		}
	}

	private void DrawPresets(ActorEntity actor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		_ = ref ((ImGuiStylePtr)(ref style)).ItemInnerSpacing;
		ImGui.Columns(2, default(ImU8String), true);
		foreach (var preset in actor.GetPresets())
		{
			string item = preset.name;
			byte item2 = (byte)preset.isEnabled;
			if (ImGui.CheckboxFlags<byte>(ImU8String.op_Implicit(item), ref item2, (byte)3))
			{
				actor.TogglePreset(item, item2 == 3);
			}
			ImGui.NextColumn();
		}
		if (ImGui.GetColumnIndex() == 1)
		{
			ImGui.NextColumn();
		}
		if (ImGui.Button(ImU8String.op_Implicit(locale.Translate("preset_edit.toggle_other")), default(Vector2)))
		{
			actor.ToggleOtherPreset(true);
		}
		ImGui.NextColumn();
		if (ImGui.Button(ImU8String.op_Implicit(locale.Translate("preset_edit.clear")), default(Vector2)))
		{
			actor.ClearVisibility();
		}
		ImGui.Columns(1, default(ImU8String), true);
		Separators.SeparatorText(ImU8String.op_Implicit(locale.Translate("preset_edit.add.title")), 0u, 0.3f, 5f, Separators.LineHeight.Bottom, ImGui.GetColorU32((ImGuiCol)24));
		ImGui.InputText(ImU8String.op_Implicit(locale.Translate("preset_edit.add.label")), ref _name, 512, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null);
		int num;
		if (_name.Length > 0)
		{
			num = ((!ctx.Config.Presets.Presets.ContainsKey(_name)) ? 1 : 0);
			if (num != 0 && ImGui.IsKeyPressed((ImGuiKey)525) && ImGui.IsItemDeactivated())
			{
				SavePreset(actor);
			}
		}
		else
		{
			num = 0;
		}
		DisabledDisposable val = ImRaii.Disabled(num == 0);
		try
		{
			if (ImGui.Button(ImU8String.op_Implicit(locale.Translate("preset_edit.add.save")), default(Vector2)))
			{
				SavePreset(actor);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void SavePreset(ActorEntity actor)
	{
		actor.SavePreset(_name);
		_name = "";
	}
}
