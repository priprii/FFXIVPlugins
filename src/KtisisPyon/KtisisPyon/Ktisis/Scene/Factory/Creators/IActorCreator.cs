using Ktisis.Data.Files;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Factory.Types;

namespace Ktisis.Scene.Factory.Creators;

public interface IActorCreator : IEntityCreator<ActorEntity, IActorCreator>, IEntityBuilderBase<ActorEntity, IActorCreator>
{
	IActorCreator WithAppearance(CharaFile file);

	IActorCreator WithMcdf(string McdfPath);
}
