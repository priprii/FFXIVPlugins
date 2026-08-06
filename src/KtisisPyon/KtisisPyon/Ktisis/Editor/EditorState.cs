using System;
using Ktisis.Editor.Actions;
using Ktisis.Editor.Animation.Types;
using Ktisis.Editor.Camera;
using Ktisis.Editor.Characters.Types;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Posing.Types;
using Ktisis.Editor.Selection;
using Ktisis.Editor.Transforms.Types;
using Ktisis.Interface.Editor.Types;
using Ktisis.Interop.Hooking;
using Ktisis.Scene.Types;

namespace Ktisis.Editor;

public class EditorState : IDisposable
{
	private readonly IEditorContext _context;

	private readonly HookScope _scope;

	private bool IsInit;

	private bool IsDisposing;

	public bool IsValid
	{
		get
		{
			if (IsInit && _context.IsGPosing)
			{
				return !IsDisposing;
			}
			return false;
		}
	}

	public required IActionManager Actions { get; init; }

	public required IAnimationManager Animation { get; init; }

	public required ICameraManager Cameras { get; init; }

	public required ICharacterManager Characters { get; init; }

	public required IEditorInterface Interface { get; init; }

	public required IPosingManager Posing { get; init; }

	public required ISceneManager Scene { get; init; }

	public required ISelectManager Selection { get; init; }

	public required ITransformHandler Transform { get; init; }

	public EditorState(IEditorContext context, HookScope scope)
	{
		_context = context;
		_scope = scope;
	}

	public void Initialize()
	{
		try
		{
			IsInit = true;
			Actions.Initialize();
			Animation.Initialize();
			Characters.Initialize();
			Cameras.Initialize();
			Posing.Initialize();
			Scene.Initialize();
		}
		catch
		{
			Dispose();
			throw;
		}
		try
		{
			Interface.Prepare();
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Error preparing interface:\n{value}");
		}
	}

	public void Update()
	{
		Scene.Update();
		Selection.Update();
	}

	public void Dispose()
	{
		IsDisposing = true;
		_scope.Dispose();
		Scene.Dispose();
		Posing.Dispose();
		Cameras.Dispose();
		GC.SuppressFinalize(this);
	}
}
