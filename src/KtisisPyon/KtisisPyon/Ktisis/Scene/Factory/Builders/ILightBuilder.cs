using Ktisis.Scene.Entities.World;
using Ktisis.Scene.Factory.Types;
using Ktisis.Structs.Lights;
using Ktisis.Structs.Objects;

namespace Ktisis.Scene.Factory.Builders;

public interface ILightBuilder : IEntityBuilder<LightEntity, ILightBuilder>, IEntityBuilderBase<LightEntity, ILightBuilder>
{
	ILightBuilder SetAddress(nint address);

	unsafe ILightBuilder SetAddress(SceneLight* pointer);

	ILightBuilder SetWorldLight(WorldObject light);
}
