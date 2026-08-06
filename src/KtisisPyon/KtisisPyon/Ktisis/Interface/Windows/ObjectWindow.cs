using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImGuizmo;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;
using GLib.Widgets;
using Ktisis.Common.Utility;
using Ktisis.Editor.Camera.Types;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Selection;
using Ktisis.Editor.Transforms;
using Ktisis.Editor.Transforms.Types;
using Ktisis.Interface.Components.Objects;
using Ktisis.Interface.Components.Transforms;
using Ktisis.Interface.Types;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Services.Game;

namespace Ktisis.Interface.Windows;

public class ObjectWindow : KtisisWindow
{
	private readonly IEditorContext _ctx;

	private readonly Gizmo2D _gizmo;

	private readonly GuiManager _gui;

	private readonly TransformTable _table;

	private readonly PropertyEditor _propEditor;

	private ITransformMemento? Transform;

	public ObjectWindow(IEditorContext ctx, Gizmo2D gizmo, GuiManager gui, TransformTable table, PropertyEditor propEditor)
		: base("object_edit.title", (ImGuiWindowFlags)0, "###KtisisObjectEditor")
	{
		_ctx = ctx;
		_gizmo = gizmo;
		_gui = gui;
		_table = table;
		_propEditor = propEditor;
	}

	public override void OnCreate()
	{
		_propEditor.Prepare(_ctx, _gui);
	}

	public override void PreOpenCheck()
	{
		if (!_ctx.IsValid)
		{
			Ktisis.Log.Verbose("Context for transform window is stale, closing...");
			Close();
		}
	}

	public override void PreDraw()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (_ctx.Config.Editor.UseToolbar)
		{
			((Window)this).Flags = (ImGuiWindowFlags)72;
		}
		else if (_ctx.Config.Editor.AutoResizeObjectEditor)
		{
			((Window)this).Flags = (ImGuiWindowFlags)8;
		}
		else
		{
			((Window)this).Flags = (ImGuiWindowFlags)0;
		}
		float num = TransformTable.CalcWidth();
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = num + ((ImGuiStylePtr)(ref style)).WindowPadding.X * 2f;
		WindowSizeConstraints value = default(WindowSizeConstraints);
		((WindowSizeConstraints)(ref value))._002Ector();
		((WindowSizeConstraints)(ref value)).MinimumSize = new Vector2(x, 0f);
		((Window)this).SizeConstraints = value;
	}

	public override void Draw()
	{
		ITransformTarget target = _ctx.Transform.Target;
		DrawToggles(target);
		DrawTransform(target);
		DrawProperties(target);
		if (_ctx.Config.Editor.AutoResizeObjectEditor)
		{
			Autoresize();
		}
	}

	public void DrawCompact()
	{
		ITransformTarget target = _ctx.Transform.Target;
		DrawToggles(target);
		DrawTransform(target);
	}

	private void DrawTransform(ITransformTarget? target)
	{
		Transform transform = target?.GetTransform() ?? new Transform();
		bool flag = target == null;
		DisabledDisposable val = ImRaii.Disabled(flag);
		try
		{
			bool isEnded;
			bool flag2 = DrawTransform(ref transform, out isEnded, flag);
			if (target != null && flag2)
			{
				if (Transform == null)
				{
					Transform = _ctx.Transform.Begin(target);
				}
				Transform.SetTransform(transform);
			}
			if (isEnded)
			{
				Transform?.Dispatch();
				Transform = null;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DrawProperties(ITransformTarget? target)
	{
		SceneEntity sceneEntity = _ctx.Selection.GetFirstSelected() ?? target?.Primary;
		if (sceneEntity != null)
		{
			((Window)this).WindowName = Ktisis.Locale.Translate(_localeWindowName) + " - " + sceneEntity.Name + _windowId;
			_propEditor.Draw(sceneEntity);
		}
	}

	private bool DrawTransform(ref Transform transform, out bool isEnded, bool disabled)
	{
		isEnded = false;
		bool flag = false;
		if (!_ctx.Config.Editor.TransformHide)
		{
			ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3f);
			flag = DrawGizmo(ref transform, ImGui.GetContentRegionAvail().X - (_ctx.Config.Editor.UseToolbar ? 3f : 0f), disabled);
			isEnded = _gizmo.IsEnded;
		}
		Transform transOut;
		bool flag2 = _table.Draw(transform, out transOut, TransformTableFlags.Default | TransformTableFlags.Operation | TransformTableFlags.UseAvailable);
		if (flag2)
		{
			transform = transOut;
		}
		isEnded |= _table.IsDeactivated;
		return flag || flag2;
	}

	private void DrawToggles(ITransformTarget? target)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Invalid comparison between Unknown and I4
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Invalid comparison between Unknown and I4
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Invalid comparison between Unknown and I4
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		float num = UiBuilder.DefaultFontSizePx * ImGuiHelpers.GlobalScale * 2f;
		Vector2 vector = new Vector2(num, num);
		ImGuizmoMode mode = _ctx.Config.Gizmo.Mode;
		int num2 = (((int)mode == 1) ? 61612 : 61461);
		string text = (((int)mode == 1) ? "world" : "local");
		string tooltip = _ctx.Locale.Translate("transform_edit.mode." + text);
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)num2, tooltip, vector))
		{
			_ctx.Config.Gizmo.Mode = (ImGuizmoMode)((int)mode != 1);
		}
		ImGui.SameLine(0f, x);
		bool visible = _ctx.Config.Gizmo.Visible;
		int num3 = (visible ? 61550 : 61552);
		string tooltip2 = _ctx.Locale.Translate("actions.Gizmo_Toggle");
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)num3, tooltip2, vector))
		{
			_ctx.Config.Gizmo.Visible = !visible;
		}
		ImGui.SameLine(0f, x);
		MirrorMode mirrorRotation = _ctx.Config.Gizmo.MirrorRotation;
		FontAwesomeIcon icon = (FontAwesomeIcon)63396;
		string text2 = "parallel";
		switch (mirrorRotation)
		{
		case MirrorMode.Inverse:
			icon = (FontAwesomeIcon)58543;
			text2 = "inverse";
			break;
		case MirrorMode.Reflect:
			icon = (FontAwesomeIcon)58554;
			text2 = "reflect";
			break;
		}
		string tooltip3 = _ctx.Locale.Translate("transform_edit.flags." + text2);
		if (Buttons.IconButtonTooltip(icon, tooltip3, vector))
		{
			_ctx.Config.Gizmo.SetNextMirrorRotation();
		}
		ImGui.SameLine(0f, x);
		SceneEntity sceneEntity = target?.Primary;
		int? num4 = target?.Targets.Count();
		if (num4 != 0 && sceneEntity != null && sceneEntity is BoneNode boneNode)
		{
			BoneNode boneNode2 = boneNode.Pose.TryResolveSibling(boneNode);
			bool flag = boneNode2 != null;
			string text3 = ((!flag) ? "unavailable" : ((num4 == 1) ? "available" : "multiple"));
			string tooltip4 = _ctx.Locale.Translate("transform_edit.sibling." + text3, new Dictionary<string, string> { 
			{
				"bone",
				flag ? boneNode2.Name : boneNode.Name
			} });
			DisabledDisposable val = ImRaii.Disabled(!flag || num4 != 1);
			try
			{
				if (Buttons.IconButtonTooltip((FontAwesomeIcon)57448, tooltip4, vector))
				{
					_ctx.Selection.Select(boneNode2, SelectMode.Multiple);
				}
				ImGui.SameLine(0f, x);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		else
		{
			ImGui.Dummy(vector);
			ImGui.SameLine(0f, x);
		}
		float num5 = ImGui.GetContentRegionAvail().X - (_ctx.Config.Editor.UseToolbar ? 3f : 0f);
		if (num5 > num)
		{
			ImGui.SetCursorPosX(ImGui.GetCursorPosX() + num5 - num);
		}
		bool transformHide = _ctx.Config.Editor.TransformHide;
		int num6 = (transformHide ? 61656 : 61655);
		string text4 = (transformHide ? "show" : "hide");
		string tooltip5 = _ctx.Locale.Translate("transform_edit.gizmo." + text4);
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)num6, tooltip5, vector))
		{
			_ctx.Config.Editor.TransformHide = !transformHide;
		}
	}

	private unsafe bool DrawGizmo(ref Transform transform, float width, bool disabled)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		Vector2 rectSize = new Vector2(width, 300f);
		_gizmo.Begin(rectSize);
		_gizmo.Mode = _ctx.Config.Gizmo.Mode;
		_gizmo.Operation = (ImGuizmoOperation)((((Enum)_ctx.Config.Gizmo.Operation).HasFlag((Enum)(object)(ImGuizmoOperation)8) && !((Enum)_ctx.Config.Gizmo.Operation).HasFlag((Enum)(object)(ImGuizmoOperation)64)) ? 56 : 120);
		if (disabled)
		{
			_gizmo.End();
			return false;
		}
		float num = 1f;
		Vector3 vector = Vector3.Zero;
		if (_ctx.Cameras.IsWorkCameraActive)
		{
			WorkCamera obj = (WorkCamera)_ctx.Cameras.Current;
			num = obj.Camera->RenderEx->FoV;
			vector = obj.Position;
		}
		else
		{
			Camera* gameCamera = CameraService.GetGameCamera();
			if (gameCamera != null)
			{
				num = ((Camera)gameCamera).FoV;
				vector = Vector3.op_Implicit(((Object)(&((Camera)(&((CameraBase)(&((Camera)gameCamera).CameraBase)).SceneCamera)).Object)).Position);
			}
		}
		Matrix4x4 matrix = transform.ComposeMatrix();
		Gizmo2D gizmo = _gizmo;
		Vector3 cameraPos = vector;
		Vector3 position = transform.Position;
		float fov = num;
		float x = rectSize.X;
		ImGuiStylePtr style = ImGui.GetStyle();
		float num2 = x - ((ImGuiStylePtr)(ref style)).WindowPadding.X * 2f;
		float y = rectSize.Y;
		style = ImGui.GetStyle();
		gizmo.SetLookAt(cameraPos, position, fov, num2 / (y - ((ImGuiStylePtr)(ref style)).WindowPadding.Y * 2f));
		Matrix4x4 delta;
		bool num3 = _gizmo.Manipulate(ref matrix, out delta);
		_gizmo.End();
		if (num3)
		{
			transform.DecomposeMatrixPrecise(matrix, transform);
		}
		return num3;
	}

	private void Autoresize()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ImGuiWindowPtr currentWindow = ImGuiP.GetCurrentWindow();
		float num = ((ImGuiWindowPtr)(ref currentWindow)).ContentSizeIdeal.Y - ImGuiP.GetHeight(ref ((ImGuiWindowPtr)(ref currentWindow)).ContentRegionRect);
		if (num != 0f)
		{
			ImGui.SetWindowSize(new Vector2(ImGui.GetWindowSize().X, ImGui.GetWindowSize().Y + num), (ImGuiCond)0);
		}
	}
}
