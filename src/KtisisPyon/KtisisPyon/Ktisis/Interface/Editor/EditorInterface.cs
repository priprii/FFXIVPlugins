using System;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using GLib.Popups.ImFileDialog;
using Ktisis.Data.Files;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Selection;
using Ktisis.Interface.Components.Chara;
using Ktisis.Interface.Components.Transforms;
using Ktisis.Interface.Editor.Context;
using Ktisis.Interface.Editor.Popup;
using Ktisis.Interface.Editor.Types;
using Ktisis.Interface.Overlay;
using Ktisis.Interface.Types;
using Ktisis.Interface.Windows;
using Ktisis.Interface.Windows.Editors;
using Ktisis.Interface.Windows.Import;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Scene.Entities.World;
using Ktisis.Scene.Modules;
using Ktisis.Scene.Modules.Actors;
using Ktisis.Scene.Modules.Lights;
using Ktisis.Scene.Types;

namespace Ktisis.Interface.Editor;

public class EditorInterface : IEditorInterface
{
	private readonly IEditorContext _ctx;

	private readonly GuiManager _gui;

	private readonly GizmoManager _gizmo;

	private static readonly FileDialogOptions CharaFileOptions = new FileDialogOptions
	{
		Filters = Ktisis.Locale.Translate("file.dialog.chara.filter") + "{.chara}",
		Extension = ".chara"
	};

	private static readonly FileDialogOptions ExportPoseFileOptions = new FileDialogOptions
	{
		Filters = Ktisis.Locale.Translate("file.dialog.pose.filter") + "{.pose,.cmp}",
		Extension = ".pose"
	};

	private static readonly FileDialogOptions LightFileOptions = new FileDialogOptions
	{
		Filters = Ktisis.Locale.Translate("file.dialog.light.filter") + "{.ktlight}",
		Extension = ".ktlight"
	};

	private static readonly FileDialogOptions ImportPoseFileOptions = new FileDialogOptions
	{
		Filters = Ktisis.Locale.Translate("file.dialog.pose.filter") + "{.pose,.cmp}"
	};

	private static readonly FileDialogOptions McdfFileOptions = new FileDialogOptions
	{
		Filters = Ktisis.Locale.Translate("file.dialog.mcdf.filter") + "{.mcdf}",
		Extension = ".mcdf"
	};

	private static readonly FileDialogOptions SceneFileOptions = new FileDialogOptions
	{
		Filters = "Ktisis Scene Files{.ktscene}",
		Extension = ".ktscene"
	};

	public EditorInterface(IEditorContext ctx, GuiManager gui)
	{
		_ctx = ctx;
		_gui = gui;
		_gizmo = new GizmoManager(ctx.Config);
	}

	public void Prepare()
	{
		if (_ctx.Config.Editor.OpenOnEnterGPose)
		{
			if (_ctx.Config.Editor.UseToolbar)
			{
				_gui.GetOrCreate<ToolbarWindow>(new object[2] { _ctx, _gui }).Open();
			}
			else
			{
				_gui.GetOrCreate<WorkspaceWindow>(new object[1] { _ctx }).Open();
			}
		}
		_ctx.Selection.Changed += OnSelectChanged;
		_gizmo.Initialize();
		_gui.GetOrCreate<OverlayWindow>(new object[3]
		{
			_ctx,
			_gizmo.Create(GizmoId.OverlayMain),
			_gizmo.Create(GizmoId.GazeTarget)
		}).Open();
	}

	private void OnSelectChanged(ISelectManager sender)
	{
		if (!_ctx.Config.Editor.ToggleEditorOnSelect)
		{
			return;
		}
		bool flag = sender.Count > 0;
		ObjectWindow objectWindow = _gui.Get<ObjectWindow>();
		if (objectWindow == null)
		{
			if (flag)
			{
				OpenObjectEditor();
			}
		}
		else if (_ctx.Config.Editor.CloseEditorOnDeselect)
		{
			((Window)objectWindow).IsOpen = flag;
		}
	}

	public void OpenConfigWindow()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (_ctx.Config.Editor.ToggleOpenWindows)
		{
			((Window)_gui.GetOrCreate<ConfigWindow>(Array.Empty<object>())).Toggle();
			return;
		}
		ConfigWindow orCreate = _gui.GetOrCreate<ConfigWindow>(Array.Empty<object>());
		orCreate.Open();
		ImGui.SetWindowFocus(ImU8String.op_Implicit(((Window)orCreate).WindowName));
	}

	public void ToggleWorkspaceWindow()
	{
		if (_ctx.Config.Editor.UseToolbar)
		{
			((Window)_gui.GetOrCreate<ToolbarWindow>(new object[1] { _ctx })).Toggle();
		}
		else
		{
			((Window)_gui.GetOrCreate<WorkspaceWindow>(new object[1] { _ctx })).Toggle();
		}
	}

	public void ToggleDebugWindow()
	{
		((Window)_gui.GetOrCreate<DebugWindow>(new object[1] { _ctx })).Toggle();
	}

	public void OpenCameraWindow()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (_ctx.Config.Editor.UseToolbar)
		{
			_gui.Get<ToolbarWindow>().DrawCameraWindow();
			return;
		}
		if (_ctx.Config.Editor.ToggleOpenWindows)
		{
			((Window)_gui.GetOrCreate<CameraWindow>(new object[1] { _ctx })).Toggle();
			return;
		}
		CameraWindow orCreate = _gui.GetOrCreate<CameraWindow>(new object[1] { _ctx });
		orCreate.Open();
		ImGui.SetWindowFocus(ImU8String.op_Implicit(((Window)orCreate).WindowName));
	}

	public void OpenEnvironmentWindow()
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (_ctx.Config.Editor.UseToolbar)
		{
			_gui.Get<ToolbarWindow>().DrawEnvWindow();
			return;
		}
		ISceneManager scene = _ctx.Scene;
		EnvModule module = scene.GetModule<EnvModule>();
		if (_ctx.Config.Editor.ToggleOpenWindows)
		{
			((Window)_gui.GetOrCreate<EnvWindow>(new object[2] { scene, module })).Toggle();
		}
		else
		{
			EnvWindow orCreate = _gui.GetOrCreate<EnvWindow>(new object[2] { scene, module });
			orCreate.Open();
			ImGui.SetWindowFocus(ImU8String.op_Implicit(((Window)orCreate).WindowName));
		}
	}

	public void OpenSceneWindow()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (_ctx.Config.Editor.ToggleOpenWindows)
		{
			((Window)_gui.GetOrCreate<SceneWindow>(new object[1] { _ctx })).Toggle();
			return;
		}
		SceneWindow orCreate = _gui.GetOrCreate<SceneWindow>(new object[1] { _ctx });
		orCreate.Open();
		ImGui.SetWindowFocus(ImU8String.op_Implicit(((Window)orCreate).WindowName));
	}

	public void OpenObjectEditor(bool forceOpen = false)
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		if (_ctx.Config.Editor.UseToolbar)
		{
			_gui.Get<ToolbarWindow>().DrawObjectWindow();
			return;
		}
		Gizmo gizmo = _gizmo.Create(GizmoId.TransformEditor);
		if (_ctx.Config.Editor.ToggleOpenWindows && !forceOpen)
		{
			((Window)_gui.GetOrCreate<ObjectWindow>(new object[3]
			{
				_ctx,
				new Gizmo2D(_ctx.Config.Gizmo, gizmo),
				_gui
			})).Toggle();
		}
		else
		{
			ObjectWindow orCreate = _gui.GetOrCreate<ObjectWindow>(new object[3]
			{
				_ctx,
				new Gizmo2D(_ctx.Config.Gizmo, gizmo),
				_gui
			});
			orCreate.Open();
			ImGui.SetWindowFocus(ImU8String.op_Implicit(((Window)orCreate).WindowName));
		}
	}

	public ObjectWindow GetObjectWindow()
	{
		Gizmo gizmo = _gizmo.Create(GizmoId.TransformEditor);
		return _gui.GetOrCreate<ObjectWindow>(new object[3]
		{
			_ctx,
			new Gizmo2D(_ctx.Config.Gizmo, gizmo),
			_gui
		});
	}

	public void OpenObjectEditor(SceneEntity entity, bool forceOpen = false)
	{
		entity.Select(SelectMode.Force);
		OpenObjectEditor(forceOpen);
	}

	public void OpenPosingWindow()
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		Gizmo gizmo = _gizmo.Create(GizmoId.TransformEditor);
		if (_ctx.Config.Editor.ToggleOpenWindows)
		{
			((Window)_gui.GetOrCreate<PosingWindow>(new object[3]
			{
				_ctx,
				_ctx.Locale,
				new Gizmo2D(_ctx.Config.Gizmo, gizmo)
			})).Toggle();
		}
		else
		{
			PosingWindow orCreate = _gui.GetOrCreate<PosingWindow>(new object[3]
			{
				_ctx,
				_ctx.Locale,
				new Gizmo2D(_ctx.Config.Gizmo, gizmo)
			});
			orCreate.Open();
			ImGui.SetWindowFocus(ImU8String.op_Implicit(((Window)orCreate).WindowName));
		}
	}

	public void OpenSceneCreateMenu()
	{
		SceneCreateMenuBuilder sceneCreateMenuBuilder = new SceneCreateMenuBuilder(_ctx);
		_gui.AddPopup(sceneCreateMenuBuilder.Create()).Open();
	}

	public void OpenActorCreateMenu()
	{
		SceneCreateMenuBuilder sceneCreateMenuBuilder = new SceneCreateMenuBuilder(_ctx);
		_gui.AddPopup(sceneCreateMenuBuilder.CreateActor()).Open();
	}

	public void OpenLightCreateMenu()
	{
		SceneCreateMenuBuilder sceneCreateMenuBuilder = new SceneCreateMenuBuilder(_ctx);
		_gui.AddPopup(sceneCreateMenuBuilder.CreateLight()).Open();
	}

	public void OpenOverlayCreateMenu()
	{
		SceneCreateMenuBuilder sceneCreateMenuBuilder = new SceneCreateMenuBuilder(_ctx);
		_gui.AddPopup(sceneCreateMenuBuilder.CreateOverlay()).Open();
	}

	public void OpenSceneEntityMenu(SceneEntity entity)
	{
		SceneEntityMenuBuilder sceneEntityMenuBuilder = new SceneEntityMenuBuilder(_ctx, entity);
		_gui.AddPopup(sceneEntityMenuBuilder.Create()).Open();
	}

	public void OpenAssignCollection(ActorEntity entity)
	{
		_gui.CreatePopup<ActorCollectionPopup>(new object[2] { _ctx, entity }).Open();
	}

	public void OpenApplyDesign(ActorEntity entity)
	{
		_gui.CreatePopup<ActorDesignPopup>(new object[2] { _ctx, entity }).Open();
	}

	public void OpenAssignCProfile(ActorEntity entity)
	{
		_gui.CreatePopup<ActorCProfilePopup>(new object[2] { _ctx, entity }).Open();
	}

	public void OpenOverworldActorList()
	{
		_gui.CreatePopup<OverworldActorPopup>(new object[1] { _ctx }).Open();
	}

	public void RefreshSceneEntities()
	{
		_ctx.Scene.GetModule<ActorModule>().RefreshGPoseActors();
		_ctx.Scene.GetModule<LightModule>().RefreshLightEntities();
		_ctx.Scene.World.Refresh();
	}

	public void SelectAllEntities()
	{
		_ctx.Selection.Clear();
		foreach (SceneEntity child in _ctx.Scene.Children)
		{
			_ctx.Selection.Select(child, SelectMode.Multiple);
		}
	}

	public void OpenRenameEntity(SceneEntity entity)
	{
		_gui.CreatePopup<EntityRenameModal>(new object[1] { entity }).Open();
	}

	public void OpenSavePreset(ActorEntity entity)
	{
		_gui.CreatePopup<PresetSaveModal>(new object[1] { entity }).Open();
	}

	public void OpenActorEditor(ActorEntity actor)
	{
		if (_ctx.Config.Editor.UseToolbar)
		{
			_gui.Get<ToolbarWindow>().DrawActorWindow();
		}
		else if (OpenEditor<ActorWindow, ActorEntity>(actor) && _ctx.Selection.Count > 0 && !_ctx.Selection.IsActorSelected(actor))
		{
			actor.Select(SelectMode.Force);
		}
	}

	public void OpenLightEditor(LightEntity light)
	{
		OpenObjectEditor(light, forceOpen: true);
	}

	public bool OpenEditor<T, TA>(TA entity) where T : EntityEditWindow<TA> where TA : SceneEntity
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		T orCreate = _gui.GetOrCreate<T>(new object[1] { _ctx });
		orCreate.SetTarget(entity);
		if (_ctx.Config.Editor.ToggleOpenWindows)
		{
			((Window)orCreate).Toggle();
		}
		else
		{
			orCreate.Open();
			ImGui.SetWindowFocus(ImU8String.op_Implicit(((Window)orCreate).WindowName));
		}
		return ((Window)orCreate).IsOpen;
	}

	public void OpenEditorFor(SceneEntity entity)
	{
		if (!(entity is ActorEntity actor))
		{
			if (entity is LightEntity light)
			{
				OpenLightEditor(light);
			}
		}
		else
		{
			OpenActorEditor(actor);
		}
	}

	public void OpenCharaImport(ActorEntity actor, bool openNpc = false)
	{
		CharaImportDialog orCreate = _gui.GetOrCreate<CharaImportDialog>(new object[1] { _ctx });
		orCreate.SetTarget(actor);
		if (openNpc)
		{
			orCreate.SetMethod(LoadMethod.Npc);
		}
		orCreate.Open();
	}

	public async Task OpenCharaExport(ActorEntity actor)
	{
		ExportCharaFile(await _ctx.Characters.SaveCharaFile(actor));
	}

	public void OpenPoseImport(ActorEntity actor)
	{
		OpenEditor<PoseImportDialog, ActorEntity>(actor);
	}

	public async Task OpenPoseExport(EntityPose pose)
	{
		ExportPoseFile(await _ctx.Posing.SavePoseFile(pose));
	}

	public async Task OpenLightExport(LightEntity light)
	{
		ExportLightFile(await _ctx.Scene.SaveLightFile(light));
	}

	public void OpenCharaFile(Action<string, CharaFile> handler)
	{
		_gui.FileDialogs.OpenFile(Ktisis.Locale.Translate("file.dialog.chara.load"), handler, CharaFileOptions);
	}

	public void OpenPoseFile(Action<string, PoseFile> handler)
	{
		_gui.FileDialogs.OpenFile(Ktisis.Locale.Translate("file.dialog.pose.load"), delegate(string path, PoseFile file)
		{
			file.ConvertLegacyBones();
			handler(path, file);
		}, ImportPoseFileOptions, DialogType.Pose);
	}

	public void OpenMcdfFile(Action<string> handler)
	{
		_gui.FileDialogs.OpenFile(Ktisis.Locale.Translate("file.dialog.mcdf.load"), handler, McdfFileOptions);
	}

	public void OpenSceneFile(Action<string> handler)
	{
		_gui.FileDialogs.OpenFile("Open Scene File", handler, SceneFileOptions);
	}

	public void OpenLightFile(Action<string, LightFile> handler)
	{
		_gui.FileDialogs.OpenFile(Ktisis.Locale.Translate("file.dialog.light.load"), handler, LightFileOptions);
	}

	public void OpenReferenceImages(Action<string> handler)
	{
		_gui.FileDialogs.OpenImage(Ktisis.Locale.Translate("file.dialog.image.load"), handler);
	}

	public void ExportCharaFile(CharaFile file)
	{
		FileDialogOptions charaFileOptions = CharaFileOptions;
		charaFileOptions.DefaultFileName = file.Nickname;
		_gui.FileDialogs.SaveFile(Ktisis.Locale.Translate("file.dialog.chara.save"), file, charaFileOptions);
	}

	public void ExportPoseFile(PoseFile file)
	{
		_gui.FileDialogs.SaveFile(Ktisis.Locale.Translate("file.dialog.pose.save"), file, ExportPoseFileOptions);
	}

	public void ExportSceneFile(SceneFile file)
	{
		FileDialogOptions sceneFileOptions = SceneFileOptions;
		_gui.FileDialogs.SaveFile("Export Scene File", file, sceneFileOptions);
	}

	public void ExportLightFile(LightFile file)
	{
		FileDialogOptions lightFileOptions = LightFileOptions;
		lightFileOptions.DefaultFileName = file.Nickname;
		_gui.FileDialogs.SaveFile(Ktisis.Locale.Translate("file.dialog.light.save"), file, lightFileOptions);
	}
}
