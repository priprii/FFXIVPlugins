using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Factory.Types;

namespace Ktisis.Scene.Factory.Builders;

public interface IActorBuilder : IEntityBuilder<ActorEntity, IActorBuilder>, IEntityBuilderBase<ActorEntity, IActorBuilder>
{
}
