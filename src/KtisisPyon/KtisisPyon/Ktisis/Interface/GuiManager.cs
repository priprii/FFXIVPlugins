using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using GLib.Popups;
using GLib.Popups.ImFileDialog;
using Ktisis.Core;
using Ktisis.Core.Attributes;
using Ktisis.Interface.Types;
using Ktisis.Interface.Windows;
using Ktisis.Localization;

namespace Ktisis.Interface;

[Singleton]
public class GuiManager : IDisposable
{
	private readonly DIBuilder _di;

	private readonly IUiBuilder _uiBuilder;

	private readonly WindowSystem _ws = new WindowSystem("KtisisPyon");

	private readonly PopupManager _popup = new PopupManager();

	private bool _hasConfig;

	private readonly List<KtisisWindow> _windows = new List<KtisisWindow>();

	public readonly LocaleManager Locale;

	public readonly FileDialogManager FileDialogs;

	public GuiManager(DIBuilder di, IUiBuilder uiBuilder, LocaleManager locale, FileDialogManager dialogs)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		_di = di;
		_uiBuilder = uiBuilder;
		Locale = locale;
		FileDialogs = dialogs;
	}

	public void Initialize()
	{
		_uiBuilder.DisableGposeUiHide = true;
		_uiBuilder.Draw += Draw;
		FileDialogs.OnOpenDialog += OnOpenDialog;
		FileDialogs.Initialize();
	}

	internal void AddSettings()
	{
		_hasConfig = true;
		_uiBuilder.OpenConfigUi += OnOpenConfigUi;
	}

	private void Draw()
	{
		_ws.Draw();
		_popup.Draw();
		FileDialogs.Draw();
	}

	public T Add<T>(T inst) where T : KtisisWindow
	{
		_ws.AddWindow((IWindow)(object)inst);
		_windows.Add(inst);
		inst.Closed += OnClose;
		Ktisis.Log.Verbose($"Added window: {((object)inst).GetType().Name} ('{((Window)inst).WindowName}')");
		return inst;
	}

	public T? Get<T>() where T : KtisisWindow
	{
		return (T)_windows.Find((KtisisWindow win) => win is T);
	}

	public bool Remove(KtisisWindow inst)
	{
		bool num = _windows.Remove(inst);
		if (num)
		{
			_ws.RemoveWindow((IWindow)(object)inst);
			inst.Closed -= OnClose;
			if (inst is IDisposable disposable)
			{
				disposable.Dispose();
			}
			Ktisis.Log.Verbose($"Removed window: {((object)inst).GetType().Name} ('{((Window)inst).WindowName}')");
		}
		return num;
	}

	public T Create<T>(params object[] parameters) where T : KtisisWindow
	{
		T val = _di.Create<T>(parameters);
		val.OnCreate();
		return Add(val);
	}

	public T CreatePopup<T>(params object[] parameters) where T : class, IPopup
	{
		return AddPopupSingleton(_di.Create<T>(parameters));
	}

	public T GetOrCreate<T>(params object[] parameters) where T : KtisisWindow
	{
		return Get<T>() ?? Create<T>(parameters);
	}

	public T AddPopup<T>(T popup) where T : class, IPopup
	{
		_popup.Add(popup);
		return popup;
	}

	public T AddPopupSingleton<T>(T popup) where T : class, IPopup
	{
		T popup2 = GetPopup<T>();
		if (popup2 != null)
		{
			_popup.Remove(popup2);
		}
		return AddPopup(popup);
	}

	public T? GetPopup<T>() where T : class, IPopup
	{
		return _popup.Get<T>();
	}

	private void OnClose(KtisisWindow window)
	{
		Ktisis.Log.Verbose($"Window {((object)window).GetType().Name} ('{((Window)window).WindowName}') closed, removing...");
		Remove(window);
	}

	private void OnOpenConfigUi()
	{
		((Window)GetOrCreate<ConfigWindow>(Array.Empty<object>())).Toggle();
	}

	private void OnOpenDialog(FileDialog dialog)
	{
		foreach (FileDialog item in _popup.GetAll<FileDialog>())
		{
			if (item.Title == dialog.Title)
			{
				item.Close();
			}
		}
		AddPopup(dialog);
	}

	internal void ResetWorkspace()
	{
		foreach (KtisisWindow item in from window in _windows.ToList()
			where ((object)window).GetType().BaseType != typeof(KtisisPopup)
			select window)
		{
			Remove(item);
		}
		_windows.Clear();
	}

	private void RemoveAll()
	{
		foreach (KtisisWindow item in _windows.ToList())
		{
			Remove(item);
		}
		_windows.Clear();
	}

	public void Dispose()
	{
		_uiBuilder.Draw -= Draw;
		if (_hasConfig)
		{
			_uiBuilder.OpenConfigUi -= OnOpenConfigUi;
			_hasConfig = false;
		}
		RemoveAll();
	}
}
