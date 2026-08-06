using System;
using Ktisis.Actions.Types;
using Ktisis.Data.Config.Actions;
using Ktisis.Editor.Actions.Input;
using Ktisis.Editor.Context.Types;

namespace Ktisis.Editor.Actions;

public class ActionManager : IActionManager, IDisposable
{
	private readonly IEditorContext _ctx;

	public IInputManager Input { get; }

	public IHistoryManager History { get; }

	public ActionManager(IEditorContext ctx, IInputManager input)
	{
		_ctx = ctx;
		Input = input;
		History = new HistoryManager();
	}

	public void Initialize()
	{
		Ktisis.Log.Verbose("Initializing input manager...");
		try
		{
			Input.Initialize();
			RegisterKeybinds();
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to initialize input manager:\n{value}");
		}
	}

	private void RegisterKeybinds()
	{
		foreach (KeyAction item in _ctx.Plugin.Actions.GetBindable())
		{
			RegisterKeybind(item);
		}
	}

	private void RegisterKeybind(KeyAction action)
	{
		ActionKeybind keybind = action.GetKeybind();
		Input.Register(keybind, action.Invoke, action.BindInfo.Trigger);
	}

	public void Dispose()
	{
		try
		{
			History.Clear();
			Input.Dispose();
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to dispose action manager:\n{value}");
		}
		finally
		{
			GC.SuppressFinalize(this);
		}
	}
}
