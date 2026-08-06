using System.Linq;
using Dalamud.Game.ClientState.Keys;
using Ktisis.Actions.Attributes;
using Ktisis.Actions.Binds;
using Ktisis.Actions.Types;
using Ktisis.Core.Types;
using Ktisis.Data.Config.Actions;
using Ktisis.Editor.Selection;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Skeleton;

namespace Ktisis.Actions.Handlers.Select;

[Action("Select_Children")]
public class ChildSelectAction(IPluginContext ctx) : KeyAction(ctx)
{
	public override KeybindInfo BindInfo { get; } = new KeybindInfo
	{
		Trigger = KeybindTrigger.OnDown,
		Default = new ActionKeybind
		{
			Enabled = true,
			Combo = new KeyCombo((VirtualKey)220, (VirtualKey)17)
		}
	};

	public override bool CanInvoke()
	{
		return base.Context.Editor.Selection.GetSelected().Count() == 1;
	}

	public override bool Invoke()
	{
		if (!CanInvoke())
		{
			return false;
		}
		SceneEntity firstSelected = base.Context.Editor.Selection.GetFirstSelected();
		if (firstSelected == null)
		{
			return false;
		}
		if (firstSelected.Children.Any())
		{
			foreach (SceneEntity item in firstSelected.Recurse())
			{
				item.Select(SelectMode.Multiple);
			}
			return true;
		}
		BoneNode bone = firstSelected as BoneNode;
		if (bone != null)
		{
			foreach (BoneNode item2 in from b in bone.Pose.GetAllBones()
				where b.IsBoneDescendantOf(bone)
				select b)
			{
				item2.Select(SelectMode.Multiple);
			}
			return true;
		}
		return false;
	}
}
