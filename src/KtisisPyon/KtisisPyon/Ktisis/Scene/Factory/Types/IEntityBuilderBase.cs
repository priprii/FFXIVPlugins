using Ktisis.Scene.Entities;

namespace Ktisis.Scene.Factory.Types;

public interface IEntityBuilderBase<out T, out TBuilder> where T : SceneEntity where TBuilder : IEntityBuilderBase<T, TBuilder>
{
	TBuilder SetName(string name);
}
