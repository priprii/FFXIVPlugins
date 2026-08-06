using System;
using System.Threading.Tasks;
using Ktisis.Data.Files;
using Ktisis.Editor.Posing.Attachment;
using Ktisis.Editor.Posing.Data;
using Ktisis.Editor.Posing.Ik;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;

namespace Ktisis.Editor.Posing.Types;

public interface IPosingManager : IDisposable
{
	bool IsValid { get; }

	IAttachManager Attachments { get; }

	PoseMemento? StashedPose { get; set; }

	DateTime? StashedAt { get; set; }

	string? StashedFrom { get; set; }

	bool IsEnabled { get; }

	bool IsIkEnabled { get; set; }

	void Initialize();

	void SetEnabled(bool enable);

	Task SyncFaceModelSpace(ActorEntity actor);

	IIkController CreateIkController();

	Task ApplyReferencePose(EntityPose pose);

	Task ApplyPartialReferencePose(EntityPose pose, int partialIndex);

	Task ApplyPoseFile(EntityPose pose, PoseFile file, PoseMode modes = PoseMode.All, PoseTransforms transforms = PoseTransforms.Rotation, bool selectedBones = false, bool includeDescendants = false, bool anchorGroups = false, bool excludeEars = false);

	Task<PoseFile> SavePoseFile(EntityPose pose);

	Task StashPose(EntityPose pose);

	Task ApplyStashedPose(EntityPose pose);

	Task ApplyFlipPose(EntityPose pose);
}
