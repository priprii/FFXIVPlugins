using System.Threading.Tasks;
using Ktisis.Scene.Entities.World;
using Ktisis.Scene.Factory.Types;
using Ktisis.Scene.Modules.Lights;
using Ktisis.Scene.Types;
using Ktisis.Structs.Lights;

namespace Ktisis.Scene.Factory.Creators;

public sealed class LightCreator : EntityCreator<LightEntity, ILightCreator>, ILightCreator, IEntityCreator<LightEntity, ILightCreator>, IEntityBuilderBase<LightEntity, ILightCreator>
{
	private LightType Type = LightType.SpotLight;

	protected override ILightCreator Builder => this;

	public LightCreator(ISceneManager scene)
		: base(scene)
	{
		base.Name = "Light";
	}

	public ILightCreator SetType(LightType type)
	{
		Type = type;
		return this;
	}

	public async Task<LightEntity> Spawn()
	{
		LightEntity obj = await Scene.GetModule<LightModule>().Spawn();
		obj.Name = base.Name;
		obj.SetType(Type);
		return obj;
	}
}
