using Dalamud.Game.ClientState.Keys;
using Ktisis.Actions.Attributes;
using Ktisis.Actions.Binds;
using Ktisis.Actions.Types;
using Ktisis.Core.Types;
using Ktisis.Data.Config.Actions;

namespace Ktisis.Actions.Handlers.Camera;

[Action("Camera_SetNext")]
public class CameraNextAction(IPluginContext ctx) : KeyAction(ctx)
{
	public override KeybindInfo BindInfo { get; } = new KeybindInfo
	{
		Trigger = KeybindTrigger.OnDown,
		Default = new ActionKeybind
		{
			Enabled = true,
			Combo = new KeyCombo((VirtualKey)221)
		}
	};

	public override bool CanInvoke()
	{
		return base.Context.Editor != null;
	}

	public override bool Invoke()
	{
		if (!CanInvoke())
		{
			return false;
		}
		base.Context.Editor.Cameras.SetNext();
		return true;
	}
}
