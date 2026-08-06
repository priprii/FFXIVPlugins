using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Container.String;
using Ktisis.Common.Utility;
using Ktisis.Editor.Posing.Attachment;
using Ktisis.Editor.Posing.Types;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Scene.Entities.World;
using Ktisis.Scene.Factory.Builders;
using Ktisis.Scene.Types;
using Ktisis.Structs.Attachment;
using Ktisis.Structs.Characters;

namespace Ktisis.Scene.Entities.Character;

public class CharaEntity : WorldEntity, IAttachable, ICharacter
{
	private readonly IPoseBuilder _pose;

	public EntityPose? Pose { get; private set; }

	public unsafe CharacterBaseEx* CharacterBaseEx => (CharacterBaseEx*)GetCharacter();

	public CharaEntity(ISceneManager scene, IPoseBuilder pose)
		: base(scene)
	{
		_pose = pose;
	}

	public override void Setup()
	{
		base.Setup();
		Pose = _pose.Add(this);
	}

	public override void Update()
	{
		if (IsDrawing())
		{
			base.Update();
		}
	}

	public unsafe bool IsDrawing()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I8
		CharacterBase* character = GetCharacter();
		if (character == null)
		{
			return false;
		}
		return (long)(((CharacterBase)character).StateFlags & 0xFF00) > 0L;
	}

	public unsafe virtual CharacterBase* GetCharacter()
	{
		return (CharacterBase*)GetObject();
	}

	public unsafe Attach* GetAttach()
	{
		if (CharacterBaseEx == null)
		{
			return null;
		}
		Attach* ptr = &CharacterBaseEx->Attach;
		if (ptr->Param == null)
		{
			return null;
		}
		return ptr;
	}

	public unsafe virtual bool IsAttached()
	{
		Attach* attach = GetAttach();
		if (attach != null)
		{
			return attach->IsActive();
		}
		return false;
	}

	public unsafe PartialBoneInfo? GetParentBone()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		Attach* attach = GetAttach();
		if (attach == null)
		{
			return null;
		}
		Skeleton* parentSkeleton = attach->GetParentSkeleton();
		if (parentSkeleton == null || ((Skeleton)parentSkeleton).PartialSkeletons == null || ((PartialSkeleton)((Skeleton)parentSkeleton).PartialSkeletons).HavokPoses.IsEmpty)
		{
			return null;
		}
		hkaPose* havokPose = ((PartialSkeleton)((Skeleton)parentSkeleton).PartialSkeletons).GetHavokPose(0);
		if (havokPose == null || ((hkaPose)havokPose).Skeleton == null)
		{
			return null;
		}
		if (!AttachUtility.TryGetParentBoneIndex(attach, out var index))
		{
			return null;
		}
		hkaSkeleton* skeleton = ((hkaPose)havokPose).Skeleton;
		PartialBoneInfo partialBoneInfo = new PartialBoneInfo();
		hkaBone val = ((hkaSkeleton)skeleton).Bones[(int)index];
		partialBoneInfo.Name = ((hkStringPtr)(ref val.Name)).String ?? string.Empty;
		partialBoneInfo.BoneIndex = index;
		partialBoneInfo.ParentIndex = ((hkaSkeleton)skeleton).ParentIndices[(int)index];
		partialBoneInfo.PartialIndex = 0;
		return partialBoneInfo;
	}

	public unsafe virtual void Detach()
	{
		Attach* attach = GetAttach();
		if (attach != null)
		{
			AttachUtility.Detach(attach);
		}
	}

	public unsafe override void SetTransform(Transform trans)
	{
		Attach* attach = GetAttach();
		if (attach != null && attach->IsActive())
		{
			Transform transform = GetTransform();
			AttachUtility.SetTransformRelative(attach, trans, transform);
			if (!(transform.Scale == trans.Scale))
			{
				transform.Scale = trans.Scale;
				base.SetTransform(transform);
			}
		}
		else
		{
			base.SetTransform(trans);
		}
	}
}
