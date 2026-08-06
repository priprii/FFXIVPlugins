using System.Numerics;
using Ktisis.Actions.Types;
using Ktisis.Common.Utility;

namespace Ktisis.Editor.Transforms.Types;

public interface ITransformMemento : IMemento
{
	ITransformMemento Save();

	void SetTransform(Transform transform);

	void SetMatrix(Matrix4x4 matrix);

	void Dispatch();
}
