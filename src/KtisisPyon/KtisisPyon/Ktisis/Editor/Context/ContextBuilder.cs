using Dalamud.Plugin.Services;
using Ktisis.Core.Attributes;
using Ktisis.Core.Types;
using Ktisis.Data.Mcdf;
using Ktisis.Editor.Actions;
using Ktisis.Editor.Actions.Input;
using Ktisis.Editor.Animation;
using Ktisis.Editor.Camera;
using Ktisis.Editor.Characters;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Posing;
using Ktisis.Editor.Posing.Attachment;
using Ktisis.Editor.Posing.AutoSave;
using Ktisis.Editor.Selection;
using Ktisis.Editor.Transforms;
using Ktisis.Interface.Editor;
using Ktisis.Interop;
using Ktisis.Interop.Hooking;
using Ktisis.Scene;
using Ktisis.Scene.Factory;
using Ktisis.Services.Data;
using Ktisis.Services.Game;

namespace Ktisis.Editor.Context;

[Singleton]
public class ContextBuilder
{
	private readonly OverlayService _overlay;

	private readonly WorldService _world;

	private readonly GPoseService _gpose;

	private readonly InteropService _interop;

	private readonly IFramework _framework;

	private readonly IDataManager _data;

	private readonly IKeyState _keyState;

	private readonly NamingService _naming;

	private readonly FormatService _format;

	private readonly McdfManager _mcdf;

	private readonly IObjectTable _objectTable;

	public SceneDataService _sceneData;

	public ContextBuilder(OverlayService overlay, WorldService world, GPoseService gpose, InteropService interop, IFramework framework, IDataManager data, IKeyState keyState, NamingService naming, FormatService format, McdfManager mcdf, IObjectTable objectTable)
	{
		_overlay = overlay;
		_world = world;
		_gpose = gpose;
		_interop = interop;
		_framework = framework;
		_data = data;
		_keyState = keyState;
		_naming = naming;
		_format = format;
		_mcdf = mcdf;
		_objectTable = objectTable;
	}

	public IEditorContext Create(IPluginContext state)
	{
		EditorContext editorContext = new EditorContext(_gpose, state);
		HookScope scope = _interop.CreateScope();
		InputManager input = new InputManager(editorContext, scope, _keyState);
		ActionManager actionManager = new ActionManager(editorContext, input);
		EntityFactory factory = new EntityFactory(editorContext, _naming, _mcdf);
		SelectManager selectManager = new SelectManager(editorContext, _gpose);
		AttachManager attach = new AttachManager();
		_sceneData = new SceneDataService(editorContext, _objectTable, _framework);
		PoseAutoSave autoSave = new PoseAutoSave(editorContext, _framework, _format, _sceneData);
		EditorState state2 = new EditorState(editorContext, scope)
		{
			Actions = actionManager,
			Animation = new AnimationManager(editorContext, scope, _data, _framework),
			Cameras = new CameraManager(editorContext, scope),
			Characters = new CharacterManager(editorContext, _objectTable, scope, _framework, _mcdf),
			Interface = new EditorInterface(editorContext, state.Gui),
			Posing = new PosingManager(editorContext, scope, _framework, attach, autoSave),
			Scene = new SceneManager(editorContext, scope, _framework, factory, _objectTable, _sceneData, _overlay, _world),
			Selection = selectManager,
			Transform = new TransformHandler(editorContext, actionManager, selectManager)
		};
		editorContext.Setup(state2);
		return editorContext;
	}
}
