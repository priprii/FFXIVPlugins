using System;
using System.Numerics;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using KamiToolKit;
using KamiToolKit.Overlay.UiOverlay;
using Ktisis.Core.Attributes;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.KTK;
using Ktisis.Scene.Entities.Game;

namespace Ktisis.Services.Game;

[Singleton]
public class OverlayService : IDisposable
{
	private readonly IDalamudPluginInterface _dpi;

	private readonly IFramework _framework;

	private readonly IObjectTable _objectTable;

	private bool _init;

	private bool _showedHint;

	private OverlayController? _controller;

	private PreviewNode? _preview;

	public OverlayService(IDalamudPluginInterface dpi, IFramework framework, IObjectTable objectTable)
	{
		_dpi = dpi;
		_framework = framework;
		_objectTable = objectTable;
	}

	public void Initialize(IEditorContext context)
	{
		if (!_init)
		{
			KamiToolKitLibrary.Initialize(_dpi);
			_controller = new OverlayController();
			_init = true;
			context.Plugin.Gui.FileDialogs.OnSelectionChanged += HandleFileDialogEvent;
			if (context.Config.Editor.ShowHints && !_showedHint)
			{
				ShowHint(context);
			}
		}
	}

	public bool AddNode(OverlayNode node)
	{
		_controller?.AddNode(node);
		return true;
	}

	public void ShowHint(IEditorContext context)
	{
		int iconId = new Random().Next(73001, 73291);
		int num = context.Locale.RandomHintKey();
		string hint = context.Locale.Translate($"hints.{num}");
		OverlayController? controller = _controller;
		if (controller != null)
		{
			HintNode hintNode = new HintNode((uint)iconId, hint, num, 300);
			hintNode.Position = new Vector2(87f, 138f);
			hintNode.Size = new Vector2(640f, 80f);
			hintNode.Scale = new Vector2(1f, 1f);
			hintNode.CollisionNode.Position = new Vector2(-99f, -155f);
			hintNode.CollisionNode.Size = new Vector2(749f, 256f);
			controller.AddNode(hintNode);
		}
		_showedHint = true;
	}

	public void ToggleCharaViewTexture(IEditorContext context, ActorEntity actor)
	{
		DisablePreview();
		_preview = new PreviewNode(context, _framework, _objectTable, actor)
		{
			Position = new Vector2(500f, 500f)
		};
		_controller?.AddNode(_preview);
	}

	private void HandleFileDialogEvent(object? sender, string path)
	{
		string text = path.Substring(path.LastIndexOf('.') + 1).ToLower();
		if ((text == "pose" || text == "cmp") ? true : false)
		{
			_preview?.PoseActor(path);
		}
	}

	public bool RemoveNode(OverlayNode node)
	{
		_controller?.RemoveNode(node);
		return true;
	}

	public void Disable()
	{
		if (_init)
		{
			DisablePreview();
			_controller?.Dispose();
			KamiToolKitLibrary.Dispose();
			_init = false;
		}
	}

	private void DisablePreview()
	{
		if (_preview != null)
		{
			_controller?.RemoveNode(_preview);
			_preview.Cleanup();
			_preview = null;
		}
	}

	public void Dispose()
	{
		Disable();
	}
}
