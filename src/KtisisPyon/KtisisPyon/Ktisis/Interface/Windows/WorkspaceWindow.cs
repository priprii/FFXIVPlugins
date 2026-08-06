using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using GLib.Widgets;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Components.Workspace;
using Ktisis.Interface.Editor.Types;
using Ktisis.Interface.Types;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;

namespace Ktisis.Interface.Windows;

public class WorkspaceWindow : KtisisWindow
{
	private readonly IEditorContext _ctx;

	private protected readonly CameraSelector _cameras;

	protected readonly WorkspaceState _workspace;

	private protected readonly SceneTree _sceneTree;

	private static readonly Vector2 MinimumSize = new Vector2(280f, 300f);

	private IEditorInterface Interface => _ctx.Interface;

	public WorkspaceWindow(IEditorContext ctx)
		: base("workspace.title", (ImGuiWindowFlags)0, "###KtisisWorkspace")
	{
		_ctx = ctx;
		_cameras = new CameraSelector(ctx);
		_workspace = new WorkspaceState(ctx);
		_sceneTree = new SceneTree(ctx);
	}

	public override void PreOpenCheck()
	{
		if (!_ctx.IsValid)
		{
			Ktisis.Log.Verbose("Context for workspace window is stale, closing...");
			Close();
		}
	}

	public override void OnOpen()
	{
		_ctx.Plugin.Gui.Get<TrayIcon>()?.Close();
		((Window)this).OnOpen();
	}

	public override void PreDraw()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		WindowSizeConstraints value = default(WindowSizeConstraints);
		((WindowSizeConstraints)(ref value))._002Ector();
		((WindowSizeConstraints)(ref value)).MinimumSize = MinimumSize;
		ImGuiIOPtr iO = ImGui.GetIO();
		((WindowSizeConstraints)(ref value)).MaximumSize = ((ImGuiIOPtr)(ref iO)).DisplaySize * 0.9f;
		((Window)this).SizeConstraints = value;
	}

	public override void Draw()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		DrawContextButtons();
		ImGui.Spacing();
		_cameras.Draw();
		_workspace.Draw();
		float num = (UiBuilder.DefaultFontSizePx + (((ImGuiStylePtr)(ref style)).ItemSpacing.Y + ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.Y) * 2f) * ImGuiHelpers.GlobalScale;
		float height = Math.Max(ImGui.GetContentRegionAvail().Y, ImGui.GetTextLineHeightWithSpacing() * 10f) - num;
		_sceneTree.Draw(height);
		ImGui.Spacing();
		DrawSceneTreeButtons();
	}

	public override void OnClose()
	{
		if (_ctx.Config.Editor.OpenTrayOnWorkspaceClose && !_ctx.Config.Editor.UseToolbar)
		{
			_ctx.Plugin.Gui.GetOrCreate<TrayIcon>(new object[1] { _ctx }).Open();
		}
		base.OnClose();
	}

	private protected void DrawContextButtons()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)61618, _ctx.Locale.Translate("transform_edit.title")))
		{
			Interface.OpenObjectEditor();
		}
		ImGui.SameLine(0f, x);
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)61829, _ctx.Locale.Translate("env_edit.title")))
		{
			Interface.OpenEnvironmentWindow();
		}
		ImGui.SameLine(0f, x);
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)58770, _ctx.Locale.Translate("scene_edit.title")))
		{
			Interface.OpenSceneWindow();
		}
		ImGui.SameLine(0f, x);
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)62804, _ctx.Locale.Translate("chara_edit.title")))
		{
			SceneEntity firstSelected = _ctx.Selection.GetFirstSelected();
			SceneEntity sceneEntity = ((firstSelected is BoneNode boneNode) ? boneNode.Pose.Parent : ((firstSelected is BoneNodeGroup boneNodeGroup) ? boneNodeGroup.Pose.Parent : ((!(firstSelected is EntityPose entityPose)) ? firstSelected : entityPose.Parent)));
			if (sceneEntity is ActorEntity actor)
			{
				Interface.OpenActorEditor(actor);
			}
			else
			{
				Interface.OpenActorEditor(_ctx.Scene.GetFirstActor());
			}
		}
		ImGui.SameLine(0f, x);
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)62432, _ctx.Locale.Translate("pose_view.title")))
		{
			Interface.OpenPosingWindow();
		}
		ImGui.SameLine(0f, x);
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)61459, _ctx.Locale.Translate("config.title")))
		{
			Interface.OpenConfigWindow();
		}
		ImGui.SameLine(0f, x);
		ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - Buttons.CalcSize() * 2f - x);
		DisabledDisposable val = ImRaii.Disabled(!_ctx.Actions.History.CanUndo);
		try
		{
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)61512, _ctx.Locale.Translate("actions.History_Undo")))
			{
				_ctx.Actions.History.Undo();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		ImGui.SameLine(0f, x);
		DisabledDisposable val2 = ImRaii.Disabled(!_ctx.Actions.History.CanRedo);
		try
		{
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)61521, _ctx.Locale.Translate("actions.History_Redo")))
			{
				_ctx.Actions.History.Redo();
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private protected void DrawSceneTreeButtons()
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		if (Buttons.IconButtonDropdown((FontAwesomeIcon)58675, Interface.OpenActorCreateMenu))
		{
			_ctx.Scene.Factory.CreateActor().Spawn();
		}
		if (ImGui.IsItemHovered((ImGuiHoveredFlags)128))
		{
			TooltipDisposable val = ImRaii.Tooltip();
			try
			{
				ImGui.Text(ImU8String.op_Implicit(_ctx.Locale.Translate("workspace.create_actor")));
			}
			finally
			{
				((TooltipDisposable)(ref val)).Dispose();
			}
		}
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		if (Buttons.IconButtonDropdown((FontAwesomeIcon)61675, Interface.OpenLightCreateMenu))
		{
			_ctx.Scene.Factory.CreateLight().Spawn();
		}
		if (ImGui.IsItemHovered((ImGuiHoveredFlags)128))
		{
			TooltipDisposable val2 = ImRaii.Tooltip();
			try
			{
				ImGui.Text(ImU8String.op_Implicit(_ctx.Locale.Translate("workspace.create_light")));
			}
			finally
			{
				((TooltipDisposable)(ref val2)).Dispose();
			}
		}
		style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)62637, _ctx.Locale.Translate("workspace.create_overlay")))
		{
			Interface.OpenOverlayCreateMenu();
		}
		style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		ColorDisposable val3 = ImRaii.PushColor((ImGuiCol)21, ImGui.GetColorU32((ImGuiCol)23), _ctx.ShowWorldObjects);
		try
		{
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)63228, _ctx.Locale.Translate("workspace.overlay.world_toggle")))
			{
				_ctx.ShowWorldObjects = !_ctx.ShowWorldObjects;
			}
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
		if (_ctx.ShowWorldObjects)
		{
			style = ImGui.GetStyle();
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			ImGui.Text(ImU8String.op_Implicit(_ctx.Locale.Translate("workspace.overlay.range")));
			style = ImGui.GetStyle();
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
			ImGui.SliderFloat(ImU8String.op_Implicit("##RangeSlider"), ref _ctx.Config.Overlay.WorldCameraRange, 5f, 100f, ImU8String.op_Implicit("%.2fy"), (ImGuiSliderFlags)0);
		}
	}
}
