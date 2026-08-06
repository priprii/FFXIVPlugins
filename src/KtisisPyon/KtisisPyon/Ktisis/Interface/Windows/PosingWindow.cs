using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImGuizmo;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;
using GLib.Widgets;
using Ktisis.Common.Extensions;
using Ktisis.Common.Utility;
using Ktisis.Data.Config.Pose2D;
using Ktisis.Data.Serialization;
using Ktisis.Editor.Camera.Types;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Selection;
using Ktisis.Editor.Transforms;
using Ktisis.Editor.Transforms.Types;
using Ktisis.Interface.Components.Posing;
using Ktisis.Interface.Components.Posing.Types;
using Ktisis.Interface.Components.Transforms;
using Ktisis.Interface.Types;
using Ktisis.Localization;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Services.Game;

namespace Ktisis.Interface.Windows;

public class PosingWindow : KtisisWindow
{
	private enum ViewEnum
	{
		Body,
		Face
	}

	private readonly IEditorContext _ctx;

	private readonly LocaleManager _locale;

	private readonly GPoseService _gpose;

	private readonly PoseViewRenderer _render;

	private readonly Gizmo2D _gizmo;

	private readonly TransformTable _table;

	private PoseViewSchema? _schema;

	private ViewEnum _view;

	internal ActorEntity? _target;

	private ITransformMemento? Transform;

	public PosingWindow(IEditorContext ctx, ITextureProvider tex, LocaleManager locale, GPoseService gpose, TransformTable table, Gizmo2D gizmo)
		: base("pose_view.title", (ImGuiWindowFlags)0, "###KtisisPoseView")
	{
		_ctx = ctx;
		_locale = locale;
		_gpose = gpose;
		_render = new PoseViewRenderer(ctx.Config, tex);
		_table = table;
		_gizmo = gizmo;
	}

	public override void OnOpen()
	{
		_schema = SchemaReader.ReadPoseView();
	}

	public override void PreOpenCheck()
	{
		if (!_ctx.IsValid)
		{
			Ktisis.Log.Verbose("Context for posing window is stale, closing...");
			Close();
		}
	}

	public override void PreDraw()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		WindowSizeConstraints value = default(WindowSizeConstraints);
		((WindowSizeConstraints)(ref value))._002Ector();
		((WindowSizeConstraints)(ref value)).MinimumSize = new Vector2(500f, 350f);
		((Window)this).SizeConstraints = value;
	}

	public override void Draw()
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		ITransformTarget target = _ctx.Transform.Target;
		if (_ctx.Config.Editor.UseLegacyPoseViewTabs && !_ctx.Config.Editor.UseToolbar)
		{
			DrawLegacyTabs();
			return;
		}
		if (UpdateTarget())
		{
			((Window)this).WindowName = Ktisis.Locale.Translate(_localeWindowName) + " - " + _target.Name + _windowId;
		}
		ActorEntity target2 = _target;
		if (target2 == null || !target2.IsValid)
		{
			ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("pose_view.no_target")));
			return;
		}
		DrawWindow(_target);
		if (!_ctx.Config.Editor.UseToolbar)
		{
			return;
		}
		ImGuiStylePtr style;
		if (_ctx.Config.Editor.FlyoutOpen)
		{
			ImGui.SameLine();
			GroupDisposable val = ImRaii.Group();
			try
			{
				DrawToggles(target);
				DrawTransform(target);
				Vector2 vector = ImGui.GetContentRegionMax().Sub(Buttons.CalcSize());
				style = ImGui.GetStyle();
				Vector2 vec = vector - ((ImGuiStylePtr)(ref style)).WindowPadding;
				float num = TransformTable.CalcWidth();
				style = ImGui.GetStyle();
				ImGui.SetCursorPos(vec.SubX(num + ((ImGuiStylePtr)(ref style)).WindowPadding.X * 2f));
				if (ImGui.Button(ImU8String.op_Implicit("<"), default(Vector2)))
				{
					Vector2 windowSize = ImGui.GetWindowSize();
					float num2 = TransformTable.CalcWidth();
					style = ImGui.GetStyle();
					ImGui.SetWindowSize(windowSize.SubX(num2 + ((ImGuiStylePtr)(ref style)).WindowPadding.X * 2f), (ImGuiCond)0);
					_ctx.Config.Editor.FlyoutOpen = false;
				}
				return;
			}
			finally
			{
				((GroupDisposable)(ref val)).Dispose();
			}
		}
		Vector2 vector2 = ImGui.GetContentRegionMax().Sub(Buttons.CalcSize());
		style = ImGui.GetStyle();
		ImGui.SetCursorPos(vector2 - ((ImGuiStylePtr)(ref style)).WindowPadding);
		if (ImGui.Button(ImU8String.op_Implicit(">"), default(Vector2)))
		{
			Vector2 windowSize2 = ImGui.GetWindowSize();
			float num3 = TransformTable.CalcWidth();
			style = ImGui.GetStyle();
			ImGui.SetWindowSize(windowSize2.AddX(num3 + ((ImGuiStylePtr)(ref style)).WindowPadding.X * 2f), (ImGuiCond)0);
			_ctx.Config.Editor.FlyoutOpen = true;
		}
	}

	private bool UpdateTarget()
	{
		ActorEntity actorEntity = (ActorEntity)_ctx.Selection.GetSelected().FirstOrDefault((SceneEntity entity) => entity is ActorEntity);
		if (actorEntity == null || _target == actorEntity)
		{
			return false;
		}
		_target = actorEntity;
		return true;
	}

	private IEnumerable<ActorEntity> GetValidTargets()
	{
		return _ctx.Scene.Children.Where((SceneEntity entity) => entity is ActorEntity).Cast<ActorEntity>();
	}

	private void DrawLegacyTabs()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		TabBarDisposable val = ImRaii.TabBar(ImU8String.op_Implicit("##pose_tabs"));
		try
		{
			foreach (ActorEntity validTarget in GetValidTargets())
			{
				TabItemDisposable val2 = ImRaii.TabItem(ImU8String.op_Implicit(validTarget.Name));
				try
				{
					if (val2.Success)
					{
						ImGui.Spacing();
						DrawWindow(validTarget);
					}
				}
				finally
				{
					((TabItemDisposable)(ref val2)).Dispose();
				}
			}
		}
		finally
		{
			((TabBarDisposable)(ref val)).Dispose();
		}
	}

	private void DrawLegacyTarget()
	{
		IGameObject? gPoseTarget = _gpose.GPoseTarget;
		ushort? tarIndex = ((gPoseTarget != null) ? new ushort?(gPoseTarget.ObjectIndex) : ((ushort?)null));
		if ((_target == null || _target.Actor.ObjectIndex != tarIndex) && tarIndex.HasValue)
		{
			ActorEntity actorEntity = GetValidTargets().FirstOrDefault((ActorEntity actor) => actor.Actor.ObjectIndex == tarIndex);
			if (actorEntity != null)
			{
				_target = actorEntity;
			}
		}
		ActorEntity target = _target;
		if (target == null || !target.IsValid)
		{
			Ktisis.Log.Info("Targeted actor has no skeleton or is invalid.");
		}
		else
		{
			DrawWindow(_target);
		}
	}

	private void DrawWindow(ActorEntity target)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Vector2 vector = ImGui.GetContentRegionAvail();
		ImGuiStylePtr style;
		if (_ctx.Config.Editor.UseToolbar && _ctx.Config.Editor.FlyoutOpen)
		{
			Vector2 vec = vector;
			float num = TransformTable.CalcWidth();
			style = ImGui.GetStyle();
			vector = vec.SubX(num + ((ImGuiStylePtr)(ref style)).WindowPadding.X * 2f);
		}
		float num2 = vector.X * 0.9f;
		style = ImGui.GetStyle();
		float num3 = ((ImGuiStylePtr)(ref style)).ItemSpacing.X * 2f;
		Vector2 vector2 = vector;
		vector2.X = num2 - num3;
		Vector2 region = vector2;
		DrawView(target, region);
		ImGui.SameLine();
		if (!_ctx.Config.Editor.UseToolbar || !_ctx.Config.Editor.FlyoutOpen)
		{
			ImGui.SetCursorPosX(num2);
		}
		DrawSideMenu(target);
	}

	private void DrawSideMenu(ActorEntity target)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		GroupDisposable val = ImRaii.Group();
		try
		{
			DrawViewSelect();
			for (int i = 0; i < 3; i++)
			{
				ImGui.Spacing();
			}
			DrawImportExport(target);
		}
		finally
		{
			((GroupDisposable)(ref val)).Dispose();
		}
	}

	private void DrawViewSelect()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		GroupDisposable val = ImRaii.Group();
		try
		{
			ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("pose_view.view_chooser")));
			ViewEnum[] values = Enum.GetValues<ViewEnum>();
			for (int i = 0; i < values.Length; i++)
			{
				ViewEnum viewEnum = values[i];
				if (ImGui.RadioButton(ImU8String.op_Implicit(viewEnum.ToString()), _view == viewEnum))
				{
					_view = viewEnum;
				}
			}
		}
		finally
		{
			((GroupDisposable)(ref val)).Dispose();
		}
	}

	private void DrawImportExport(ActorEntity target)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (target.Pose != null)
		{
			if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("pose_view.import")), default(Vector2)))
			{
				_ctx.Interface.OpenPoseImport(target);
			}
			if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("pose_view.export")), default(Vector2)))
			{
				_ctx.Interface.OpenPoseExport(target.Pose);
			}
		}
	}

	private void DrawView(ActorEntity target, Vector2 region)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		ChildDisposable val = ImRaii.Child(ImU8String.op_Implicit("##viewFrame"), region, false, (ImGuiWindowFlags)8);
		try
		{
			IViewFrame viewFrame = _render.StartFrame();
			if (_view == ViewEnum.Body)
			{
				DrawView(viewFrame, "Body", 0.35f);
				ImGui.SameLine();
				DrawView(viewFrame, "Armor", 0.35f);
				ImGui.SameLine();
				GroupDisposable val2 = ImRaii.Group();
				try
				{
					DrawView(viewFrame, "Hands", 0.3f, 0.6f);
					ImGui.Spacing();
					bool num = target.Pose?.HasTail() ?? false;
					bool flag = target.Pose?.HasBunnyEars() ?? false;
					IDictionary<string, string> template = _render.BuildTemplate(target);
					float num2;
					if (num)
					{
						if (!flag)
						{
							goto IL_00cd;
						}
						num2 = 0.15f;
					}
					else
					{
						if (flag)
						{
							goto IL_00cd;
						}
						num2 = 0f;
					}
					goto IL_00dd;
					IL_00cd:
					num2 = 0.3f;
					goto IL_00dd;
					IL_00dd:
					float width = num2;
					if (num)
					{
						DrawView(viewFrame, "Tail", width, 0.4f);
						if (flag)
						{
							ImGui.SameLine();
						}
					}
					if (flag)
					{
						DrawView(viewFrame, "Ears", width, 0.4f, template);
					}
				}
				finally
				{
					((GroupDisposable)(ref val2)).Dispose();
				}
			}
			else
			{
				DrawView(viewFrame, "Face", 0.65f);
				ImGui.SameLine();
				GroupDisposable val3 = ImRaii.Group();
				try
				{
					DrawView(viewFrame, "Lips", 0.35f, 0.5f);
					DrawView(viewFrame, "Mouth", 0.35f, 0.5f);
				}
				finally
				{
					((GroupDisposable)(ref val3)).Dispose();
				}
			}
			if (target.Pose != null)
			{
				viewFrame.DrawBones(target.Pose);
			}
		}
		finally
		{
			((ChildDisposable)(ref val)).Dispose();
		}
	}

	private void DrawView(IViewFrame frame, string name, float width = 1f, float height = 1f, IDictionary<string, string>? template = null)
	{
		if (!(_schema == null) && _schema.Views.TryGetValue(name, out PoseViewEntry value))
		{
			frame.DrawView(value, width, height, template);
		}
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

	private bool DrawTransform(ref Transform transform, out bool isEnded, bool disabled)
	{
		isEnded = false;
		bool flag = false;
		if (!_ctx.Config.Editor.TransformHide)
		{
			flag = DrawGizmo(ref transform, ImGui.GetContentRegionAvail().X - (_ctx.Config.Editor.UseToolbar ? 0.1f : 0f), disabled);
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

	private unsafe bool DrawGizmo(ref Transform transform, float width, bool disabled)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		Vector2 rectSize = new Vector2(width, 300f);
		_gizmo.Begin(rectSize, "pose");
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
		float x2 = ImGui.GetContentRegionAvail().X;
		if (x2 > num)
		{
			ImGui.SetCursorPosX(ImGui.GetCursorPosX() + x2 - num);
		}
		bool transformHide = _ctx.Config.Editor.TransformHide;
		int num5 = (transformHide ? 61656 : 61655);
		string text4 = (transformHide ? "show" : "hide");
		string tooltip5 = _ctx.Locale.Translate("transform_edit.gizmo." + text4);
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)num5, tooltip5, vector))
		{
			_ctx.Config.Editor.TransformHide = !transformHide;
		}
	}
}
