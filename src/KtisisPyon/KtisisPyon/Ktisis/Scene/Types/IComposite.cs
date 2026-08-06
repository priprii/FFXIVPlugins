using System.Collections.Generic;
using Ktisis.Scene.Entities;

namespace Ktisis.Scene.Types;

public interface IComposite
{
	SceneEntity? Parent { get; set; }

	IEnumerable<SceneEntity> Children { get; }

	bool Add(SceneEntity entity);

	bool Remove(SceneEntity entity);

	IEnumerable<SceneEntity> Recurse();
}
