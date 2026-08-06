using System;

namespace Ktisis.Editor.Transforms.Types;

public interface ITransformHandler
{
	ITransformTarget? Target { get; }

	ITransformMemento Begin(ITransformTarget target);

	ITransformMemento Begin(ITransformTarget target, Action<TransformSetup> configure);
}
