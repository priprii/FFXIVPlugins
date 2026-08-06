using System;
using Ktisis.Scene.Entities.World;
using Ktisis.Scene.Factory.Types;
using Ktisis.Scene.Types;
using Ktisis.Structs.Lights;
using Ktisis.Structs.Objects;

namespace Ktisis.Scene.Factory.Builders;

public sealed class LightBuilder : EntityBuilder<LightEntity, ILightBuilder>, ILightBuilder, IEntityBuilder<LightEntity, ILightBuilder>, IEntityBuilderBase<LightEntity, ILightBuilder>
{
	private nint Address = IntPtr.Zero;

	private WorldObject? WorldLight;

	protected override LightBuilder Builder => this;

	public LightBuilder(ISceneManager scene)
		: base(scene)
	{
		base.Name = "Light";
	}

	public ILightBuilder SetAddress(nint address)
	{
		Address = address;
		return this;
	}

	public unsafe ILightBuilder SetAddress(SceneLight* pointer)
	{
		Address = (nint)pointer;
		return this;
	}

	public ILightBuilder SetWorldLight(WorldObject light)
	{
		WorldLight = light;
		return this;
	}

	protected override LightEntity Build()
	{
		if (Address == IntPtr.Zero)
		{
			throw new Exception("Attempted to create light from null pointer.");
		}
		return new LightEntity(Scene)
		{
			Name = base.Name,
			Address = Address,
			WorldLight = WorldLight
		};
	}
}
