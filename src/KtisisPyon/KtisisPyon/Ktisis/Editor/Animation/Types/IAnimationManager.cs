using Ktisis.Scene.Entities.Game;
using Ktisis.Structs.Actors;

namespace Ktisis.Editor.Animation.Types;

public interface IAnimationManager
{
	bool SpeedControlEnabled { get; set; }

	void Initialize();

	IAnimationEditor GetAnimationEditor(ActorEntity actor);

	void SetPose(ActorEntity actor, PoseModeEnum poseMode, byte pose = byte.MaxValue);

	bool PlayEmote(ActorEntity actor, uint id);

	bool PlayTimeline(ActorEntity actor, uint id);

	void SetTimelineSpeed(ActorEntity actor, uint slot, float speed);

	void ResetTimelineSpeeds(ActorEntity actor);
}
