using Ktisis.Scene.Factory.Builders;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Entities.Character;

public class WeaponEntity : CharaEntity
{
	public WeaponEntity(ISceneManager scene, IPoseBuilder pose)
		: base(scene, pose)
	{
		base.Type = EntityType.Weapon;
	}
}
