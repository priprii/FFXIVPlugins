using System;
using Dalamud.Interface;

namespace PyonPix.Structs.Ui;

public class UiTab(FontAwesomeIcon icon, string tooltip, Action draw, string? text = null)
{
	public FontAwesomeIcon Icon = icon;

	public string Tooltip = tooltip;

	public Action Draw = draw;

	public string? Text = text;
}
