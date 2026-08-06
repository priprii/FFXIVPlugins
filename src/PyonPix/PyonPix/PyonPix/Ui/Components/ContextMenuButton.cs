using System;
using Dalamud.Interface;

namespace PyonPix.Ui.Components;

public class ContextMenuButton : ContextMenuItem
{
	public Func<string> Text;

	public FontAwesomeIcon? Icon;

	public Action? OnClick;

	public bool CloseOnClick;

	public Func<bool>? IsActive;

	public Func<bool>? IsDisabled;

	public ContextMenuTint ActiveTint;

	public ContextMenuTint DisabledTint;

	public Func<(string, string?)?>? Tooltip;

	public ContextMenuButton(Func<string> text, Action? onClick = null, bool closeOnClick = true, FontAwesomeIcon? icon = null, Func<bool>? isActive = null, Func<bool>? isDisabled = null, ContextMenuTint activeTint = ContextMenuTint.Both, ContextMenuTint disabledTint = ContextMenuTint.Both, Func<(string, string?)?>? tooltip = null)
	{
		Text = text;
		OnClick = onClick;
		CloseOnClick = closeOnClick;
		Icon = icon;
		IsActive = isActive;
		IsDisabled = isDisabled;
		ActiveTint = activeTint;
		DisabledTint = disabledTint;
		Tooltip = tooltip;
	}

	public ContextMenuButton(string text, Action? onClick = null, bool closeOnClick = true, FontAwesomeIcon? icon = null, Func<bool>? isActive = null, Func<bool>? isDisabled = null, ContextMenuTint activeTint = ContextMenuTint.Both, ContextMenuTint disabledTint = ContextMenuTint.Both, Func<(string, string?)?>? tooltip = null)
		: this(() => text, onClick, closeOnClick, icon, isActive, isDisabled, activeTint, disabledTint, tooltip)
	{
	}
}
