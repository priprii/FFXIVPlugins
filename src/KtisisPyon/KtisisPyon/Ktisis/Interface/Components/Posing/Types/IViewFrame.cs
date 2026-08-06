using System.Collections.Generic;
using Ktisis.Data.Config.Pose2D;
using Ktisis.Scene.Entities.Skeleton;

namespace Ktisis.Interface.Components.Posing.Types;

public interface IViewFrame
{
	void DrawView(PoseViewEntry entry, float width, float height, IDictionary<string, string>? templates = null);

	void DrawBones(EntityPose pose);
}
