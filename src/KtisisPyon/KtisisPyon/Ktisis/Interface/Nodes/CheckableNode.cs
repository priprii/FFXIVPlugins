using System;
using Dalamud.Bindings.ImGui;
using GLib.Popups.Context;

namespace Ktisis.Interface.Nodes;

public class CheckableNode : IContextMenuNode
{
	private readonly string _name;

	private readonly bool _state;

	private readonly Action _handler;

	public string? Shortcut;

	public CheckableNode(string name, bool state, Action handler)
	{
		_name = name;
		_state = state;
		_handler = handler;
	}

	public void Draw()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if ((Shortcut == null) ? ImGui.MenuItem(ImU8String.op_Implicit(_name), _state, true) : ImGui.MenuItem(ImU8String.op_Implicit(_name), ImU8String.op_Implicit(Shortcut), _state, true))
		{
			_handler();
		}
	}
}
