using Dalamud.Game.ClientState.Objects.Types;
using Ktisis.Data.Mcdf;
using Ktisis.Editor.Context.Types;
using Ktisis.Scene.Factory.Builders;
using Ktisis.Scene.Factory.Creators;
using Ktisis.Scene.Factory.Types;
using Ktisis.Scene.Types;
using Ktisis.Services.Data;
using Ktisis.Structs.Lights;

namespace Ktisis.Scene.Factory;

public class EntityFactory : IEntityFactory
{
	private readonly IEditorContext _ctx;

	private readonly INameResolver _naming;

	private readonly McdfManager _mcdfManager;

	private ISceneManager Scene => _ctx.Scene;

	public EntityFactory(IEditorContext ctx, INameResolver naming, McdfManager mcdfManager)
	{
		_ctx = ctx;
		_naming = naming;
		_mcdfManager = mcdfManager;
	}

	public IActorBuilder BuildActor(IGameObject actor)
	{
		return new ActorBuilder(Scene, BuildPose(), actor);
	}

	public ILightBuilder BuildLight()
	{
		return new LightBuilder(Scene);
	}

	public IObjectBuilder BuildObject()
	{
		return new ObjectBuilder(Scene, BuildPose(), _naming);
	}

	public IPoseBuilder BuildPose()
	{
		return new PoseBuilder(Scene);
	}

	public IRefImageBuilder BuildRefImage()
	{
		return new RefImageBuilder(Scene);
	}

	public IOverlayBuilder BuildOverlay(OverlayTypes type)
	{
		return new OverlayBuilder(Scene, type);
	}

	public IActorCreator CreateActor()
	{
		return new ActorCreator(Scene, _mcdfManager);
	}

	public ILightCreator CreateLight()
	{
		return new LightCreator(Scene);
	}

	public ILightCreator CreateLight(LightType type)
	{
		return CreateLight().SetType(type);
	}
}
