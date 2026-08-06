using System.Numerics;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using Ktisis.Interop.Hooking;
using Ktisis.Scene.Entities.Game;
using Ktisis.Structs.Actors;

namespace Ktisis.Editor.Animation;

public class AnimationModule : HookModule
{
	private unsafe delegate void SetTimelineSpeedDelegate(AnimationTimeline* timeline, uint slot, float speed);

	private unsafe delegate void UpdatePosDelegate(CharacterEx* chara);

	public unsafe delegate bool PlayEmoteDelegate(EmoteController* controller, nint id, nint option, nint chair);

	private unsafe delegate bool SetEmoteModeDelegate(EmoteController* a1, EmoteModeEnum mode);

	private unsafe delegate nint EmoteControllerUpdateDrawOffsetDelegate(EmoteController* a1);

	private unsafe delegate nint CancelTimelineDelegate(AnimationContainer* a1, nint a2, nint a3);

	public unsafe delegate bool SetTimelineIdDelegate(AnimationTimeline* a1, ushort a2, nint a3);

	[Signature("83 FA 0E 73 22", DetourName = "SetTimelineSpeedDetour")]
	private Hook<SetTimelineSpeedDelegate>? SetTimelineSpeedHook;

	[Signature("E8 ?? ?? ?? ?? 84 DB 74 3A", DetourName = "UpdatePosDetour")]
	private Hook<UpdatePosDelegate> UpdatePosHook;

	[Signature("E8 ?? ?? ?? ?? 88 45 68")]
	public PlayEmoteDelegate PlayEmote;

	[Signature("E8 ?? ?? ?? ?? F6 46 10 01")]
	private SetEmoteModeDelegate SetEmoteMode;

	[Signature("E8 ?? ?? ?? ?? 0F BE 53 20")]
	private EmoteControllerUpdateDrawOffsetDelegate EmoteControllerUpdateDrawOffset;

	[Signature("E8 ?? ?? ?? ?? 80 7B 17 01")]
	private CancelTimelineDelegate CancelTimeline;

	[Signature("E8 ?? ?? ?? ?? 4C 8B BC 24 ?? ?? ?? ?? 4C 8D 9C 24 ?? ?? ?? ?? 49 8B 5B 40")]
	public SetTimelineIdDelegate SetTimelineId;

	public bool SpeedControlEnabled { get; set; }

	public AnimationModule(IHookMediator hook)
		: base(hook)
	{
	}

	public unsafe void SetTimelineSpeed(AnimationTimeline* timeline, uint slot, float speed)
	{
		SetTimelineSpeedHook?.Original(timeline, slot, speed);
	}

	private unsafe void SetTimelineSpeedDetour(AnimationTimeline* timeline, uint slot, float speed)
	{
		if (SpeedControlEnabled)
		{
			CharacterEx* ptr = (CharacterEx*)((byte*)timeline - 2624);
			if (ptr->IsGPose)
			{
				return;
			}
		}
		SetTimelineSpeedHook.Original(timeline, slot, speed);
	}

	private unsafe void UpdatePosDetour(CharacterEx* chara)
	{
		if (!chara->IsGPose)
		{
			UpdatePosHook.Original(chara);
		}
	}

	public unsafe void SetPose(ActorEntity actor, PoseModeEnum poseMode, byte pose)
	{
		EmoteModeEnum emoteModeEnum = poseMode switch
		{
			PoseModeEnum.Battle => EmoteModeEnum.Normal, 
			PoseModeEnum.SitGround => EmoteModeEnum.SitGround, 
			PoseModeEnum.SitChair => EmoteModeEnum.SitChair, 
			PoseModeEnum.Sleeping => EmoteModeEnum.Sleeping, 
			_ => EmoteModeEnum.Normal, 
		};
		CharacterEx* ptr = (CharacterEx*)(actor.IsValid ? actor.Character : null);
		if (ptr != null)
		{
			bool num = emoteModeEnum == EmoteModeEnum.SitChair;
			Vector3 drawObjectOffset;
			Vector3 cameraOffsetSmooth;
			if (num)
			{
				drawObjectOffset = ptr->DrawObjectOffset;
				cameraOffsetSmooth = ptr->CameraOffsetSmooth;
			}
			else
			{
				drawObjectOffset = Vector3.Zero;
				cameraOffsetSmooth = Vector3.Zero;
			}
			byte pose2 = ptr->EmoteController.Pose;
			if (pose == byte.MaxValue)
			{
				pose = (byte)((pose2 != byte.MaxValue) ? pose2 : 0);
			}
			CancelTimeline(&ptr->Animation, 0, 0);
			SetEmoteMode(&ptr->EmoteController, emoteModeEnum);
			ptr->EmoteController.Mode = poseMode;
			ptr->EmoteController.Pose = pose;
			if (num)
			{
				ptr->EmoteController.IsDrawObjectOffset = false;
				EmoteControllerUpdateDrawOffset(&ptr->EmoteController);
				ptr->DrawObjectOffset = drawObjectOffset;
				ptr->CameraOffsetSmooth = cameraOffsetSmooth;
			}
		}
	}
}
