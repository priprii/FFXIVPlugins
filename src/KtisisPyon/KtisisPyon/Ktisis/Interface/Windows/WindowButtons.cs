using System;
using System.Runtime.CompilerServices;
using Dalamud.Interface;

namespace Ktisis.Interface.Windows;

internal record WindowButtons(DrawContentDelegate Window, FontAwesomeIcon Icon, string TooltipText, Type WindowType)
{
	[CompilerGenerated]
	public void Deconstruct(out DrawContentDelegate Window, out FontAwesomeIcon Icon, out string TooltipText, out Type WindowType)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected I4, but got Unknown
		Window = this.Window;
		Icon = (FontAwesomeIcon)(int)this.Icon;
		TooltipText = this.TooltipText;
		WindowType = this.WindowType;
	}
}
