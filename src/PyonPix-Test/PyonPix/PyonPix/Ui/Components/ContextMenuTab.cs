using System;
using System.Collections.Generic;
using Dalamud.Interface;

namespace PyonPix.Ui.Components;

public class ContextMenuTab
{
	public Func<string> Text;

	public FontAwesomeIcon? Icon;

	public List<ContextMenuItem> Items;

	public string Id { get; }

	public ContextMenuTab(string id, string text, List<ContextMenuItem> items, FontAwesomeIcon? icon = null)
		: this(id, () => text, items, icon)
	{
	}

	public ContextMenuTab(string id, Func<string> text, List<ContextMenuItem> items, FontAwesomeIcon? icon = null)
	{
		Id = id;
		Text = text;
		Items = items;
		Icon = icon;
	}
}
