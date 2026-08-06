using System;
using Dalamud.Interface;

namespace PyonPix.Ui.Components;

public class ContextMenuSubText : ContextMenuItem
{
	public Func<string> Text;

	public FontAwesomeIcon? Icon;

	public ContextMenuSubText(Func<string> text, FontAwesomeIcon? icon = null)
	{
		Text = text;
		Icon = icon;
	}

	public ContextMenuSubText(string text, FontAwesomeIcon? icon = null)
		: this(() => text, icon)
	{
	}
}
