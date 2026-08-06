using System;
using Dalamud.Utility;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.World;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Factory.Types;

public abstract class EntityBuilder<T, TBuilder> : EntityBuilderBase<T, TBuilder> where T : SceneEntity where TBuilder : IEntityBuilder<T, TBuilder>
{
	protected EntityBuilder(ISceneManager scene)
		: base(scene)
	{
	}

	protected abstract T Build();

	public T Add()
	{
		return Add(Scene);
	}

	public virtual T Add(IComposite parent)
	{
		if (!Scene.IsValid)
		{
			throw new Exception("Attempted to build entity for invalid scene.");
		}
		T result = GetResult();
		parent.Add(result);
		if (result is WorldEntity worldEntity)
		{
			worldEntity.Setup();
		}
		return result;
	}

	private T GetResult()
	{
		T val = Build();
		if (StringExtensions.IsNullOrEmpty(val.Name))
		{
			val.Name = val.GetType().Name;
		}
		return val;
	}
}
