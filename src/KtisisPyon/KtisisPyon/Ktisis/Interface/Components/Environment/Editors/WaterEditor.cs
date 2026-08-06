using System;
using System.Threading;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using Ktisis.Core.Attributes;
using Ktisis.Scene.Modules;
using Ktisis.Services.Game;
using Ktisis.Structs.Env;

namespace Ktisis.Interface.Components.Environment.Editors;

[Singleton]
public class WaterEditor : EditorBase, IDisposable
{
	private GPoseService _gpose;

	private readonly IFramework _framework;

	private unsafe WaterRendererEx* _renderer;

	public bool Frozen;

	public override string Name => Ktisis.Locale.Translate("env_edit.water.title");

	public unsafe WaterEditor(IFramework framework, GPoseService gpose)
	{
		_framework = framework;
		_gpose = gpose;
		Manager* ptr = Manager.Instance();
		_renderer = (WaterRendererEx*)(&((Manager)ptr).WaterRenderer);
		_framework.RunOnTick((Action)delegate
		{
			_gpose.StateChanged += OnGPoseEvent;
			_gpose.Subscribe();
		}, default(TimeSpan), 1, default(CancellationToken));
	}

	public override bool IsActivated(EnvOverride flags)
	{
		return Frozen;
	}

	public override void Draw(IEnvModule module, ref EnvState state)
	{
	}

	public unsafe void Draw()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Checkbox(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.enable")), ref Frozen);
		ImGui.Spacing();
		DisabledDisposable val = ImRaii.Disabled(!Frozen);
		try
		{
			float unk = _renderer->Unk1;
			if (ImGui.DragFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.water.water_one")), ref unk, 1f, 0f, 0f, default(ImU8String), (ImGuiSliderFlags)0))
			{
				_renderer->Unk1 = unk;
			}
			float unk2 = _renderer->Unk2;
			if (ImGui.DragFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.water.water_two")), ref unk2, 1f, 0f, 0f, default(ImU8String), (ImGuiSliderFlags)0))
			{
				_renderer->Unk2 = unk2;
			}
			float unk3 = _renderer->Unk3;
			if (ImGui.DragFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.water.water_three")), ref unk3, 1f, 0f, 0f, default(ImU8String), (ImGuiSliderFlags)0))
			{
				_renderer->Unk3 = unk3;
			}
			float unk4 = _renderer->Unk4;
			if (ImGui.DragFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.water.water_four")), ref unk4, 1f, 0f, 0f, default(ImU8String), (ImGuiSliderFlags)0))
			{
				_renderer->Unk4 = unk4;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void OnGPoseEvent(object sender, bool active)
	{
		if (!active)
		{
			Frozen = false;
		}
	}

	public unsafe void Dispose()
	{
		_renderer = null;
		Frozen = false;
		_gpose.StateChanged -= OnGPoseEvent;
	}
}
