using System.Drawing;
using Dalamud.Game.ClientState.Keys;
using Ktisis.Actions.Attributes;
using Ktisis.Actions.Binds;
using Ktisis.Actions.Types;
using Ktisis.Core.Types;
using Ktisis.Data.Config.Actions;
using Ktisis.Data.Config.Sections;
using KtisisPyon.Common.Utility;

namespace Ktisis.Actions.Handlers.Output;

[Action("Output_HiRes_Toggle")]
public class ToggleHiResAction(IPluginContext ctx) : KeyAction(ctx)
{
	public override KeybindInfo BindInfo { get; } = new KeybindInfo
	{
		Trigger = KeybindTrigger.OnRelease,
		Default = new ActionKeybind
		{
			Enabled = true,
			Combo = new KeyCombo((VirtualKey)120)
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
		PyonConfig pyon = ctx.Config.File.Pyon;
		if (pyon.DefaultSize == Size.Empty || pyon.HiResSize == Size.Empty)
		{
			return false;
		}
		Win32.SetWinRes(pyon);
		return true;
	}
}
