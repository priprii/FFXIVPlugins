using System;
using Ktisis.Core.Types;
using Ktisis.Data.Config;
using Ktisis.Editor.Actions;
using Ktisis.Editor.Animation.Types;
using Ktisis.Editor.Camera;
using Ktisis.Editor.Characters.Types;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Posing.Types;
using Ktisis.Editor.Selection;
using Ktisis.Editor.Transforms.Types;
using Ktisis.Interface;
using Ktisis.Interface.Editor.Types;
using Ktisis.Localization;
using Ktisis.Scene.Types;
using Ktisis.Services.Game;

namespace Ktisis.Editor.Context;

public class EditorContext : IEditorContext, IDisposable
{
	private readonly GPoseService _gpose;

	private EditorState? _state;

	public bool IsValid => _state?.IsValid ?? false;

	public bool IsGPosing => _gpose.IsGPosing;

	public bool ShowWorldObjects { get; set; }

	public IPluginContext Plugin { get; }

	private EditorState State
	{
		get
		{
			if (_state == null)
			{
				throw new Exception("Attempting to access invalid context.");
			}
			return _state;
		}
	}

	public Configuration Config => Plugin.Config.File;

	public GuiManager Gui => Plugin.Gui;

	public LocaleManager Locale => Gui.Locale;

	public IActionManager Actions => State.Actions;

	public IAnimationManager Animation => State.Animation;

	public ICharacterManager Characters => State.Characters;

	public ICameraManager Cameras => State.Cameras;

	public IEditorInterface Interface => State.Interface;

	public IPosingManager Posing => State.Posing;

	public ISceneManager Scene => State.Scene;

	public ISelectManager Selection => State.Selection;

	public ITransformHandler Transform => State.Transform;

	public EditorContext(GPoseService gpose, IPluginContext plugin)
	{
		_gpose = gpose;
		Plugin = plugin;
	}

	public void Setup(EditorState state)
	{
		if (_state != null)
		{
			throw new Exception("Attempted double initialization of editor context!");
		}
		_state = state;
	}

	public void Initialize()
	{
		_state?.Initialize();
	}

	public void Update()
	{
		_state?.Update();
	}

	public void Dispose()
	{
		try
		{
			_state?.Dispose();
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to destroy editor context:\n{value}");
		}
		finally
		{
			_state = null;
		}
		GC.SuppressFinalize(this);
	}
}
