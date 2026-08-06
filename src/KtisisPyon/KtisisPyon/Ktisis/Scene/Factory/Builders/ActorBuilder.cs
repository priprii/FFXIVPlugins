using Dalamud.Game.ClientState.Objects.Types;
using Ktisis.Common.Extensions;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Factory.Types;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Factory.Builders;

public sealed class ActorBuilder : EntityBuilder<ActorEntity, IActorBuilder>, IActorBuilder, IEntityBuilder<ActorEntity, IActorBuilder>, IEntityBuilderBase<ActorEntity, IActorBuilder>
{
	private readonly IPoseBuilder _pose;

	private readonly IGameObject _gameObject;

	protected override ActorBuilder Builder => this;

	public ActorBuilder(ISceneManager scene, IPoseBuilder pose, IGameObject gameObject)
		: base(scene)
	{
		base.Name = gameObject.GetNameOrFallback(scene.Context, false);
		_pose = pose;
		_gameObject = gameObject;
	}

	protected override ActorEntity Build()
	{
		return new ActorEntity(Scene, _pose, _gameObject)
		{
			Name = base.Name
		};
	}
}
