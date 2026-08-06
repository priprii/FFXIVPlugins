using Ktisis.Scene.Entities;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Factory.Types;

public interface IEntityBuilder<out T, out TBuilder> : IEntityBuilderBase<T, TBuilder> where T : SceneEntity where TBuilder : IEntityBuilder<T, TBuilder>
{
	T Add();

	T Add(IComposite parent);
}
