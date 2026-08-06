using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using Ktisis.Common.Utility;
using Ktisis.Editor.Posing;
using Ktisis.Editor.Posing.Attachment;
using Ktisis.Editor.Posing.Types;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Types;
using Ktisis.Structs.Attachment;

namespace Ktisis.Scene.Entities.Skeleton;

public class BoneNode : SkeletonNode, ITransform, IVisibility, IAttachTarget
{
	public PartialBoneInfo Info;

	public uint PartialId;

	public bool Visible { get; set; }

	public unsafe override bool IsValid
	{
		get
		{
			if (base.IsValid && GetSkeleton() != null)
			{
				return GetPose() != null;
			}
			return false;
		}
	}

	public BoneNode(ISceneManager scene, EntityPose pose, PartialBoneInfo bone, uint partialId)
		: base(scene)
	{
		base.Type = EntityType.BoneNode;
		base.Pose = pose;
		Info = bone;
		PartialId = partialId;
	}

	public unsafe hkaPose* GetPose()
	{
		return base.Pose.GetPose(Info.PartialIndex);
	}

	public unsafe Skeleton* GetSkeleton()
	{
		return base.Pose.GetSkeleton();
	}

	public bool MatchesId(int pId, int bId)
	{
		if (Info.PartialIndex == pId)
		{
			return Info.BoneIndex == bId;
		}
		return false;
	}

	public unsafe Matrix4x4? GetMatrixModel()
	{
		hkaPose* pose = GetPose();
		if (pose == null)
		{
			return null;
		}
		return HavokPosing.GetMatrix(pose, Info.BoneIndex);
	}

	protected unsafe Matrix4x4? CalcMatrixWorld()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		Skeleton* skeleton = GetSkeleton();
		if (skeleton != null)
		{
			Matrix4x4? matrixModel = GetMatrixModel();
			if (matrixModel.HasValue)
			{
				Matrix4x4 valueOrDefault = matrixModel.GetValueOrDefault();
				Transform transform = new Transform(((Skeleton)skeleton).Transform);
				valueOrDefault.Translation *= transform.Scale;
				valueOrDefault = Matrix4x4.Transform(valueOrDefault, transform.Rotation);
				valueOrDefault.Translation += transform.Position;
				return valueOrDefault;
			}
		}
		return null;
	}

	protected unsafe void SetMatrixWorld(Matrix4x4 matrix)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Skeleton* skeleton = GetSkeleton();
		hkaPose* ptr = ((skeleton != null) ? GetPose() : null);
		if (ptr != null)
		{
			Transform transformModel = GetTransformModel();
			if (transformModel != null)
			{
				Transform transform = new Transform(((Skeleton)skeleton).Transform);
				matrix.Translation -= transform.Position;
				matrix = Matrix4x4.Transform(matrix, Quaternion.Inverse(transform.Rotation));
				matrix.Translation /= transform.Scale;
				HavokPosing.SetModelTransform(ptr, Info.BoneIndex, new Transform(matrix, transformModel));
			}
		}
	}

	protected void SetTransformWorld(Transform transform)
	{
		SetMatrixWorld(transform.ComposeMatrix());
	}

	public Transform? CalcTransformWorld()
	{
		Matrix4x4? matrix4x = CalcMatrixWorld();
		if (matrix4x.HasValue)
		{
			Transform transformModel = GetTransformModel();
			if (transformModel != null)
			{
				return new Transform(matrix4x.Value, transformModel);
			}
		}
		return null;
	}

	public Transform? CalcTransformOverlay()
	{
		Transform transform = CalcTransformWorld();
		if (transform == null)
		{
			return null;
		}
		Vector3? offset = Scene.Context.Config.Offsets.GetOffset(this);
		if (offset.HasValue)
		{
			Vector3 vector = Vector3.Transform(offset.Value, transform.Rotation);
			transform.Position += vector;
		}
		return transform;
	}

	public unsafe Transform? GetTransformModel()
	{
		hkaPose* pose = GetPose();
		if (pose == null)
		{
			return null;
		}
		return HavokPosing.GetModelTransform(pose, Info.BoneIndex);
	}

	public unsafe bool IsBoneChildOf(BoneNode node)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (base.Pose != node.Pose)
		{
			return false;
		}
		Skeleton* skeleton = GetSkeleton();
		if (skeleton == null || ((Skeleton)skeleton).PartialSkeletons == null)
		{
			return false;
		}
		if (Info.PartialIndex == node.Info.PartialIndex)
		{
			return Info.ParentIndex == node.Info.BoneIndex;
		}
		if (node.Info.PartialIndex != 0)
		{
			return false;
		}
		PartialSkeleton val = ((PartialSkeleton*)((Skeleton)skeleton).PartialSkeletons)[Info.PartialIndex];
		if (Info.BoneIndex == val.ConnectedBoneIndex)
		{
			return node.Info.BoneIndex == val.ConnectedParentBoneIndex;
		}
		return false;
	}

	public unsafe bool IsBoneDescendantOf(BoneNode node)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		if (base.Pose != node.Pose)
		{
			return false;
		}
		Skeleton* skeleton = GetSkeleton();
		if (skeleton == null || ((Skeleton)skeleton).PartialSkeletons == null)
		{
			return false;
		}
		PartialSkeleton val = ((PartialSkeleton*)((Skeleton)skeleton).PartialSkeletons)[Info.PartialIndex];
		int partialIndex = Info.PartialIndex;
		int partialIndex2 = node.Info.PartialIndex;
		hkaPose* havokPose;
		int num;
		int boneIndex;
		if (partialIndex != partialIndex2)
		{
			if (partialIndex == 0 || partialIndex2 != 0)
			{
				return false;
			}
			PartialSkeleton partialSkeletons = *((Skeleton)skeleton).PartialSkeletons;
			havokPose = ((PartialSkeleton)(ref partialSkeletons)).GetHavokPose(0);
			num = val.ConnectedParentBoneIndex;
			boneIndex = node.Info.BoneIndex;
			if (num == boneIndex)
			{
				return true;
			}
		}
		else
		{
			havokPose = ((PartialSkeleton)(ref val)).GetHavokPose(0);
			num = Info.BoneIndex;
			boneIndex = node.Info.BoneIndex;
		}
		if (havokPose != null && ((hkaPose)havokPose).Skeleton != null)
		{
			return HavokPosing.IsBoneDescendantOf(((hkaSkeleton)((hkaPose)havokPose).Skeleton).ParentIndices, num, boneIndex);
		}
		return false;
	}

	public bool IsVieraEarBone()
	{
		if (Info.Name.Length >= 7 && Info.Name.StartsWith("j_zer"))
		{
			return Info.Name[6] == '_';
		}
		return false;
	}

	public virtual Transform? GetTransform()
	{
		return CalcTransformWorld();
	}

	public virtual void SetTransform(Transform transform)
	{
		SetTransformWorld(transform);
	}

	public virtual Matrix4x4? GetMatrix()
	{
		return CalcMatrixWorld();
	}

	public virtual void SetMatrix(Matrix4x4 matrix)
	{
		SetMatrixWorld(matrix);
	}

	public unsafe bool TryAcceptAttach(IAttachable child)
	{
		if (Info.PartialIndex > 0)
		{
			return false;
		}
		Attach* attach = child.GetAttach();
		CharacterBase* character = child.GetCharacter();
		if (attach == null || character == null)
		{
			return false;
		}
		Skeleton* skeleton = GetSkeleton();
		Skeleton* skeleton2 = ((CharacterBase)character).Skeleton;
		if (skeleton == null || skeleton2 == null)
		{
			return false;
		}
		AttachUtility.SetBoneAttachment(skeleton, skeleton2, attach, (ushort)Info.BoneIndex, 0);
		return true;
	}
}
