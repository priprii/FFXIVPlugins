using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using GLib.Widgets;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Transforms.Types;
using Ktisis.Interface.Widgets;
using Ktisis.Scene.Entities;

namespace Ktisis.Interface.Components.Workspace;

public class WorkspaceState
{
	private readonly IEditorContext _ctx;

	public WorkspaceState(IEditorContext ctx)
	{
		_ctx = ctx;
	}

	public void Draw()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float y = (ImGui.GetFontSize() + ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.Y) * 2f + ((ImGuiStylePtr)(ref style)).ItemSpacing.Y;
		float x = ImGui.GetContentRegionAvail().X - 6f;
		ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3f);
		ChildFrameDisposable val = ImRaii.ChildFrame(ImGui.GetID(ImU8String.op_Implicit("SceneState_Frame")), new Vector2(x, y));
		try
		{
			DrawContext();
			DrawShowAll();
			DrawOverlayToggle();
		}
		finally
		{
			((ChildFrameDisposable)(ref val)).Dispose();
		}
	}

	internal void DrawCompact()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float y = (ImGui.GetFontSize() + ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.Y) * 2f + ((ImGuiStylePtr)(ref style)).ItemSpacing.Y;
		float x = ImGui.GetContentRegionAvail().X - 6f;
		ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3f);
		ChildFrameDisposable val = ImRaii.ChildFrame(ImGui.GetID(ImU8String.op_Implicit("SceneState_Frame")), new Vector2(x, y));
		try
		{
			float cursorPosY = ImGui.GetCursorPosY();
			float y2 = ImGui.GetContentRegionAvail().Y;
			float cursorPosX = ImGui.GetCursorPosX();
			ImGuiStylePtr style2 = ImGui.GetStyle();
			ImGui.SetCursorPosX(cursorPosX + ((ImGuiStylePtr)(ref style2)).ItemSpacing.X);
			ImGui.SetCursorPosY(cursorPosY + (y2 - ImGui.GetFrameHeight()) / 2f);
			bool isEnabled = _ctx.Posing.IsEnabled;
			bool flag = ImGui.IsKeyDown((ImGuiKey)642);
			bool flag2 = _ctx.Config.Editor.ConfirmExit && isEnabled && !flag;
			string text = ((!isEnabled) ? "disable" : (flag2 ? "enable-blocked" : "enable"));
			string text2 = text;
			if (flag2)
			{
				ImGui.BeginDisabled();
			}
			ColorDisposable val2 = ImRaii.PushColor((ImGuiCol)21, isEnabled ? 4278255360u : 4285558976u, true);
			try
			{
				if (ImGui.IsItemHovered((ImGuiHoveredFlags)128))
				{
					TooltipDisposable val3 = ImRaii.Tooltip();
					try
					{
						ImGui.Text(ImU8String.op_Implicit(_ctx.Locale.Translate("workspace.posing.hint." + text2)));
					}
					finally
					{
						((TooltipDisposable)(ref val3)).Dispose();
					}
				}
				if (flag2)
				{
					ImGui.EndDisabled();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((ChildFrameDisposable)(ref val)).Dispose();
		}
	}

	private void DrawContext()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		float cursorPosY = ImGui.GetCursorPosY();
		float y = ImGui.GetContentRegionAvail().Y;
		float cursorPosX = ImGui.GetCursorPosX();
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SetCursorPosX(cursorPosX + ((ImGuiStylePtr)(ref style)).ItemSpacing.X);
		ImGui.SetCursorPosY(cursorPosY + (y - ImGui.GetFrameHeight()) / 2f);
		bool v = _ctx.Posing.IsEnabled;
		bool flag = ImGui.IsKeyDown((ImGuiKey)642);
		bool flag2 = _ctx.Config.Editor.ConfirmExit && v && !flag;
		string text = ((!v) ? "disable" : (flag2 ? "enable-blocked" : "enable"));
		string text2 = text;
		if (flag2)
		{
			ImGui.BeginDisabled();
		}
		uint num = (v ? 4282046570u : 4283453124u);
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)21, v ? 4278255360u : 4285558976u, true);
		try
		{
			if (ToggleButton.Draw("##KtisisPoseToggle", ref v, num))
			{
				_ctx.Posing.SetEnabled(v);
			}
			if (ImGui.IsItemHovered((ImGuiHoveredFlags)128))
			{
				TooltipDisposable val2 = ImRaii.Tooltip();
				try
				{
					ImGui.Text(ImU8String.op_Implicit(_ctx.Locale.Translate("workspace.posing.hint." + text2)));
				}
				finally
				{
					((TooltipDisposable)(ref val2)).Dispose();
				}
			}
			if (flag2)
			{
				ImGui.EndDisabled();
			}
			ImGui.SameLine();
			ImGuiStylePtr style2 = ImGui.GetStyle();
			float num2 = (UiBuilder.DefaultFontSizePx * 2f + ((ImGuiStylePtr)(ref style2)).ItemInnerSpacing.Y) * ImGuiHelpers.GlobalScale;
			ImGui.SetCursorPosY(cursorPosY + (y - num2) / 2f);
			ImGui.BeginGroup();
			StyleDisposable val3 = ImRaii.PushStyle((ImGuiStyleVar)13, Vector2.Zero, true);
			try
			{
				ColorDisposable val4 = ImRaii.PushColor((ImGuiCol)0, num, true);
				try
				{
					ImGui.Text(ImU8String.op_Implicit(_ctx.Locale.Translate("workspace.posing.toggle." + text2)));
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
				ColorDisposable val5 = ImRaii.PushColor((ImGuiCol)0, 3758096383u, true);
				try
				{
					DrawTargetLabel(_ctx.Transform);
				}
				finally
				{
					((IDisposable)val5)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
			ImGui.EndGroup();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DrawTargetLabel(ITransformHandler transform)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		ITransformTarget target = transform.Target;
		if (target == null)
		{
			ImGui.TextDisabled(ImU8String.op_Implicit(_ctx.Locale.Translate("workspace.state.select_count.none")));
			return;
		}
		string name = target.Primary?.Name ?? "INVALID";
		int num = transform.Target.Targets.Count();
		if (num == 1)
		{
			ImGui.Text(ImU8String.op_Implicit(name));
			return;
		}
		num--;
		string handle = "workspace.state.select_count." + ((num > 1) ? "plural" : "single");
		ImGui.Text(ImU8String.op_Implicit(_ctx.Locale.Translate(handle, new Dictionary<string, string>
		{
			{
				"count",
				num.ToString()
			},
			{
				"target",
				target.Primary?.Name ?? "INVALID"
			}
		})));
		List<SceneEntity> list = transform.Target.Targets.Where((SceneEntity tar) => tar.Name != name).ToList();
		if (!ImGui.IsItemHovered())
		{
			return;
		}
		TooltipDisposable val = ImRaii.Tooltip();
		try
		{
			for (int num2 = 0; num2 < num; num2++)
			{
				ImU8String val2 = new ImU8String(0, 1);
				((ImU8String)(ref val2)).AppendFormatted<string>(list[num2].Name);
				ImGui.Text(val2);
			}
		}
		finally
		{
			((TooltipDisposable)(ref val)).Dispose();
		}
	}

	private void DrawShowAll()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		IdDisposable val = ImRaii.PushId(ImU8String.op_Implicit("##OverlayBulkVisButton"), true);
		try
		{
			ColorDisposable val2 = ImRaii.PushColor((ImGuiCol)21, 0u, true);
			try
			{
				ImGui.SameLine();
				bool bulkVisOverride = _ctx.Config.Overlay.BulkVisOverride;
				ColorDisposable val3 = ImRaii.PushColor((ImGuiCol)0, bulkVisOverride ? 4026531839u : 2164260863u, true);
				try
				{
					string tooltip = (bulkVisOverride ? "Hide All Bones" : "Show All Bones");
					Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
					float num = contentRegionAvail.Y - ImGui.GetCursorPosY() / 2f;
					float num2 = ImGui.GetCursorPosX() + contentRegionAvail.X - num * 2f;
					ImGuiStylePtr style = ImGui.GetStyle();
					ImGui.SetCursorPosX(num2 - ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
					if (Buttons.IconButtonTooltip((FontAwesomeIcon)58594, tooltip, new Vector2(num, num)))
					{
						_ctx.Config.Overlay.BulkVisOverride = !bulkVisOverride;
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
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

	private void DrawOverlayToggle()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		IdDisposable val = ImRaii.PushId(ImU8String.op_Implicit("##OverlayToggleButton"), true);
		try
		{
			ColorDisposable val2 = ImRaii.PushColor((ImGuiCol)21, 0u, true);
			try
			{
				ImGui.SameLine();
				bool visible = _ctx.Config.Overlay.Visible;
				ColorDisposable val3 = ImRaii.PushColor((ImGuiCol)0, visible ? 4026531839u : 2164260863u, true);
				try
				{
					int num = (visible ? 61550 : 61552);
					string tooltip = (visible ? _ctx.Locale.Translate("workspace.overlay.hide") : _ctx.Locale.Translate("workspace.overlay.show"));
					float num2 = ImGui.GetContentRegionAvail().Y - ImGui.GetCursorPosY() / 2f;
					ImGuiStylePtr style = ImGui.GetStyle();
					ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
					if (Buttons.IconButtonTooltip((FontAwesomeIcon)num, tooltip, new Vector2(num2, num2)))
					{
						_ctx.Config.Overlay.Visible = !visible;
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
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
}
