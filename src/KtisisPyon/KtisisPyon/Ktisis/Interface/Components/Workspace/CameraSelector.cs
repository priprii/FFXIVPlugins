using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using GLib.Widgets;
using Ktisis.Common.Extensions;
using Ktisis.Editor.Camera;
using Ktisis.Editor.Camera.Types;
using Ktisis.Editor.Context.Types;

namespace Ktisis.Interface.Components.Workspace;

public class CameraSelector
{
	private readonly IEditorContext _ctx;

	private bool _isOpen;

	private float _lastScroll;

	private ICameraManager Cameras => _ctx.Cameras;

	public CameraSelector(IEditorContext ctx)
	{
		_ctx = ctx;
	}

	public void Draw()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		IdDisposable val = ImRaii.PushId(ImU8String.op_Implicit("##CameraSelect"), true);
		try
		{
			ImGuiStylePtr style = ImGui.GetStyle();
			float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
			ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - (Buttons.CalcSize() + x) * 3f - (_ctx.Config.Editor.UseToolbar ? 3f : 0f));
			DrawSelector();
			ImGui.SameLine(0f, x);
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)61543, Ktisis.Locale.Translate("cameras.selector.create")))
			{
				Cameras.Create();
			}
			ImGui.SameLine(0f, x);
			EditorCamera current = Cameras.Current;
			bool flag = ((current != null && (current.IsDefault || current is WorkCamera)) ? true : false);
			bool flag2 = flag;
			if (!ImGui.IsKeyDown((ImGuiKey)642) || flag2)
			{
				if (Buttons.IconButtonTooltip((FontAwesomeIcon)62211, Ktisis.Locale.Translate("cameras.selector.edit") + (flag2 ? "" : (" " + Ktisis.Locale.Translate("cameras.selector.edit_can_delete")))))
				{
					_ctx.Interface.OpenCameraWindow();
				}
			}
			else if (Buttons.IconButtonTooltip((FontAwesomeIcon)61944, Ktisis.Locale.Translate("cameras.selector.delete")))
			{
				Cameras.DeleteCurrent();
			}
			ImGui.SameLine(0f, x);
			DrawFreecamToggle();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DrawFreecamToggle()
	{
		bool isWorkCameraActive = Cameras.IsWorkCameraActive;
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)21, ImGui.GetColorU32((ImGuiCol)23), isWorkCameraActive);
		try
		{
			ColorDisposable val2 = ImRaii.PushColor((ImGuiCol)0, ImGui.GetColorU32((ImGuiCol)0).SetAlpha(207), !isWorkCameraActive);
			try
			{
				if (Buttons.IconButtonTooltip((FontAwesomeIcon)61488, Ktisis.Locale.Translate("actions.Camera_Work_Toggle")))
				{
					Cameras.ToggleWorkCameraMode();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DrawSelector()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		DisabledDisposable val = ImRaii.Disabled(Cameras.IsWorkCameraActive);
		try
		{
			EditorCamera current = Cameras.Current;
			bool flag = ImGui.BeginCombo(ImU8String.op_Implicit("##CameraSelectList"), ImU8String.op_Implicit(current?.Name ?? "INVALID"), (ImGuiComboFlags)0);
			if (flag)
			{
				if (!_isOpen && _lastScroll > 0f)
				{
					ImGui.SetScrollY(_lastScroll);
				}
				foreach (EditorCamera camera in Cameras.GetCameras())
				{
					if (ImGui.Selectable(ImU8String.op_Implicit(camera.Name), camera == current, (ImGuiSelectableFlags)0, default(Vector2)))
					{
						Cameras.SetCurrent(camera);
					}
				}
				_lastScroll = ImGui.GetScrollY();
				ImGui.EndCombo();
			}
			_isOpen = flag;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
