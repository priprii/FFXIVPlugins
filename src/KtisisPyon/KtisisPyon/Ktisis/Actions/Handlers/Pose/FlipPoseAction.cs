using Dalamud.Game.ClientState.Keys;
using Ktisis.Actions.Attributes;
using Ktisis.Actions.Binds;
using Ktisis.Actions.Types;
using Ktisis.Core.Types;
using Ktisis.Data.Config.Actions;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;

namespace Ktisis.Actions.Handlers.Pose;

[Action("Pose_FlipPose")]
public class FlipPoseAction(IPluginContext ctx) : KeyAction(ctx)
{
	public override KeybindInfo BindInfo { get; } = new KeybindInfo
	{
		Trigger = KeybindTrigger.OnDown,
		Default = new ActionKeybind
		{
			Enabled = true,
			Combo = new KeyCombo((VirtualKey)70, (VirtualKey)17)
		}
	};

	public override bool CanInvoke()
	{
		SceneEntity sceneEntity = base.Context.Editor?.Transform.Target?.Primary;
		if (sceneEntity != null)
		{
			if (!(sceneEntity is ActorEntity))
			{
				return sceneEntity is BoneNode;
			}
			return true;
		}
		return false;
	}

	public override bool Invoke()
	{
		if (!CanInvoke())
		{
			return false;
		}
		SceneEntity primary = base.Context.Editor.Transform.Target.Primary;
		EntityPose entityPose = ((primary is ActorEntity actorEntity) ? actorEntity.Pose : ((primary is BoneNode boneNode) ? boneNode.Pose : null));
		if (entityPose != null)
		{
			base.Context.Editor.Posing.ApplyFlipPose(entityPose);
		}
		return true;
	}
}
