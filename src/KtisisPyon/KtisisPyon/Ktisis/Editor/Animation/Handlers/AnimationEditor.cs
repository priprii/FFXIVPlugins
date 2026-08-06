using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FFXIVClientStructs.Havok.Animation.Animation;
using FFXIVClientStructs.Havok.Animation.Playback.Control;
using FFXIVClientStructs.Havok.Animation.Playback.Control.Default;
using Ktisis.Common.Extensions;
using Ktisis.Editor.Animation.Game;
using Ktisis.Editor.Animation.Types;
using Ktisis.Editor.Context.Types;
using Ktisis.Scene.Entities.Game;
using Ktisis.Structs.Actors;

namespace Ktisis.Editor.Animation.Handlers;

public class AnimationEditor(IAnimationManager mgr, IEditorContext ctx, ActorEntity actor) : IAnimationEditor
{
	private static readonly List<uint> IdlePoses;

	private static readonly Dictionary<PoseModeEnum, int> StancePoses;

	private const ushort IdlePose = 3;

	private const ushort DrawWeaponId = 1;

	private const ushort SheatheWeaponId = 2;

	private const uint BattleIdle = 34u;

	private const uint BattlePose = 93u;

	public bool SpeedControlEnabled
	{
		get
		{
			return mgr.SpeedControlEnabled;
		}
		set
		{
			mgr.SpeedControlEnabled = value;
		}
	}

	public bool Posing => ctx.Posing.IsEnabled;

	public unsafe bool IsWeaponDrawn
	{
		get
		{
			CharacterEx* chara = GetChara();
			if (chara != null)
			{
				return IsWeaponDrawnFor(chara);
			}
			return false;
		}
	}

	private unsafe CharacterEx* GetChara()
	{
		if (!actor.IsValid)
		{
			return null;
		}
		return (CharacterEx*)actor.Character;
	}

	public unsafe bool TryGetModeAndPose(out PoseModeEnum mode, out int pose)
	{
		CharacterEx* ptr = (CharacterEx*)(actor.IsValid ? actor.Character : null);
		if (ptr == null)
		{
			mode = PoseModeEnum.None;
			pose = 0;
			return false;
		}
		PoseModeEnum poseModeEnum2;
		switch (ptr->EmoteMode)
		{
		case EmoteModeEnum.SitGround:
			poseModeEnum2 = PoseModeEnum.SitGround;
			break;
		case EmoteModeEnum.SitChair:
			poseModeEnum2 = PoseModeEnum.SitChair;
			break;
		case EmoteModeEnum.Sleeping:
			poseModeEnum2 = PoseModeEnum.Sleeping;
			break;
		default:
		{
			PoseModeEnum mode2 = ptr->EmoteController.Mode;
			PoseModeEnum poseModeEnum = ((mode2 != PoseModeEnum.None) ? mode2 : PoseModeEnum.Idle);
			poseModeEnum2 = poseModeEnum;
			break;
		}
		}
		mode = poseModeEnum2;
		pose = ptr->EmoteController.Pose;
		return true;
	}

	public int GetPoseCount(PoseModeEnum poseMode)
	{
		if (poseMode == PoseModeEnum.Idle || poseMode == PoseModeEnum.None)
		{
			return IsWeaponDrawn ? 2 : IdlePoses.Count;
		}
		return StancePoses.GetValueOrDefault(poseMode, 1);
	}

	public void SetPose(PoseModeEnum poseMode, byte pose = byte.MaxValue)
	{
		mgr.SetPose(actor, poseMode, pose);
		if ((poseMode != PoseModeEnum.Idle && poseMode != PoseModeEnum.None) || 1 == 0)
		{
			return;
		}
		if (pose == 0)
		{
			mgr.PlayTimeline(actor, IsWeaponDrawn ? 34u : 3u);
		}
		else if (IsWeaponDrawn)
		{
			mgr.PlayEmote(actor, 93u);
		}
		else if (pose < IdlePoses.Count)
		{
			uint num = IdlePoses[pose];
			if (num != 0)
			{
				mgr.PlayEmote(actor, num);
			}
		}
	}

	public void PlayAnimation(GameAnimation animation, bool playStart = true)
	{
		if (!(animation is EmoteAnimation { Index: 0 } emoteAnimation) || !playStart || !mgr.PlayEmote(actor, emoteAnimation.EmoteId))
		{
			mgr.PlayTimeline(actor, animation.TimelineId);
		}
	}

	public void PlayTimeline(uint id)
	{
		mgr.PlayTimeline(actor, id);
	}

	public unsafe AnimationTimeline GetTimeline()
	{
		CharacterEx* chara = GetChara();
		if (chara == null)
		{
			return default(AnimationTimeline);
		}
		return chara->Animation.Timeline;
	}

	public unsafe void SetForceTimeline(ushort id)
	{
		CharacterEx* chara = GetChara();
		if (chara != null)
		{
			chara->Animation.Timeline.ActionTimelineId = id;
		}
	}

	public void SetTimelineSpeed(uint slot, float speed)
	{
		mgr.SetTimelineSpeed(actor, slot, speed);
	}

	public void ResetTimelineSpeeds()
	{
		mgr.ResetTimelineSpeeds(actor);
	}

	public async void DoPoseExpression(uint id)
	{
		mgr.PlayTimeline(actor, id);
		await ctx.Posing.SyncFaceModelSpace(actor);
	}

	public unsafe hkaDefaultAnimationControl* GetHkaControl(int index)
	{
		return actor.Actor.GetDefaultControlForIndex(index);
	}

	public unsafe float? GetHkaDuration(hkaDefaultAnimationControl* control)
	{
		if (control == null)
		{
			return null;
		}
		return ((hkaAnimation)((hkaAnimationBinding)((hkaAnimationControl)(&((hkaDefaultAnimationControl)control).hkaAnimationControl)).Binding.ptr).Animation.ptr).Duration;
	}

	public unsafe float? GetHkaLocalTime(hkaDefaultAnimationControl* control)
	{
		if (control == null)
		{
			return null;
		}
		return ((hkaAnimationControl)(&((hkaDefaultAnimationControl)control).hkaAnimationControl)).LocalTime;
	}

	public unsafe void SetHkaLocalTime(hkaDefaultAnimationControl* control, float time)
	{
		((hkaAnimationControl)(&((hkaDefaultAnimationControl)control).hkaAnimationControl)).LocalTime = time;
	}

	public unsafe void ToggleWeapon()
	{
		CharacterEx* chara = GetChara();
		if (chara != null)
		{
			bool flag = IsWeaponDrawnFor(chara);
			PlayTimeline((!flag) ? 1u : 2u);
			chara->CombatFlags ^= CombatFlags.WeaponDrawn;
		}
	}

	private unsafe static bool IsWeaponDrawnFor(CharacterEx* chara)
	{
		return chara->CombatFlags.HasFlag(CombatFlags.WeaponDrawn);
	}

	static AnimationEditor()
	{
		int num = 7;
		List<uint> list = new List<uint>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<uint> span = CollectionsMarshal.AsSpan(list);
		span[0] = 0u;
		span[1] = 91u;
		span[2] = 92u;
		span[3] = 107u;
		span[4] = 108u;
		span[5] = 218u;
		span[6] = 219u;
		IdlePoses = list;
		StancePoses = new Dictionary<PoseModeEnum, int>
		{
			{
				PoseModeEnum.SitGround,
				4
			},
			{
				PoseModeEnum.SitChair,
				5
			},
			{
				PoseModeEnum.Sleeping,
				3
			}
		};
	}
}
