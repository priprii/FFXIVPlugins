using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using GLib.Popups;

namespace Ktisis.Interface.Types;

public abstract class KtisisPopup(string id, ImGuiWindowFlags flags = (ImGuiWindowFlags)0) : IPopup
{
	private bool _isOpen;

	private bool _isOpening;

	private bool _isClosing;

	public bool IsOpen
	{
		get
		{
			if (!_isOpen)
			{
				return _isOpening;
			}
			return true;
		}
	}

	public void Open()
	{
		_isOpening = true;
	}

	public void Close()
	{
		_isClosing = true;
	}

	public bool Draw()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (_isOpening)
		{
			_isOpening = false;
			ImGui.OpenPopup(ImU8String.op_Implicit(id), (ImGuiPopupFlags)0);
		}
		_isOpen = ImGui.IsPopupOpen(ImU8String.op_Implicit(id), (ImGuiPopupFlags)0) && !_isClosing;
		if (!_isOpen)
		{
			return false;
		}
		PopupDisposable val = ImRaii.Popup(ImU8String.op_Implicit(id), flags);
		try
		{
			if (!val.Success)
			{
				return false;
			}
			try
			{
				OnDraw();
			}
			catch (Exception value)
			{
				Ktisis.Log.Error($"Error drawing popup:\n{value}");
			}
			return true;
		}
		finally
		{
			((PopupDisposable)(ref val)).Dispose();
		}
	}

	protected abstract void OnDraw();
}
