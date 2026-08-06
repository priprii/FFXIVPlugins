using System;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Entities;

public class SceneRoot : SceneEntity
{
	public override bool IsValid => Scene.IsValid;

	public override SceneEntity? Parent
	{
		get
		{
			return null;
		}
		set
		{
			throw new Exception("Attempted to set parent of scene root.");
		}
	}

	public SceneRoot(ISceneManager scene)
		: base(scene)
	{
	}

	public override bool Add(SceneEntity entity)
	{
		if (entity is ActorEntity actorEntity)
		{
			Ktisis.Log.Debug($"Adding actor to scene: '{actorEntity.Name}' (index: {actorEntity.Actor.ObjectIndex})");
		}
		else
		{
			Ktisis.Log.Debug($"Adding entity to scene: '{entity.Name}' ({entity.GetType().Name})");
		}
		return base.Add(entity);
	}
}
