using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Ktisis.Editor.Animation.Types;
using Ktisis.Editor.Characters.State;
using Ktisis.Editor.Characters.Types;
using Ktisis.Editor.Context.Types;
using Ktisis.GameData.Excel.Types;
using Ktisis.Interface.Components.Chara;
using Ktisis.Interface.Components.Chara.Select;
using Ktisis.Interface.Types;
using Ktisis.Scene.Entities.Game;
using Ktisis.Structs.Actors;
using Ktisis.Structs.Characters;

namespace Ktisis.Interface.Windows.Editors;

public class ActorWindow : EntityEditWindow<ActorEntity>
{
	private readonly CustomizeEditorTab _custom;

	private readonly EquipmentEditorTab _equip;

	private readonly AnimationEditorTab _anim;

	private readonly PluginDataEditorTab _ipc;

	private readonly NpcSelect _npcs;

	private ICustomizeEditor _editCustom;

	private IAnimationManager Animation => Context.Animation;

	private ICharacterManager Manager => Context.Characters;

	public ActorWindow(IEditorContext ctx, CustomizeEditorTab custom, EquipmentEditorTab equip, AnimationEditorTab anim, NpcSelect npcs, IDalamudPluginInterface dpi)
		: base("chara_edit.title", ctx, (ImGuiWindowFlags)0, "###KtisisActorEditor")
	{
		_custom = custom;
		_equip = equip;
		_anim = anim;
		_ipc = new PluginDataEditorTab(ctx, dpi);
		_npcs = npcs;
		_npcs.OnSelected += OnNpcSelect;
	}

	public override void PreOpenCheck()
	{
		if (!Context.IsValid)
		{
			Ktisis.Log.Verbose("Context for actor window is stale, closing...");
			Close();
		}
	}

	public override void SetTarget(ActorEntity target)
	{
		((Window)this).WindowName = Ktisis.Locale.Translate(_localeWindowName) + " - " + target.Name + _windowId;
		base.SetTarget(target);
		ICustomizeEditor editCustom = (_custom.Editor = Manager.GetCustomizeEditor(target));
		_editCustom = editCustom;
		_equip.Editor = Manager.GetEquipmentEditor(target);
		_anim.Editor = Animation.GetAnimationEditor(target);
		_ipc.SetTarget(target);
		_anim.ClearPoseExpression();
	}

	public override void OnOpen()
	{
		_custom.Setup(Context);
		_anim.Setup();
	}

	public override void PreDraw()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		base.PreDraw();
		WindowSizeConstraints value = default(WindowSizeConstraints);
		((WindowSizeConstraints)(ref value))._002Ector();
		((WindowSizeConstraints)(ref value)).MinimumSize = new Vector2(560f, 380f);
		ImGuiIOPtr iO = ImGui.GetIO();
		((WindowSizeConstraints)(ref value)).MaximumSize = ((ImGuiIOPtr)(ref iO)).DisplaySize * 0.9f;
		((Window)this).SizeConstraints = value;
	}

	public override void Draw()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		UpdateTarget();
		TabBarDisposable val = ImRaii.TabBar(ImU8String.op_Implicit("##ActorEditTabs"));
		try
		{
			DrawTab(Ktisis.Locale.Translate("chara_edit.animation.tab"), _anim.Draw);
			DrawTab(Ktisis.Locale.Translate("chara_edit.customize.tab"), _custom.Draw);
			DrawTab(Ktisis.Locale.Translate("chara_edit.equip.tab"), _equip.Draw);
			DrawTab(Ktisis.Locale.Translate("chara_edit.ipc.tab"), _ipc.Draw);
			DrawTab(Ktisis.Locale.Translate("chara_edit.misc.tab"), DrawMisc);
		}
		finally
		{
			((TabBarDisposable)(ref val)).Dispose();
		}
	}

	private static void DrawTab(string name, Action draw)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		TabItemDisposable val = ImRaii.TabItem(ImU8String.op_Implicit(name));
		try
		{
			if (val.Success)
			{
				draw();
			}
		}
		finally
		{
			((TabItemDisposable)(ref val)).Dispose();
		}
	}

	private unsafe void DrawMisc()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		ImGui.Spacing();
		uint modelId = _editCustom.GetModelId();
		if (ImGui.InputUInt(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.misc.model")), ref modelId, 1u, 0u, default(ImU8String), (ImGuiInputTextFlags)32))
		{
			_editCustom.SetModelId(modelId);
		}
		ImGui.SameLine(0f, x);
		_npcs.DrawSearchIcon();
		CharacterEx* character = (CharacterEx*)base.Target.Character;
		if (character != null)
		{
			ImGui.Spacing();
			ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.misc.opacity")), ref character->Opacity, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0);
		}
		ImGui.Spacing();
		ImGui.Spacing();
		DrawWetness();
	}

	private void DrawWetness()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		bool hasValue = base.Target.Appearance.Wetness.HasValue;
		if (ImGui.Checkbox(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.misc.wetness")), ref hasValue))
		{
			ToggleWetness();
		}
		WetnessState? wetness = GetWetness();
		if (!wetness.HasValue)
		{
			return;
		}
		DisabledDisposable val = ImRaii.Disabled(!hasValue);
		try
		{
			ImGui.Spacing();
			WetnessState value = wetness.Value;
			if ((0u | (ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.misc.wetness.weather")), ref value.WeatherWetness, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0) ? 1u : 0u) | (ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.misc.wetness.swim")), ref value.SwimmingWetness, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0) ? 1u : 0u) | (ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.misc.wetness.depth")), ref value.WetnessDepth, 0f, 3f, default(ImU8String), (ImGuiSliderFlags)0) ? 1u : 0u)) != 0)
			{
				base.Target.Appearance.Wetness = value;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private unsafe WetnessState? GetWetness()
	{
		WetnessState? wetness = base.Target.Appearance.Wetness;
		if (wetness.HasValue)
		{
			return wetness.GetValueOrDefault();
		}
		CharacterBaseEx* characterBaseEx = base.Target.CharacterBaseEx;
		if (characterBaseEx == null)
		{
			return null;
		}
		return characterBaseEx->Wetness;
	}

	private unsafe void ToggleWetness()
	{
		AppearanceState appearance = base.Target.Appearance;
		if (appearance.Wetness.HasValue)
		{
			appearance.Wetness = null;
			return;
		}
		CharacterBaseEx* characterBaseEx = base.Target.CharacterBaseEx;
		appearance.Wetness = ((characterBaseEx != null) ? new WetnessState?(characterBaseEx->Wetness) : ((WetnessState?)null));
	}

	private void OnNpcSelect(INpcBase npc)
	{
		_editCustom.SetModelId(npc.GetModelId());
	}
}
