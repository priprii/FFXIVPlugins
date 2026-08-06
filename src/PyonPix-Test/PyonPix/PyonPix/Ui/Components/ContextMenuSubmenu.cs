using System;
using System.Collections.Generic;
using Dalamud.Interface;

namespace PyonPix.Ui.Components;

public class ContextMenuSubmenu : ContextMenuItem
{
	public Func<string> Text;

	public FontAwesomeIcon? Icon;

	public List<ContextMenuItem> SubItems;

	public Func<bool>? IsDisabled;

	public ContextMenuSubmenu(string text, List<ContextMenuItem> subItems, FontAwesomeIcon? icon = null, Func<bool>? isDisabled = null)
	{
		Text = () => text;
		Icon = icon;
		SubItems = subItems;
		IsDisabled = isDisabled;
	}

	public ContextMenuSubmenu(Func<string> text, List<ContextMenuItem> subItems, FontAwesomeIcon? icon = null, Func<bool>? isDisabled = null)
	{
		Text = text;
		Icon = icon;
		SubItems = subItems;
		IsDisabled = isDisabled;
	}
}
