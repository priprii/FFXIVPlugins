using System;
using System.Threading.Tasks;
using Ktisis.Data.Files;
using Ktisis.Interface.Types;
using Ktisis.Interface.Windows;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Scene.Entities.World;

namespace Ktisis.Interface.Editor.Types;

public interface IEditorInterface
{
	void Prepare();

	void OpenConfigWindow();

	void ToggleWorkspaceWindow();

	void ToggleDebugWindow();

	void OpenCameraWindow();

	void OpenEnvironmentWindow();

	ObjectWindow GetObjectWindow();

	void OpenObjectEditor(bool forceOpen = false);

	void OpenPosingWindow();

	void OpenSceneWindow();

	void OpenSceneCreateMenu();

	void OpenActorCreateMenu();

	void OpenLightCreateMenu();

	void OpenOverlayCreateMenu();

	void OpenSceneEntityMenu(SceneEntity entity);

	void OpenAssignCollection(ActorEntity entity);

	void OpenApplyDesign(ActorEntity entity);

	void OpenAssignCProfile(ActorEntity entity);

	void OpenOverworldActorList();

	void RefreshSceneEntities();

	void SelectAllEntities();

	void OpenRenameEntity(SceneEntity entity);

	void OpenSavePreset(ActorEntity actorEntity);

	void OpenActorEditor(ActorEntity actor);

	void OpenLightEditor(LightEntity light);

	bool OpenEditor<T, TA>(TA entity) where T : EntityEditWindow<TA> where TA : SceneEntity;

	void OpenEditorFor(SceneEntity entity);

	void OpenCharaImport(ActorEntity actor, bool openNpc = false);

	Task OpenCharaExport(ActorEntity actor);

	void OpenPoseImport(ActorEntity actor);

	Task OpenPoseExport(EntityPose pose);

	Task OpenLightExport(LightEntity light);

	void OpenCharaFile(Action<string, CharaFile> handler);

	void OpenPoseFile(Action<string, PoseFile> handler);

	void OpenMcdfFile(Action<string> handler);

	void OpenLightFile(Action<string, LightFile> handler);

	void OpenSceneFile(Action<string> handler);

	void OpenReferenceImages(Action<string> handler);

	void ExportCharaFile(CharaFile file);

	void ExportPoseFile(PoseFile file);

	void ExportSceneFile(SceneFile file);
}
