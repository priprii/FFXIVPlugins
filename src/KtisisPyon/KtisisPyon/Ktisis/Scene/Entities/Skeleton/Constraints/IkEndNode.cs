using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using Ktisis.Common.Utility;
using Ktisis.Editor.Posing.Types;
using Ktisis.Scene.Decor.Ik;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Entities.Skeleton.Constraints;

public abstract class IkEndNode : BoneNode, IIkNode
{
	private new IkNodeGroupBase? Parent => base.Parent as IkNodeGroupBase;

	public virtual bool IsEnabled => Parent?.IsEnabled ?? false;

	protected abstract bool IsOverride { get; }

	protected IkEndNode(ISceneManager scene, EntityPose pose, PartialBoneInfo bone, uint partialId)
		: base(scene, pose, bone, partialId)
	{
	}

	public unsafe virtual void Enable()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Skeleton* skeleton = GetSkeleton();
		if (skeleton != null)
		{
			Transform offset = new Transform(((Skeleton)skeleton).Transform);
			Transform transform = CalcTransformWorld();
			if (transform != null)
			{
				SetTransformTarget(transform, offset, transform);
			}
			Parent?.Enable();
		}
	}

	public virtual void Disable()
	{
		Parent?.Disable();
	}

	public virtual void Toggle()
	{
		if (IsEnabled)
		{
			Disable();
		}
		else
		{
			Enable();
		}
	}

	public abstract Transform GetTransformTarget(Transform offset, Transform world);

	public abstract void SetTransformTarget(Transform target, Transform offset, Transform world);

	public unsafe override Transform? GetTransform()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Skeleton* skeleton = GetSkeleton();
		if (skeleton == null)
		{
			return null;
		}
		Transform offset = new Transform(((Skeleton)skeleton).Transform);
		Transform transform = CalcTransformWorld();
		if (!IsOverride || transform == null)
		{
			return transform;
		}
		return GetTransformTarget(offset, transform);
	}

	public unsafe override void SetTransform(Transform transform)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Skeleton* skeleton = GetSkeleton();
		if (skeleton != null)
		{
			Transform offset = new Transform(((Skeleton)skeleton).Transform);
			Transform transform2 = CalcTransformWorld();
			if (IsOverride && transform2 != null)
			{
				SetTransformTarget(transform, offset, transform2);
			}
			else
			{
				SetTransformWorld(transform);
			}
		}
	}

	public override Matrix4x4? GetMatrix()
	{
		if (!IsOverride)
		{
			return CalcMatrixWorld();
		}
		return GetTransform()?.ComposeMatrix();
	}

	public override void SetMatrix(Matrix4x4 matrix)
	{
		if (IsOverride)
		{
			Transform transform = GetTransform();
			if (transform != null)
			{
				SetTransform(new Transform(matrix, transform));
			}
			else
			{
				SetTransform(new Transform(matrix));
			}
		}
		else
		{
			SetMatrixWorld(matrix);
		}
	}
}
