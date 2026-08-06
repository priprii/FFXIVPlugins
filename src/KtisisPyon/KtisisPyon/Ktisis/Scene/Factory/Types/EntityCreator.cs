using Ktisis.Scene.Entities;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Factory.Types;

public abstract class EntityCreator<T, TBuilder> : EntityBuilderBase<T, TBuilder> where T : SceneEntity where TBuilder : IEntityCreator<T, TBuilder>
{
	protected EntityCreator(ISceneManager scene)
		: base(scene)
	{
	}
}
