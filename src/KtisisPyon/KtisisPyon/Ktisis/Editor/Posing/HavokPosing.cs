using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Container.Array;
using FFXIVClientStructs.Havok.Common.Base.Container.String;
using FFXIVClientStructs.Havok.Common.Base.Math.Matrix;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using FFXIVClientStructs.Havok.Common.Base.Math.Quaternion;
using FFXIVClientStructs.Havok.Common.Base.Math.Vector;
using Ktisis.Common.Utility;
using Ktisis.Interop;

namespace Ktisis.Editor.Posing;

public static class HavokPosing
{
	private static readonly Alloc<Matrix4x4> Matrix = new Alloc<Matrix4x4>(16uL);

	private static readonly ConcurrentDictionary<nint, Transform?> _abdomenTransformCache = new ConcurrentDictionary<nint, Transform>();

	public unsafe static Matrix4x4 GetMatrix(hkQsTransformf* transform)
	{
		((hkQsTransformf)transform).get4x4ColumnMajor((float*)Matrix.Address);
		return *Matrix.Data;
	}

	public unsafe static Matrix4x4 GetMatrix(hkaPose* pose, int boneIndex)
	{
		if (pose == null || ((hkaPose)pose).ModelPose.Data == null)
		{
			return Matrix4x4.Identity;
		}
		return GetMatrix((hkQsTransformf*)((byte*)((hkaPose)pose).ModelPose.Data + (nint)boneIndex * (nint)Unsafe.SizeOf<hkQsTransformf>()));
	}

	public unsafe static void SetMatrix(hkQsTransformf* trans, Matrix4x4 matrix)
	{
		*Matrix.Data = matrix;
		((hkQsTransformf)trans).set((hkMatrix4f*)Matrix.Address);
	}

	public unsafe static void SetMatrix(hkaPose* pose, int boneIndex, Matrix4x4 matrix)
	{
		SetMatrix((hkQsTransformf*)((byte*)((hkaPose)pose).ModelPose.Data + (nint)boneIndex * (nint)Unsafe.SizeOf<hkQsTransformf>()), matrix);
	}

	public unsafe static void CalcCachedAbdomenModelTransform(hkaPose* pose, int boneIndex)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		Transform orAdd = _abdomenTransformCache.GetOrAdd((nint)pose, (nint _) => GetModelTransform(pose, boneIndex));
		if (orAdd != null)
		{
			byte* num = (byte*)((hkaPose)pose).ModelPose.Data + (nint)boneIndex * (nint)Unsafe.SizeOf<hkQsTransformf>();
			Unsafe.Write(&((hkQsTransformf)num).Translation, new hkVector4f
			{
				X = orAdd.Position.X,
				Y = orAdd.Position.Y,
				Z = orAdd.Position.Z,
				W = 0f
			});
			Unsafe.Write(&((hkQsTransformf)num).Rotation, new hkQuaternionf
			{
				X = orAdd.Rotation.X,
				Y = orAdd.Rotation.Y,
				Z = orAdd.Rotation.Z,
				W = orAdd.Rotation.W
			});
			Unsafe.Write(&((hkQsTransformf)num).Scale, new hkVector4f
			{
				X = orAdd.Scale.X,
				Y = orAdd.Scale.Y,
				Z = orAdd.Scale.Z,
				W = 0f
			});
		}
	}

	private unsafe static void SetCachedAbdomenModelTransform(hkaPose* pose, Transform transform)
	{
		_abdomenTransformCache[(nint)pose] = transform;
	}

	public static void ClearCachedAbdomenModelTransform()
	{
		_abdomenTransformCache.Clear();
	}

	public unsafe static Transform? GetModelTransform(hkaPose* pose, int boneIx)
	{
		if (pose == null || ((hkaPose)pose).ModelPose.Data == null || boneIx < 0 || boneIx >= ((hkaPose)pose).ModelPose.Length)
		{
			return null;
		}
		hkQsTransformf* ptr = (hkQsTransformf*)((byte*)((hkaPose)pose).ModelPose.Data + (nint)boneIx * (nint)Unsafe.SizeOf<hkQsTransformf>());
		Vector3 pos = new Vector3(((hkVector4f)(&((hkQsTransformf)ptr).Translation)).X, ((hkVector4f)(&((hkQsTransformf)ptr).Translation)).Y, ((hkVector4f)(&((hkQsTransformf)ptr).Translation)).Z);
		Quaternion rot = new Quaternion(((hkQuaternionf)(&((hkQsTransformf)ptr).Rotation)).X, ((hkQuaternionf)(&((hkQsTransformf)ptr).Rotation)).Y, ((hkQuaternionf)(&((hkQsTransformf)ptr).Rotation)).Z, ((hkQuaternionf)(&((hkQsTransformf)ptr).Rotation)).W);
		Vector3 scale = new Vector3(((hkVector4f)(&((hkQsTransformf)ptr).Scale)).X, ((hkVector4f)(&((hkQsTransformf)ptr).Scale)).Y, ((hkVector4f)(&((hkQsTransformf)ptr).Scale)).Z);
		return new Transform(pos, rot, scale);
	}

	public unsafe static void SetModelTransform(hkaPose* pose, int boneIx, Transform trans)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		if (pose != null && ((hkaPose)pose).ModelPose.Data != null && boneIx >= 0 && boneIx < ((hkaPose)pose).ModelPose.Length)
		{
			byte* num = (byte*)((hkaPose)pose).ModelPose.Data + (nint)boneIx * (nint)Unsafe.SizeOf<hkQsTransformf>();
			Unsafe.Write(&((hkQsTransformf)num).Translation, new hkVector4f
			{
				X = trans.Position.X,
				Y = trans.Position.Y,
				Z = trans.Position.Z,
				W = 0f
			});
			Unsafe.Write(&((hkQsTransformf)num).Rotation, new hkQuaternionf
			{
				X = trans.Rotation.X,
				Y = trans.Rotation.Y,
				Z = trans.Rotation.Z,
				W = trans.Rotation.W
			});
			Unsafe.Write(&((hkQsTransformf)num).Scale, new hkVector4f
			{
				X = trans.Scale.X,
				Y = trans.Scale.Y,
				Z = trans.Scale.Z,
				W = 0f
			});
			hkaBone val = ((hkaSkeleton)((hkaPose)pose).Skeleton).Bones[boneIx];
			if (((hkStringPtr)(ref val.Name)).String == "n_hara")
			{
				SetCachedAbdomenModelTransform(pose, trans);
			}
		}
	}

	public unsafe static Transform? GetLocalTransform(hkaPose* pose, int boneIx)
	{
		if (pose == null || ((hkaPose)pose).LocalPose.Data == null || boneIx < 0 || boneIx >= ((hkaPose)pose).LocalPose.Length)
		{
			return null;
		}
		hkQsTransformf* ptr = (hkQsTransformf*)((byte*)((hkaPose)pose).LocalPose.Data + (nint)boneIx * (nint)Unsafe.SizeOf<hkQsTransformf>());
		Vector3 pos = new Vector3(((hkVector4f)(&((hkQsTransformf)ptr).Translation)).X, ((hkVector4f)(&((hkQsTransformf)ptr).Translation)).Y, ((hkVector4f)(&((hkQsTransformf)ptr).Translation)).Z);
		Quaternion rot = new Quaternion(((hkQuaternionf)(&((hkQsTransformf)ptr).Rotation)).X, ((hkQuaternionf)(&((hkQsTransformf)ptr).Rotation)).Y, ((hkQuaternionf)(&((hkQsTransformf)ptr).Rotation)).Z, ((hkQuaternionf)(&((hkQsTransformf)ptr).Rotation)).W);
		Vector3 scale = new Vector3(((hkVector4f)(&((hkQsTransformf)ptr).Scale)).X, ((hkVector4f)(&((hkQsTransformf)ptr).Scale)).Y, ((hkVector4f)(&((hkQsTransformf)ptr).Scale)).Z);
		return new Transform(pos, rot, scale);
	}

	public unsafe static void Propagate(Skeleton* skele, int partialIx, int boneIx, Transform target, Transform initial, bool propagatePartials = true)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		PartialSkeleton val = ((PartialSkeleton*)((Skeleton)skele).PartialSkeletons)[partialIx];
		hkaPose* havokPose = ((PartialSkeleton)(ref val)).GetHavokPose(0);
		if (havokPose == null || ((hkaPose)havokPose).Skeleton == null)
		{
			return;
		}
		Vector3 position = target.Position;
		Vector3 deltaPos = position - initial.Position;
		Quaternion deltaRot = Quaternion.Normalize(target.Rotation / initial.Rotation);
		Propagate(havokPose, boneIx, position, deltaPos, deltaRot);
		if (partialIx != 0 || !propagatePartials)
		{
			return;
		}
		hkaSkeleton* skeleton = ((hkaPose)havokPose).Skeleton;
		for (int i = 0; i < ((Skeleton)skele).PartialSkeletonCount; i++)
		{
			PartialSkeleton val2 = ((PartialSkeleton*)((Skeleton)skele).PartialSkeletons)[i];
			if (((PartialSkeleton)(ref val2)).HavokPoses.IsEmpty)
			{
				continue;
			}
			hkaPose* havokPose2 = ((PartialSkeleton)(ref val2)).GetHavokPose(0);
			if (havokPose2 == null)
			{
				continue;
			}
			hkaSkeleton* skeleton2 = ((hkaPose)havokPose2).Skeleton;
			if (!IsMultiRootSkeleton(((hkaSkeleton)skeleton2).ParentIndices))
			{
				short connectedBoneIndex = val2.ConnectedBoneIndex;
				short connectedParentBoneIndex = val2.ConnectedParentBoneIndex;
				if (connectedParentBoneIndex == boneIx || IsBoneDescendantOf(((hkaSkeleton)skeleton).ParentIndices, connectedParentBoneIndex, boneIx))
				{
					Propagate(havokPose2, connectedBoneIndex, position, deltaPos, deltaRot);
				}
				continue;
			}
			foreach (int multiRoot in GetMultiRoots(((hkaSkeleton)skeleton2).ParentIndices))
			{
				hkaBone val3 = ((hkaSkeleton)skeleton2).Bones[multiRoot];
				short num = TryGetBoneNameIndex(havokPose, ((hkStringPtr)(ref val3.Name)).String);
				val3 = ((hkaSkeleton)skeleton).Bones[boneIx];
				string text = ((hkStringPtr)(ref val3.Name)).String;
				val3 = ((hkaSkeleton)skeleton2).Bones[multiRoot];
				bool num2 = text == ((hkStringPtr)(ref val3.Name)).String;
				bool flag = num != -1 && IsBoneDescendantOf(((hkaSkeleton)skeleton).ParentIndices, num, boneIx);
				if (num2 || flag)
				{
					Propagate(havokPose2, multiRoot, position, deltaPos, deltaRot);
				}
			}
		}
	}

	private unsafe static void Propagate(hkaPose* pose, int boneIx, Vector3 sourcePos, Vector3 deltaPos, Quaternion deltaRot)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		hkaSkeleton* skeleton = ((hkaPose)pose).Skeleton;
		for (int i = boneIx; i < ((hkaSkeleton)skeleton).Bones.Length; i++)
		{
			if (IsBoneDescendantOf(((hkaSkeleton)skeleton).ParentIndices, i, boneIx))
			{
				Transform modelTransform = GetModelTransform(pose, i);
				if (modelTransform == null)
				{
					Ktisis.Log.Error($"HavokPosing.Propagate - null transform returned for pose; boneI {i} boneIx {boneIx}");
				}
				else
				{
					Matrix4x4 matrix4x = Matrix4x4.CreateScale(ClampVector3(modelTransform.Scale));
					Matrix4x4 matrix4x2 = Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(deltaRot * modelTransform.Rotation));
					Matrix4x4 matrix4x3 = Matrix4x4.CreateTranslation(deltaPos + sourcePos + Vector3.Transform(modelTransform.Position - sourcePos, deltaRot));
					SetModelTransform(pose, i, new Transform(matrix4x * matrix4x2 * matrix4x3, modelTransform));
				}
			}
		}
	}

	private static Vector3 ClampVector3(Vector3 vector)
	{
		float x = ((vector.X < 0.001f && vector.X > -0.001f) ? 0.001f : vector.X);
		float y = ((vector.Y < 0.001f && vector.Y > -0.001f) ? 0.001f : vector.Y);
		float z = ((vector.Z < 0.001f && vector.Z > -0.001f) ? 0.001f : vector.Z);
		return new Vector3(x, y, z);
	}

	public unsafe static Quaternion ParentSkeleton(Skeleton* modelSkeleton, int partialIndex)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		PartialSkeleton val = ((PartialSkeleton*)((Skeleton)modelSkeleton).PartialSkeletons)[partialIndex];
		hkaPose* havokPose = ((PartialSkeleton)(ref val)).GetHavokPose(0);
		if (havokPose == null)
		{
			return Quaternion.Identity;
		}
		PartialSkeleton partialSkeletons = *((Skeleton)modelSkeleton).PartialSkeletons;
		hkaPose* havokPose2 = ((PartialSkeleton)(ref partialSkeletons)).GetHavokPose(0);
		if (havokPose2 == null)
		{
			return Quaternion.Identity;
		}
		Transform modelTransform = GetModelTransform(havokPose, val.ConnectedBoneIndex);
		Transform modelTransform2 = GetModelTransform(havokPose2, val.ConnectedParentBoneIndex);
		Quaternion quaternion = Quaternion.Normalize(modelTransform2.Rotation / modelTransform.Rotation);
		Transform transform = new Transform(modelTransform2.Position, modelTransform.Rotation, modelTransform.Scale);
		SetModelTransform(havokPose, val.ConnectedBoneIndex, transform);
		Propagate(modelSkeleton, partialIndex, val.ConnectedBoneIndex, transform, modelTransform);
		Transform transform2 = new Transform(modelTransform2.Position, Quaternion.Normalize(quaternion * modelTransform.Rotation), modelTransform2.Scale);
		SetModelTransform(havokPose, val.ConnectedBoneIndex, transform2);
		Propagate(modelSkeleton, partialIndex, val.ConnectedBoneIndex, transform2, transform);
		return quaternion;
	}

	public unsafe static void SyncModelSpace(Skeleton* skeleton, int partialIndex)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (skeleton == null || ((Skeleton)skeleton).PartialSkeletons == null)
		{
			return;
		}
		PartialSkeleton val = ((PartialSkeleton*)((Skeleton)skeleton).PartialSkeletons)[partialIndex];
		hkaPose* havokPose = ((PartialSkeleton)(ref val)).GetHavokPose(0);
		if (havokPose == null || ((hkaPose)havokPose).Skeleton == null)
		{
			return;
		}
		for (int i = 1; i < ((hkaSkeleton)((hkaPose)havokPose).Skeleton).Bones.Length; i++)
		{
			Transform modelTransform = GetModelTransform(havokPose, ((hkaSkeleton)((hkaPose)havokPose).Skeleton).ParentIndices[i]);
			if (modelTransform != null)
			{
				Transform localTransform = GetLocalTransform(havokPose, i);
				Transform modelTransform2 = GetModelTransform(havokPose, i);
				modelTransform2.Position = modelTransform.Position + Vector3.Transform(localTransform.Position, modelTransform.Rotation);
				modelTransform2.Rotation = Quaternion.Normalize(modelTransform.Rotation * localTransform.Rotation);
				SetModelTransform(havokPose, i, modelTransform2);
			}
		}
		if (partialIndex > 0)
		{
			ParentSkeleton(skeleton, partialIndex);
		}
	}

	public unsafe static short TryGetBoneNameIndex(hkaPose* pose, string? name)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (pose == null || ((hkaPose)pose).Skeleton == null || StringExtensions.IsNullOrEmpty(name))
		{
			return -1;
		}
		hkArray<hkaBone> bones = ((hkaSkeleton)((hkaPose)pose).Skeleton).Bones;
		for (short num = 0; num < bones.Length; num++)
		{
			hkaBone val = bones[(int)num];
			if (((hkStringPtr)(ref val.Name)).String == name)
			{
				return num;
			}
		}
		return -1;
	}

	public static bool IsBoneDescendantOf(hkArray<short> indices, int bone, int parent)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!IsMultiRootSkeleton(indices) && parent < 1)
		{
			return true;
		}
		for (short num = indices[bone]; num != -1; num = indices[(int)num])
		{
			if (num == parent)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsMultiRootSkeleton(hkArray<short> indices)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (GetMultiRoots(indices).Count > 1)
		{
			return true;
		}
		return false;
	}

	public static List<int> GetMultiRoots(hkArray<short> indices)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		List<int> list = new List<int>();
		for (int i = 0; i < indices.Length; i++)
		{
			if (indices[i] == -1)
			{
				list.Add(i);
			}
		}
		return list;
	}
}
