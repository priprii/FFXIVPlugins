using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;
using Ktisis.Common.Utility;
using Ktisis.Editor.Posing;
using Ktisis.Editor.Transforms.Types;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Skeleton;

namespace Ktisis.Editor.Transforms;

public class TransformTarget : ITransformTarget, ITransform
{
	private readonly Dictionary<EntityPose, Dictionary<int, List<BoneNode>>> PoseMap;

	public SceneEntity? Primary { get; }

	public IEnumerable<SceneEntity> Targets { get; }

	public TransformSetup Setup { get; set; } = new TransformSetup();

	public TransformTarget(SceneEntity? primary, IEnumerable<SceneEntity> targets)
	{
		targets = targets.ToList();
		Primary = primary;
		Targets = targets;
		PoseMap = TransformResolver.BuildPoseMap(primary, targets);
	}

	public Transform? GetTransform()
	{
		if (Primary is ITransform transform)
		{
			return transform.GetTransform();
		}
		return null;
	}

	public void SetTransform(Transform transform)
	{
		Transform transform2 = GetTransform();
		if (transform2 != null)
		{
			TransformObjects(transform, transform2);
			TransformSkeletons(transform, transform2);
		}
	}

	private void TransformObjects(Transform transform, Transform initial)
	{
		if (!Matrix4x4.Invert(initial.ComposeMatrix(), out var result))
		{
			return;
		}
		Matrix4x4 result2 = result * transform.ComposeMatrix();
		switch (Setup.MirrorRotation)
		{
		case MirrorMode.Inverse:
			Matrix4x4.Invert(result2, out result2);
			break;
		case MirrorMode.Reflect:
			Matrix4x4.Invert(result2, out result2);
			break;
		}
		foreach (SceneEntity item in Targets.Where((SceneEntity tar) => tar != null && tar.IsValid && !(tar is BoneNode)))
		{
			if (!(item is ITransform transform2))
			{
				continue;
			}
			Transform transform3 = transform2.GetTransform();
			if (transform3 != null)
			{
				if (item == Primary)
				{
					transform2.SetTransform(transform);
					continue;
				}
				transform3.DecomposeMatrixPrecise(transform3.ComposeMatrix() * result2, transform3);
				transform2.SetTransform(transform3);
			}
		}
	}

	private unsafe void TransformSkeletons(Transform transform, Transform initial)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		Transform delta = new Transform(transform.Position - initial.Position, Quaternion.Normalize(transform.Rotation / initial.Rotation), transform.Scale / initial.Scale);
		foreach (KeyValuePair<EntityPose, Dictionary<int, List<BoneNode>>> item in PoseMap)
		{
			item.Deconstruct(out var key, out var value);
			EntityPose entityPose = key;
			Dictionary<int, List<BoneNode>> dictionary = value;
			Skeleton* skeleton = entityPose.GetSkeleton();
			if (skeleton == null || ((Skeleton)skeleton).PartialSkeletons == null)
			{
				continue;
			}
			ushort partialSkeletonCount = ((Skeleton)skeleton).PartialSkeletonCount;
			for (int i = 0; i < partialSkeletonCount; i++)
			{
				if (!dictionary.TryGetValue(i, out var value2))
				{
					continue;
				}
				PartialSkeleton val = ((PartialSkeleton*)((Skeleton)skeleton).PartialSkeletons)[i];
				hkaPose* havokPose = ((PartialSkeleton)(ref val)).GetHavokPose(0);
				if (havokPose == null)
				{
					continue;
				}
				foreach (BoneNode item2 in value2.Where((BoneNode bone) => bone.IsValid))
				{
					TransformBone(transform, initial, delta, skeleton, havokPose, item2);
				}
			}
		}
	}

	private unsafe void TransformBone(Transform transform, Transform initial, Transform delta, Skeleton* skeleton, hkaPose* hkaPose, BoneNode bone)
	{
		int boneIndex = bone.Info.BoneIndex;
		Transform transform2 = bone.GetTransform();
		if (transform2 == null)
		{
			return;
		}
		MirrorMode mirrorRotation = Setup.MirrorRotation;
		bool flag = (uint)(mirrorRotation - 1) <= 1u;
		bool flag2 = flag;
		if (flag2 && Primary is BoneNode node)
		{
			flag2 &= !bone.IsBoneDescendantOf(node);
		}
		Matrix4x4 mx;
		if (bone == Primary)
		{
			mx = transform.ComposeMatrix();
		}
		else
		{
			Vector3 scales = transform2.Scale * delta.Scale;
			Vector3 vector2;
			Quaternion value;
			if (flag2)
			{
				Vector3 vector = Vector3.Transform(delta.Position, Quaternion.Inverse(initial.Rotation));
				if (Setup.MirrorRotation == MirrorMode.Inverse)
				{
					value = Quaternion.Conjugate(delta.Rotation);
					vector = new Vector3(0f - vector.X, 0f - vector.Y, 0f - vector.Z);
				}
				else
				{
					Quaternion value2 = Quaternion.Inverse(initial.Rotation) * delta.Rotation * initial.Rotation;
					Matrix4x4 matrix4x = new Matrix4x4(-1f, 0f, 0f, 0f, 0f, -1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
					Matrix4x4 matrix4x2 = Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(value2));
					value2 = Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(matrix4x * matrix4x2 * matrix4x));
					value = initial.Rotation * value2 * Quaternion.Inverse(initial.Rotation);
					vector = new Vector3(vector.X, vector.Y, 0f - vector.Z);
				}
				vector2 = Vector3.Transform(vector, transform2.Rotation);
			}
			else
			{
				value = delta.Rotation;
				vector2 = delta.Position;
			}
			value = Quaternion.Normalize(value);
			Quaternion value3 = ((!Setup.RelativeBones) ? (value * transform2.Rotation) : (Quaternion.Normalize(transform2.Rotation / initial.Rotation) * value * initial.Rotation));
			value3 = Quaternion.Normalize(value3);
			Matrix4x4 matrix4x3 = Matrix4x4.CreateScale(scales);
			Matrix4x4 matrix4x4 = Matrix4x4.CreateFromQuaternion(value3);
			Matrix4x4 matrix4x5 = Matrix4x4.CreateTranslation(transform2.Position + vector2);
			mx = matrix4x3 * matrix4x4 * matrix4x5;
		}
		Transform modelTransform = HavokPosing.GetModelTransform(hkaPose, boneIndex);
		bone.SetTransform(new Transform(mx, modelTransform));
		if (Setup.ParentBones)
		{
			Transform modelTransform2 = HavokPosing.GetModelTransform(hkaPose, boneIndex);
			HavokPosing.Propagate(skeleton, bone.Info.PartialIndex, bone.Info.BoneIndex, modelTransform2, modelTransform);
		}
	}
}
