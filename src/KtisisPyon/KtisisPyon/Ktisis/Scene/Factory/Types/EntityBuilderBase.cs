using Ktisis.Scene.Entities;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Factory.Types;

public abstract class EntityBuilderBase<T, TBuilder> : IEntityBuilderBase<T, TBuilder> where T : SceneEntity where TBuilder : IEntityBuilderBase<T, TBuilder>
{
	protected readonly ISceneManager Scene;

	protected string Name { get; set; } = string.Empty;

	protected abstract TBuilder Builder { get; }

	protected EntityBuilderBase(ISceneManager scene)
	{
		Scene = scene;
	}

	public virtual TBuilder SetName(string name)
	{
		Name = name;
		return Builder;
	}
}
