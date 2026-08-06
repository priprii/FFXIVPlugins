using System;

namespace PyonPix.Ui.Components;

public class ContextMenuCheckbox : ContextMenuItem
{
	public Func<string> Text;

	public Func<bool> GetValue;

	public Action<bool> SetValue;

	public bool CloseOnClick;

	public Func<bool>? IsDisabled;

	public ContextMenuTint DisabledTint;

	public Func<(string, string?)?>? Tooltip;

	public ContextMenuCheckbox(Func<string> text, Func<bool> getValue, Action<bool> setValue, bool closeOnClick = false, Func<bool>? isDisabled = null, ContextMenuTint disabledTint = ContextMenuTint.Both, Func<(string, string?)?>? tooltip = null)
	{
		Text = text;
		GetValue = getValue;
		SetValue = setValue;
		CloseOnClick = closeOnClick;
		IsDisabled = isDisabled;
		DisabledTint = disabledTint;
		Tooltip = tooltip;
	}

	public ContextMenuCheckbox(string text, Func<bool> getValue, Action<bool> setValue, bool closeOnClick = false, Func<bool>? isDisabled = null, ContextMenuTint disabledTint = ContextMenuTint.Both, Func<(string, string?)?>? tooltip = null)
		: this(() => text, getValue, setValue, closeOnClick, isDisabled, disabledTint, tooltip)
	{
	}
}
