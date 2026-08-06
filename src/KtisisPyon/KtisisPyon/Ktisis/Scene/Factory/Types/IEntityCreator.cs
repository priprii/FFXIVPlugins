using System.Threading.Tasks;
using Ktisis.Scene.Entities;

namespace Ktisis.Scene.Factory.Types;

public interface IEntityCreator<T, out TBuilder> : IEntityBuilderBase<T, TBuilder> where T : SceneEntity where TBuilder : IEntityBuilderBase<T, TBuilder>
{
	Task<T> Spawn();
}
