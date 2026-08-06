using System.Collections.Generic;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Entities;

namespace Ktisis.Editor.Transforms.Types;

public interface ITransformTarget : ITransform
{
	SceneEntity? Primary { get; }

	IEnumerable<SceneEntity> Targets { get; }

	TransformSetup Setup { get; set; }
}
