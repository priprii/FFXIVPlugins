using System.Numerics;
using Ktisis.Common.Utility;

namespace Ktisis.Scene.Decor;

public interface ITransform
{
	Transform? GetTransform();

	void SetTransform(Transform trans);

	Matrix4x4? GetMatrix()
	{
		return GetTransform()?.ComposeMatrix();
	}

	void SetMatrix(Matrix4x4 mx)
	{
		Transform transform = GetTransform();
		if (transform != null)
		{
			SetTransform(new Transform(mx, transform));
		}
		else
		{
			SetTransform(new Transform(mx));
		}
	}
}
