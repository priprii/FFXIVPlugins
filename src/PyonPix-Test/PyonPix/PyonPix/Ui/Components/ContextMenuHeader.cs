using System;
using Dalamud.Interface;

namespace PyonPix.Ui.Components;

public class ContextMenuHeader : ContextMenuItem
{
	public Func<string> Text;

	public FontAwesomeIcon? Icon;

	public ContextMenuHeader(Func<string> text, FontAwesomeIcon? icon = null)
	{
		Text = text;
		Icon = icon;
	}

	public ContextMenuHeader(string text, FontAwesomeIcon? icon = null)
		: this(() => text, icon)
	{
	}
}
