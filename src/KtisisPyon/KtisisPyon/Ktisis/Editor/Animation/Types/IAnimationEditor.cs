using FFXIVClientStructs.Havok.Animation.Playback.Control.Default;
using Ktisis.Editor.Animation.Game;
using Ktisis.Structs.Actors;

namespace Ktisis.Editor.Animation.Types;

public interface IAnimationEditor
{
	bool SpeedControlEnabled { get; set; }

	bool Posing { get; }

	bool IsWeaponDrawn { get; }

	bool TryGetModeAndPose(out PoseModeEnum mode, out int pose);

	int GetPoseCount(PoseModeEnum poseMode);

	void SetPose(PoseModeEnum poseMode, byte pose = byte.MaxValue);

	void PlayAnimation(GameAnimation animation, bool playStart = true);

	void PlayTimeline(uint id);

	AnimationTimeline GetTimeline();

	void SetForceTimeline(ushort id);

	void SetTimelineSpeed(uint slot, float speed);

	void ResetTimelineSpeeds();

	void DoPoseExpression(uint id);

	unsafe hkaDefaultAnimationControl* GetHkaControl(int index);

	unsafe float? GetHkaDuration(hkaDefaultAnimationControl* control);

	unsafe float? GetHkaLocalTime(hkaDefaultAnimationControl* control);

	unsafe void SetHkaLocalTime(hkaDefaultAnimationControl* control, float time);

	void ToggleWeapon();
}
