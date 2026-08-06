using System;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using Ktisis.Editor.Animation.Handlers;
using Ktisis.Editor.Animation.Types;
using Ktisis.Editor.Context.Types;
using Ktisis.Interop.Hooking;
using Ktisis.Scene.Entities.Game;
using Ktisis.Structs.Actors;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace Ktisis.Editor.Animation;

public class AnimationManager : IAnimationManager
{
	private readonly IEditorContext _ctx;

	private readonly HookScope _scope;

	private readonly IDataManager _data;

	private readonly IFramework _framework;

	private AnimationModule? Module { get; set; }

	private ExcelSheet<ActionTimeline>? Timelines { get; set; }

	public bool SpeedControlEnabled
	{
		get
		{
			return Module?.SpeedControlEnabled ?? false;
		}
		set
		{
			if (Module != null)
			{
				Module.SpeedControlEnabled = value;
			}
		}
	}

	public AnimationManager(IEditorContext ctx, HookScope scope, IDataManager data, IFramework framework)
	{
		_ctx = ctx;
		_scope = scope;
		_data = data;
		_framework = framework;
	}

	public void Initialize()
	{
		Ktisis.Log.Verbose("Initializing character manager...");
		try
		{
			Module = _scope.Create<AnimationModule>(Array.Empty<object>());
			Module.Initialize();
			Module.EnableAll();
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to initialize animation module:\n{value}");
		}
		Timelines = _data.GetExcelSheet<ActionTimeline>((ClientLanguage?)null, (string)null);
	}

	public IAnimationEditor GetAnimationEditor(ActorEntity actor)
	{
		return new AnimationEditor(this, _ctx, actor);
	}

	public void SetPose(ActorEntity actor, PoseModeEnum poseMode, byte pose = byte.MaxValue)
	{
		_framework.RunOnFrameworkThread((Action)delegate
		{
			Module?.SetPose(actor, poseMode, pose);
		});
	}

	public unsafe bool PlayEmote(ActorEntity actor, uint id)
	{
		CharacterEx* character = (CharacterEx*)actor.Character;
		if (character == null)
		{
			return false;
		}
		character->Animation.Timeline.ActionTimelineId = 0;
		character->EmoteController.IsForceDefaultPose = false;
		return Module.PlayEmote(&character->EmoteController, (nint)id, 0, 0);
	}

	public unsafe bool PlayTimeline(ActorEntity actor, uint id)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		ActionTimeline? val = Timelines?.GetRow(id);
		if (!val.HasValue)
		{
			return false;
		}
		CharacterEx* ptr = (CharacterEx*)(actor.IsValid ? actor.Character : null);
		if (ptr == null)
		{
			return false;
		}
		ptr->Animation.Timeline.ActionTimelineId = 0;
		ActionTimeline value = val.Value;
		if (((ActionTimeline)(ref value)).Pause)
		{
			ptr->Mode = 3;
			ptr->EmoteMode = EmoteModeEnum.Normal;
		}
		else if (ptr->Mode == 3 && ptr->EmoteMode == EmoteModeEnum.Normal)
		{
			ptr->Mode = 1;
		}
		if (Module != null)
		{
			return Module.SetTimelineId(&ptr->Animation.Timeline, (ushort)id, IntPtr.Zero);
		}
		return false;
	}

	public unsafe void SetTimelineSpeed(ActorEntity actor, uint slot, float speed)
	{
		CharacterEx* ptr = (CharacterEx*)(actor.IsValid ? actor.Character : null);
		if (ptr != null)
		{
			Module?.SetTimelineSpeed(&ptr->Animation.Timeline, slot, speed);
		}
	}

	public unsafe void ResetTimelineSpeeds(ActorEntity actor)
	{
		CharacterEx* ptr = (CharacterEx*)(actor.IsValid ? actor.Character : null);
		if (ptr != null)
		{
			TimelineSlot[] values = Enum.GetValues<TimelineSlot>();
			foreach (TimelineSlot slot in values)
			{
				Module?.SetTimelineSpeed(&ptr->Animation.Timeline, (uint)slot, 1f);
			}
		}
	}
}
