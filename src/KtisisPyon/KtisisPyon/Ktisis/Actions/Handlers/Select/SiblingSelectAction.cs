using System.Linq;
using Dalamud.Game.ClientState.Keys;
using Ktisis.Actions.Attributes;
using Ktisis.Actions.Binds;
using Ktisis.Actions.Types;
using Ktisis.Core.Types;
using Ktisis.Data.Config.Actions;
using Ktisis.Editor.Selection;
using Ktisis.Scene.Entities.Skeleton;

namespace Ktisis.Actions.Handlers.Select;

[Action("Select_Sibling")]
public class SiblingSelectAction(IPluginContext ctx) : KeyAction(ctx)
{
	public override KeybindInfo BindInfo { get; } = new KeybindInfo
	{
		Trigger = KeybindTrigger.OnDown,
		Default = new ActionKeybind
		{
			Enabled = true,
			Combo = new KeyCombo((VirtualKey)220)
		}
	};

	public override bool CanInvoke()
	{
		int? num = base.Context.Editor?.Transform.Target?.Targets.Count();
		if (num.HasValue)
		{
			return num == 1;
		}
		return false;
	}

	public override bool Invoke()
	{
		if (!CanInvoke())
		{
			return false;
		}
		if (!(base.Context.Editor.Transform.Target.Primary is BoneNode boneNode))
		{
			return false;
		}
		BoneNode boneNode2 = boneNode.Pose.TryResolveSibling(boneNode);
		if (boneNode2 == null)
		{
			return false;
		}
		base.Context.Editor.Selection.Select(boneNode2, SelectMode.Multiple);
		return true;
	}
}
