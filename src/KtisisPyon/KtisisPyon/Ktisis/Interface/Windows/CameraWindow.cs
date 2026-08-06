using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;
using GLib.Popups;
using GLib.Widgets;
using Ktisis.Common.Extensions;
using Ktisis.Common.Utility;
using Ktisis.Editor.Camera;
using Ktisis.Editor.Camera.Types;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Components.Transforms;
using Ktisis.Interface.Types;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Scene.Types;
using Ktisis.Structs.Camera;

namespace Ktisis.Interface.Windows;

public class CameraWindow : KtisisWindow
{
	private readonly IEditorContext _ctx;

	private readonly TransformTable _fixedPos;

	private readonly TransformTable _relativePos;

	private bool IsWork;

	private float _toolbar;

	private readonly PopupList<BoneNode> _boneList;

	private BoneNode? _selected;

	private BoneNode? _previouslyDrawn;

	private List<BoneNode> tracked;

	private const TransformTableFlags TransformFlags = TransformTableFlags.Default | TransformTableFlags.UseAvailable;

	public CameraWindow(IEditorContext ctx, TransformTable fixedPos, TransformTable relativePos)
		: base("camera_edit.title", (ImGuiWindowFlags)0, "###KtisisCameraEditor")
	{
		_ctx = ctx;
		_fixedPos = fixedPos;
		_relativePos = relativePos;
		_toolbar = (_ctx.Config.Editor.UseToolbar ? 3f : 0f);
		_boneList = new PopupList<BoneNode>("##BoneList", DrawBoneSelect).WithSearch(BoneSearchPredicate);
	}

	public override void PreOpenCheck()
	{
		IEditorContext ctx = _ctx;
		if (ctx != null && ctx.IsValid)
		{
			ICameraManager cameras = ctx.Cameras;
			if (cameras != null && cameras.Current != null)
			{
				return;
			}
		}
		Ktisis.Log.Verbose("State for camera window is stale, closing.");
		Close();
	}

	public override void PreDraw()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		((Window)this).SizeCondition = (ImGuiCond)1;
		WindowSizeConstraints value = default(WindowSizeConstraints);
		((WindowSizeConstraints)(ref value))._002Ector();
		((WindowSizeConstraints)(ref value)).MinimumSize = new Vector2(TransformTable.CalcWidth(), 300f);
		ImGuiIOPtr iO = ImGui.GetIO();
		((WindowSizeConstraints)(ref value)).MaximumSize = ((ImGuiIOPtr)(ref iO)).DisplaySize * 0.75f;
		((Window)this).SizeConstraints = value;
		IsWork = _ctx.Cameras.IsWorkCameraActive;
		((Window)this).WindowName = Ktisis.Locale.Translate(_localeWindowName) + (IsWork ? " [Work Camera]" : "") + _windowId;
	}

	public override void Draw()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		EditorCamera current = _ctx.Cameras.Current;
		if (current == null || !current.IsValid)
		{
			return;
		}
		DrawToggles(current);
		DisabledDisposable val = ImRaii.Disabled(IsWork);
		try
		{
			ImGui.Spacing();
			ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - _toolbar);
			ImGui.InputText(ImU8String.op_Implicit("##CameraName"), ref current.Name, 64, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null);
			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();
			DrawOrbitTarget(current);
			ImGui.Spacing();
			if (!current.IsTracking)
			{
				DrawFixedPosition(current);
			}
			else
			{
				DrawTracking(current);
			}
			DrawRelativeOffset(current);
			ImGui.Spacing();
			DrawAnglePan(current);
			ImGui.Spacing();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		ImGui.Spacing();
		DrawSliders(current);
		if (current is WorkCamera)
		{
			ImGui.Spacing();
			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();
			DrawFreeCamOptions(current);
		}
	}

	private void DrawFreeCamOptions(EditorCamera camera)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Text(ImU8String.op_Implicit("Work camera options"));
		DrawSliderFloat("##workCamSpeed", (FontAwesomeIcon)61518, ref _ctx.Config.Editor.WorkcamMoveSpeed, 0.01f, 5f, 0.01f, _ctx.Locale.Translate("config.workspace.workcam.speed"));
		DrawSliderFloat("##workCamFastMulti", (FontAwesomeIcon)61816, ref _ctx.Config.Editor.WorkcamFastMulti, 0.01f, 5f, 0.01f, _ctx.Locale.Translate("config.workspace.workcam.fastMulti"));
		DrawSliderFloat("##workCamSlowMulti", (FontAwesomeIcon)61537, ref _ctx.Config.Editor.WorkcamSlowMulti, 0.01f, 5f, 0.01f, _ctx.Locale.Translate("config.workspace.workcam.slowMulti"));
		DrawSliderFloat("##workCamVertMulti", (FontAwesomeIcon)61565, ref _ctx.Config.Editor.WorkcamVertMulti, 0.01f, 5f, 0.01f, _ctx.Locale.Translate("config.workspace.workcam.vertMulti"));
		DrawSliderFloat("##workCamSens", (FontAwesomeIcon)61541, ref _ctx.Config.Editor.WorkcamSens, 0.01f, 5f, 0.01f, _ctx.Locale.Translate("config.workspace.workcam.sens"));
	}

	private void DrawToggles(EditorCamera camera)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		bool flag = !camera.Flags.HasFlag(CameraFlags.NoCollide) && !IsWork;
		DisabledDisposable val = ImRaii.Disabled(IsWork);
		try
		{
			if (ImGui.Checkbox(ImU8String.op_Implicit(_ctx.Locale.Translate("camera_edit.toggles.collide")), ref flag))
			{
				camera.Flags ^= CameraFlags.NoCollide;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		ImGui.SameLine();
		bool delimited = camera.Flags.HasFlag(CameraFlags.Delimit);
		if (ImGui.Checkbox(ImU8String.op_Implicit(_ctx.Locale.Translate("camera_edit.toggles.delimit")), ref delimited))
		{
			camera.SetDelimited(delimited);
		}
		DrawOrthographicToggle(camera);
	}

	private unsafe void DrawOrthographicToggle(EditorCamera camera)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (camera.Camera != null && camera.Camera->RenderEx != null)
		{
			ImGui.SameLine();
			bool isOrthographic = camera.IsOrthographic;
			if (ImGui.Checkbox(ImU8String.op_Implicit(_ctx.Locale.Translate("camera_edit.toggles.ortho")), ref isOrthographic))
			{
				camera.SetOrthographic(isOrthographic);
			}
		}
	}

	private unsafe void DrawOrbitTarget(EditorCamera camera)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		IdDisposable val = ImRaii.PushId(ImU8String.op_Implicit("CameraOrbitTarget"), true);
		try
		{
			IGameObject val2 = _ctx.Cameras.ResolveOrbitTarget(camera);
			if (val2 == null)
			{
				return;
			}
			bool hasValue = camera.OrbitTarget.HasValue;
			int num = (hasValue ? 61475 : 61596);
			string tooltip = (hasValue ? _ctx.Locale.Translate("camera_edit.orbit.unlock") : _ctx.Locale.Translate("camera_edit.orbit.lock"));
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)num, tooltip))
			{
				camera.OrbitTarget = (hasValue ? ((ushort?)null) : new ushort?(val2.ObjectIndex));
			}
			ImGui.SameLine();
			string tooltip2 = "Turn camera tracking " + (camera.IsTracking ? "off" : "on");
			Vector4? iconColor = (camera.IsTracking ? (*ImGui.GetStyleColorVec4((ImGuiCol)35)) : (*ImGui.GetStyleColorVec4((ImGuiCol)0)));
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)58557, tooltip2, null, iconColor))
			{
				camera.IsTracking = !camera.IsTracking;
			}
			ImGui.SameLine();
			if (!camera.IsTracking)
			{
				string text = "Orbiting: " + val2.GetNameOrFallback(_ctx);
				if (hasValue)
				{
					ImGui.Text(ImU8String.op_Implicit(text));
				}
				else
				{
					ImGui.TextDisabled(ImU8String.op_Implicit(text));
				}
				ImGui.SameLine(0f, 0f);
				ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - Buttons.CalcSize() - _toolbar);
				if (Buttons.IconButtonTooltip((FontAwesomeIcon)61473, _ctx.Locale.Translate("camera_edit.offset.to_target")))
				{
					GameObject* address = (GameObject*)val2.Address;
					DrawObject* drawObject = ((GameObject)address).DrawObject;
					if (drawObject != null)
					{
						camera.RelativeOffset = Vector3.op_Implicit(((Object)(&((DrawObject)drawObject).Object)).Position - ((GameObject)address).Position);
					}
				}
				return;
			}
			ImU8String val3 = new ImU8String(15, 1);
			((ImU8String)(ref val3)).AppendLiteral("Tracking mode: ");
			((ImU8String)(ref val3)).AppendFormatted<TrackingMode>(camera.Tracking);
			ImGui.Text(val3);
			ImGui.SameLine(0f, 0f);
			ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - Buttons.CalcSize() - _toolbar);
			string text2 = "";
			TrackingMode tracking = TrackingMode.None;
			switch (camera.Tracking)
			{
			case TrackingMode.Follow:
				text2 = "2";
				tracking = TrackingMode.Pan;
				break;
			case TrackingMode.FollowAndPan:
				text2 = "0";
				tracking = TrackingMode.None;
				break;
			case TrackingMode.Pan:
				text2 = "3";
				tracking = TrackingMode.FollowAndPan;
				break;
			case TrackingMode.None:
				text2 = "1";
				tracking = TrackingMode.Follow;
				break;
			}
			if (ImGui.Button(ImU8String.op_Implicit(text2), Vector2.Create(Buttons.CalcSize())))
			{
				camera.Tracking = tracking;
			}
			if (!ImGui.IsItemHovered())
			{
				return;
			}
			TooltipDisposable val4 = ImRaii.Tooltip();
			try
			{
				ImGui.Text(ImU8String.op_Implicit(tracking.ToString()));
			}
			finally
			{
				((TooltipDisposable)(ref val4)).Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DrawFixedPosition(EditorCamera camera)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		IdDisposable val = ImRaii.PushId(ImU8String.op_Implicit("CameraFixedPosition"), true);
		try
		{
			Vector3? position = camera.GetPosition();
			if (!position.HasValue)
			{
				return;
			}
			Vector3 position2 = position.Value;
			bool hasValue = camera.FixedPosition.HasValue;
			if (!hasValue)
			{
				position2 -= camera.RelativeOffset;
			}
			int num = (hasValue ? 61475 : 61596);
			string tooltip = (hasValue ? _ctx.Locale.Translate("camera_edit.position.unlock") : _ctx.Locale.Translate("camera_edit.position.lock"));
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)num, tooltip))
			{
				camera.FixedPosition = (hasValue ? ((Vector3?)null) : new Vector3?(position2));
			}
			ImGuiStylePtr style = ImGui.GetStyle();
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			DisabledDisposable val2 = ImRaii.Disabled(!hasValue);
			try
			{
				if (_fixedPos.DrawPosition(ref position2, TransformTableFlags.Default | TransformTableFlags.UseAvailable))
				{
					camera.FixedPosition = position2;
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

	private void DrawRelativeOffset(EditorCamera camera)
	{
		DrawIconAlign((FontAwesomeIcon)61543, out var spacing, _ctx.Locale.Translate("camera_edit.offset.from_base"));
		ImGui.SameLine(0f, spacing);
		_relativePos.DrawPosition(ref camera.RelativeOffset, TransformTableFlags.Default | TransformTableFlags.UseAvailable);
	}

	private unsafe void DrawAnglePan(EditorCamera camera)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		GameCameraEx* camera2 = camera.Camera;
		if (camera2 != null)
		{
			string hint = _ctx.Locale.Translate("camera_edit.angle");
			DrawIconAlign((FontAwesomeIcon)58555, out var spacing, hint);
			ImGui.SameLine(0f, spacing);
			ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - _toolbar);
			Vector2 vector = camera2->Angle * MathHelpers.Rad2Deg;
			if (ImGui.DragFloat2(ImU8String.op_Implicit("##CameraAngle"), ref vector, 0.25f, 0f, 0f, default(ImU8String), (ImGuiSliderFlags)0))
			{
				camera2->Angle = vector * MathHelpers.Deg2Rad;
			}
			string hint2 = _ctx.Locale.Translate("camera_edit.pan");
			DrawIconAlign((FontAwesomeIcon)61618, out spacing, hint2);
			ImGui.SameLine(0f, spacing);
			ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - _toolbar);
			Vector2 vector2 = camera2->Pan * MathHelpers.Rad2Deg;
			if (ImGui.DragFloat2(ImU8String.op_Implicit("##CameraPan"), ref vector2, 0.25f, 0f, 0f, default(ImU8String), (ImGuiSliderFlags)0))
			{
				vector2.X %= 360f;
				vector2.Y %= 360f;
				camera2->Pan = vector2 * MathHelpers.Deg2Rad;
			}
		}
	}

	private void DrawTracking(EditorCamera camera)
	{
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		tracked = camera.Target;
		_previouslyDrawn = null;
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)61543, _ctx.Locale.Translate("camera_edit.tracking.info")))
		{
			if (!ImGui.IsKeyDown((ImGuiKey)642))
			{
				_boneList.Open();
			}
			else
			{
				camera.Target.Clear();
			}
		}
		ImGui.SameLine();
		DisabledDisposable val = ImRaii.Disabled(_ctx.Selection.GetSelected().Count((SceneEntity e) => e.Type == EntityType.BoneNode) == 0);
		try
		{
			if (Buttons.IconButton((FontAwesomeIcon)62307))
			{
				camera.Target.Clear();
				foreach (BoneNode item in from e in _ctx.Selection.GetSelected()
					where e.Type == EntityType.BoneNode
					select e)
				{
					camera.Target.Add(item);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		if (ImGui.IsItemHovered())
		{
			TooltipDisposable val2 = ImRaii.Tooltip();
			try
			{
				ImGui.Text(ImU8String.op_Implicit(_ctx.Locale.Translate("camera_edit.tracking.track")));
			}
			finally
			{
				((TooltipDisposable)(ref val2)).Dispose();
			}
		}
		if (_boneList.IsOpen)
		{
			List<BoneNode> list = new List<BoneNode>();
			foreach (SceneEntity child in _ctx.Scene.Children)
			{
				list.AddRange(from e in child.Recurse().OfType<BoneNode>()
					where e.Type == EntityType.BoneNode
					select e);
			}
			_boneList.Draw(list, out _selected);
		}
		if (_selected != null)
		{
			if (camera.Target.Contains(_selected))
			{
				camera.Target.Remove(_selected);
			}
			else
			{
				camera.Target.Add(_selected);
			}
			_selected = null;
		}
		ImGui.SameLine();
		ImGui.Text(ImU8String.op_Implicit(_ctx.Locale.Translate("camera_edit.tracking.current")));
		if (camera.Target.Count == 0)
		{
			ImGui.SameLine();
			ImGui.Text(ImU8String.op_Implicit("N/A"));
			return;
		}
		if (camera.Target.Count == 1)
		{
			ImGui.SameLine();
			ImU8String val3 = default(ImU8String);
			((ImU8String)(ref val3))._002Ector(4, 2);
			((ImU8String)(ref val3)).AppendFormatted<string>(camera.Target[0].Name);
			((ImU8String)(ref val3)).AppendLiteral(" on ");
			((ImU8String)(ref val3)).AppendFormatted<string>(camera.Target[0].Root.Name);
			ImGui.Text(val3);
			return;
		}
		ImGui.SameLine();
		ImGui.Text(ImU8String.op_Implicit(_ctx.Locale.Translate("camera_edit.tracking.multi")));
		if (!ImGui.IsItemHovered())
		{
			return;
		}
		TooltipDisposable val4 = ImRaii.Tooltip();
		try
		{
			foreach (IGrouping<string, BoneNode> item2 in from t in camera.Target
				group t by t.Root.Name)
			{
				Separators.SeparatorText(ImU8String.op_Implicit(item2.Key), 0u, 0.5f);
				foreach (BoneNode item3 in item2)
				{
					ImGui.Text(ImU8String.op_Implicit(item3.Name));
				}
			}
		}
		finally
		{
			((TooltipDisposable)(ref val4)).Dispose();
		}
	}

	private static bool BoneSearchPredicate(BoneNode bone, string query)
	{
		return bone.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase);
	}

	private bool DrawBoneSelect(BoneNode bone, bool isFocus)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		if (_previouslyDrawn?.Root.Name != bone.Root.Name)
		{
			Separators.SeparatorText(ImU8String.op_Implicit(bone.Root?.Name));
			Separators.SeparatorText(ImU8String.op_Implicit(bone.Parent?.Name), 0u, 0.5f, 5f, Separators.LineHeight.Middle);
		}
		else if (_previouslyDrawn?.Parent?.Name != bone.Parent?.Name)
		{
			Separators.SeparatorText(ImU8String.op_Implicit(bone.Parent?.Name), 0u, 0.5f, 5f, Separators.LineHeight.Middle);
		}
		_previouslyDrawn = bone;
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(0, 1);
		((ImU8String)(ref val)).AppendFormatted<string>(bone.Name);
		return ImGui.Selectable(val, tracked.Contains(bone), (ImGuiSelectableFlags)0, default(Vector2));
	}

	private unsafe void DrawSliders(EditorCamera camera)
	{
		GameCameraEx* camera2 = camera.Camera;
		if (camera2 != null)
		{
			string hint = _ctx.Locale.Translate("camera_edit.sliders.rotation");
			string hint2 = _ctx.Locale.Translate("camera_edit.sliders.zoom");
			string hint3 = _ctx.Locale.Translate("camera_edit.sliders.distance");
			DisabledDisposable val = ImRaii.Disabled(IsWork);
			try
			{
				DrawSliderAngle("##CameraRotate", (FontAwesomeIcon)57560, ref camera2->Rotation, -180f, 180f, 0.5f, hint);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			DrawSliderAngle("##CameraZoom", (FontAwesomeIcon)61925, ref camera2->Zoom, -40f, 100f, 0.5f, hint2);
			DrawSliderFloat("##CameraDistance", (FontAwesomeIcon)61830, ref camera2->Distance, camera2->DistanceMin, camera2->DistanceMax, 0.05f, hint3);
			if (camera.IsOrthographic)
			{
				string hint4 = _ctx.Locale.Translate("camera_edit.sliders.ortho_zoom");
				DrawSliderFloat("##OrthographicZoom", (FontAwesomeIcon)62977, ref camera.OrthographicZoom, 0.1f, 10f, 0.01f, hint4);
			}
		}
	}

	private void DrawSliderAngle(string label, FontAwesomeIcon icon, ref float value, float min, float max, float drag, string hint = "")
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		DrawSliderIcon(icon, hint);
		ImGui.SliderAngle(ImU8String.op_Implicit(label), ref value, min, max, ImU8String.op_Implicit(""), (ImGuiSliderFlags)16);
		float value2 = value * MathHelpers.Rad2Deg;
		if (DrawSliderDrag(label, ref value2, min, max, drag, angle: true))
		{
			value = value2 * MathHelpers.Deg2Rad;
		}
	}

	private void DrawSliderFloat(string label, FontAwesomeIcon icon, ref float value, float min, float max, float drag, string hint = "")
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		DrawSliderIcon(icon, hint);
		ImGui.SliderFloat(ImU8String.op_Implicit(label), ref value, min, max, ImU8String.op_Implicit(""), (ImGuiSliderFlags)0);
		DrawSliderDrag(label, ref value, min, max, drag, angle: false);
	}

	private void DrawSliderIcon(FontAwesomeIcon icon, string hint = "")
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		DrawIconAlign(icon, out var spacing, hint);
		ImGui.SameLine(0f, spacing);
		ImGui.SetNextItemWidth(ImGui.CalcItemWidth() - (ImGui.GetCursorPosX() - ImGui.GetCursorStartPos().X));
	}

	private bool DrawSliderDrag(string label, ref float value, float min, float max, float drag, bool angle)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - _toolbar);
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(6, 1);
		((ImU8String)(ref val)).AppendFormatted<string>(label);
		((ImU8String)(ref val)).AppendLiteral("##Drag");
		return ImGui.DragFloat(val, ref value, drag, min, max, ImU8String.op_Implicit(angle ? "%.0f°" : "%.3f"), (ImGuiSliderFlags)0);
	}

	private void DrawIconAlign(FontAwesomeIcon icon, out float spacing, string hint = "")
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).CellPadding.X;
		float num = (UiBuilder.DefaultFontSizePx * ImGuiHelpers.GlobalScale - Icons.CalcIconSize(icon).X) / 2f;
		ImGui.SetCursorPosX(ImGui.GetCursorPosX() + x + num);
		Icons.DrawIcon(icon);
		if (!string.IsNullOrEmpty(hint) && ImGui.IsItemHovered())
		{
			TooltipDisposable val = ImRaii.Tooltip();
			try
			{
				ImGui.Text(ImU8String.op_Implicit(hint));
			}
			finally
			{
				((TooltipDisposable)(ref val)).Dispose();
			}
		}
		float num2 = x + num;
		style = ImGui.GetStyle();
		spacing = num2 + ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
	}
}
