using Ktisis.Scene.Entities.World;
using Ktisis.Scene.Factory.Types;
using Ktisis.Structs.Lights;

namespace Ktisis.Scene.Factory.Creators;

public interface ILightCreator : IEntityCreator<LightEntity, ILightCreator>, IEntityBuilderBase<LightEntity, ILightCreator>
{
	ILightCreator SetType(LightType type);
}
