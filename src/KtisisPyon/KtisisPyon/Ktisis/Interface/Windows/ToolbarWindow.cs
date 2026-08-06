using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Style;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility.Numerics;
using GLib.Widgets;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Components.Workspace;
using Ktisis.Interface.Editor.Types;
using Ktisis.Interface.Types;
using Ktisis.Interface.Windows.Editors;
using Ktisis.Interface.Windows.ToolbarModules;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Scene.Modules;

namespace Ktisis.Interface.Windows;

public class ToolbarWindow : KtisisWindow
{
	private readonly IEditorContext _ctx;

	private readonly GuiManager _gui;

	private KtisisWindow? _subWindow;

	private readonly WorkspaceState _workspace;

	private readonly StyleDisposable WindowStyle = new StyleDisposable();

	private List<WindowButtons> _buttons;

	private IEditorInterface Interface => _ctx.Interface;

	public ToolbarWindow(IEditorContext ctx, GuiManager gui)
		: base("toolbar.title", (ImGuiWindowFlags)0, "###KtisisToolbar")
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		_ctx = ctx;
		_gui = gui;
		_workspace = new WorkspaceState(ctx);
		((Window)this).Flags = (ImGuiWindowFlags)(((Window)this).Flags | 0x20 | 0x200000 | 0x40 | 8 | 0x10);
		_buttons = new List<WindowButtons>
		{
			new WindowButtons(DrawWorkspaceWindow, (FontAwesomeIcon)58793, Ktisis.Locale.Translate("toolbar.buttons.workspace"), typeof(Workspace)),
			new WindowButtons(DrawObjectWindow, (FontAwesomeIcon)61618, Ktisis.Locale.Translate("toolbar.buttons.object"), typeof(ObjectWindow)),
			new WindowButtons(DrawActorWindow, (FontAwesomeIcon)62804, Ktisis.Locale.Translate("toolbar.buttons.actor"), typeof(ActorWindow)),
			new WindowButtons(DrawPosingWindow, (FontAwesomeIcon)62432, Ktisis.Locale.Translate("toolbar.buttons.posing"), typeof(PosingWindow)),
			new WindowButtons(DrawEnvWindow, (FontAwesomeIcon)63172, Ktisis.Locale.Translate("toolbar.buttons.env"), typeof(Env)),
			new WindowButtons(DrawCameraWindow, (FontAwesomeIcon)61571, Ktisis.Locale.Translate("toolbar.buttons.camera"), typeof(CameraWindow)),
			new WindowButtons(DrawSceneWindow, (FontAwesomeIcon)58770, "Scene Editor", typeof(SceneWindow)),
			new WindowButtons(DrawConfigWindow, (FontAwesomeIcon)61573, Ktisis.Locale.Translate("toolbar.buttons.config"), typeof(ConfigWindow))
		};
	}

	public override void PreOpenCheck()
	{
		if (!_ctx.IsValid)
		{
			Ktisis.Log.Verbose("Context for toolbar window is stale, closing...");
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
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		if (((ImGuiStylePtr)(ref style)).ItemSpacing.X < 8f)
		{
			WindowStyle.Push((ImGuiStyleVar)13, VectorExtensions.WithY(StyleModelV1.DalamudClassic.ItemSpacing, ((ImGuiStylePtr)(ref style)).ItemSpacing.Y));
		}
		if (((ImGuiStylePtr)(ref style)).FramePadding.X > 4f)
		{
			WindowStyle.Push((ImGuiStyleVar)10, VectorExtensions.WithY(StyleModelV1.DalamudClassic.FramePadding, ((ImGuiStylePtr)(ref style)).FramePadding.Y));
		}
		if (((ImGuiStylePtr)(ref style)).CellPadding.X > 4f)
		{
			WindowStyle.Push((ImGuiStyleVar)16, VectorExtensions.WithY(StyleModelV1.DalamudClassic.CellPadding, ((ImGuiStylePtr)(ref style)).CellPadding.Y));
		}
		WindowStyle.Push((ImGuiStyleVar)22, StyleModelV1.DalamudClassic.ButtonTextAlign);
		((Window)this).PreDraw();
	}

	public unsafe override void Draw()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		_workspace.Draw();
		ImGui.Spacing();
		ImGuiWindowPtr currentWindow = ImGuiP.GetCurrentWindow();
		ImGui.SetCursorPosX((((ImGuiWindowPtr)(ref currentWindow)).ContentSize.X - (float)_buttons.Count * (48f * ImGuiHelpers.GlobalScale + x) - 2f * x - Buttons.CalcSize()) / 2f);
		foreach (WindowButtons button in _buttons)
		{
			ImGuiCol val;
			if (button.WindowType != typeof(PosingWindow))
			{
				val = (ImGuiCol)((((object)_subWindow)?.GetType() == button.WindowType) ? 23 : 21);
			}
			else
			{
				PosingWindow posingWindow = _ctx.Plugin.Gui.Get<PosingWindow>();
				val = (ImGuiCol)((posingWindow != null && ((Window)posingWindow).IsOpen) ? 23 : 21);
			}
			Vector4 styleColorVec = *ImGui.GetStyleColorVec4(val);
			ColorDisposable val2 = ImRaii.PushColor((ImGuiCol)21, styleColorVec, true);
			try
			{
				if (Buttons.IconButtonTooltip(button.Icon, button.TooltipText, new Vector2(48f, 48f) * ImGuiHelpers.GlobalScale))
				{
					button.Window();
				}
				ImGui.SameLine(0f, x * 2f);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		ImGui.SameLine();
		GroupDisposable val3 = ImRaii.Group();
		try
		{
			float num = (48f * ImGuiHelpers.GlobalScale - x) / 2f;
			DisabledDisposable val4 = ImRaii.Disabled(!_ctx.Actions.History.CanUndo);
			try
			{
				if (Buttons.IconButtonTooltip((FontAwesomeIcon)61512, _ctx.Locale.Translate("actions.History_Undo"), new Vector2(num, num)))
				{
					_ctx.Actions.History.Undo();
				}
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
			DisabledDisposable val5 = ImRaii.Disabled(!_ctx.Actions.History.CanRedo);
			try
			{
				if (Buttons.IconButtonTooltip((FontAwesomeIcon)61521, _ctx.Locale.Translate("actions.History_Redo"), new Vector2(num, num)))
				{
					_ctx.Actions.History.Redo();
				}
			}
			finally
			{
				((IDisposable)val5)?.Dispose();
			}
		}
		finally
		{
			((GroupDisposable)(ref val3)).Dispose();
		}
		if (_subWindow != null)
		{
			ImGui.Spacing();
			ImGui.Spacing();
			((Window)_subWindow).Draw();
		}
	}

	public override void PostDraw()
	{
		((Window)this).PostDraw();
		WindowStyle.Dispose();
	}

	internal void DrawWorkspaceWindow()
	{
		SetSubWindow<Workspace>();
	}

	internal void DrawObjectWindow()
	{
		SetSubWindow<ObjectWindow>();
	}

	internal void DrawActorWindow()
	{
		SetSubWindow<ActorWindow>();
	}

	internal void DrawPosingWindow()
	{
		Interface.OpenPosingWindow();
	}

	internal void DrawEnvWindow()
	{
		SetSubWindow<Env>();
	}

	internal void DrawCameraWindow()
	{
		SetSubWindow<CameraWindow>();
	}

	internal void DrawSceneWindow()
	{
		SetSubWindow<SceneWindow>();
	}

	internal void DrawConfigWindow()
	{
		SetSubWindow<ConfigWindow>();
	}

	private void SetSubWindow<T>() where T : KtisisWindow
	{
		if (((object)_subWindow)?.GetType() == typeof(ObjectWindow) && typeof(T) != typeof(ObjectWindow))
		{
			_subWindow?.Close();
		}
		if (((object)_subWindow)?.GetType() == typeof(T))
		{
			((Window)_subWindow).OnClose();
			_subWindow = null;
			return;
		}
		if (typeof(T) == typeof(Env))
		{
			EnvModule module = _ctx.Scene.GetModule<EnvModule>();
			_subWindow = _gui.GetOrCreate<Env>(new object[2] { _ctx.Scene, module });
		}
		else if (typeof(T) == typeof(ObjectWindow))
		{
			_subWindow = Interface.GetObjectWindow();
		}
		else if (typeof(T) == typeof(ConfigWindow))
		{
			_subWindow = _gui.GetOrCreate<ConfigWindow>(Array.Empty<object>());
		}
		else if (typeof(T) == typeof(ActorWindow))
		{
			_subWindow = _gui.GetOrCreate<T>(new object[1] { _ctx });
			((Window)_subWindow).Size = new Vector2(0f, 400f);
		}
		else if (typeof(T) == typeof(SceneWindow))
		{
			_subWindow = _gui.GetOrCreate<SceneWindow>(new object[1] { _ctx });
		}
		else
		{
			_subWindow = _gui.GetOrCreate<T>(new object[1] { _ctx });
		}
		if (_subWindow is ActorWindow actorWindow)
		{
			SceneEntity firstSelected = _ctx.Selection.GetFirstSelected();
			SceneEntity sceneEntity = ((firstSelected is BoneNode boneNode) ? boneNode.Pose.Parent : ((firstSelected is BoneNodeGroup boneNodeGroup) ? boneNodeGroup.Pose.Parent : ((!(firstSelected is EntityPose entityPose)) ? firstSelected : entityPose.Parent)));
			if (sceneEntity is ActorEntity target)
			{
				actorWindow.SetTarget(target);
			}
			else
			{
				actorWindow.SetTarget(_ctx.Scene.GetFirstActor());
			}
		}
		((Window)_subWindow).OnOpen();
	}

	public override void OnClose()
	{
		base.OnClose();
		if (_ctx.Config.Editor.OpenTrayOnWorkspaceClose)
		{
			_ctx.Plugin.Gui.GetOrCreate<TrayIcon>(new object[1] { _ctx }).Open();
		}
		_gui.Remove(this);
	}
}
