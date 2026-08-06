using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Ktisis.Events;

namespace Ktisis.Interface.Types;

public abstract class KtisisWindow : Window
{
	public delegate void ClosedDelegate(KtisisWindow window);

	private readonly Event<Action<KtisisWindow>> _closedEvent = new Event<Action<KtisisWindow>>();

	internal string _localeWindowName;

	internal string _windowId;

	public event ClosedDelegate Closed
	{
		add
		{
			_closedEvent.Add(value.Invoke);
		}
		remove
		{
			_closedEvent.Remove(value.Invoke);
		}
	}

	protected KtisisWindow(string localeWindowName, ImGuiWindowFlags flags = (ImGuiWindowFlags)0, string windowId = "", bool forceMainWindow = false)
		: base(Ktisis.Locale.Translate(localeWindowName) + windowId, flags, forceMainWindow)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		_localeWindowName = localeWindowName;
		_windowId = windowId;
		((Window)this).RespectCloseHotkey = false;
		Ktisis.Locale.LocaleChanged += ChangeWindowLocale;
	}

	public void Open()
	{
		((Window)this).IsOpen = true;
	}

	public void Close()
	{
		try
		{
			if (!((Window)this).IsOpen)
			{
				((Window)this).OnClose();
			}
		}
		finally
		{
			((Window)this).IsOpen = false;
		}
	}

	private void ChangeWindowLocale()
	{
		((Window)this).WindowName = Ktisis.Locale.Translate(_localeWindowName ?? "") + _windowId;
	}

	public virtual void OnCreate()
	{
	}

	public override void OnClose()
	{
		Ktisis.Locale.LocaleChanged -= ChangeWindowLocale;
		_closedEvent.Invoke(this);
	}
}
