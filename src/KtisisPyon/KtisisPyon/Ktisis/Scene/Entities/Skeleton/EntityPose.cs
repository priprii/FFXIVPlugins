using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.STD;
using Ktisis.Common.Extensions;
using Ktisis.Common.Utility;
using Ktisis.Editor.Posing.Ik;
using Ktisis.Editor.Posing.Types;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Entities.Character;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Factory.Builders;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Entities.Skeleton;

public class EntityPose : SkeletonGroup, ISkeleton, IConfigurable
{
	private readonly IPoseBuilder _builder;

	public readonly IIkController IkController;

	public bool OverlayVisible;

	private readonly Dictionary<int, PartialSkeletonInfo> Partials = new Dictionary<int, PartialSkeletonInfo>();

	private readonly Dictionary<(int p, int i), BoneNode> BoneMap = new Dictionary<(int, int), BoneNode>();

	public EntityPose(ISceneManager scene, IPoseBuilder builder, IIkController ik)
		: base(scene)
	{
		_builder = builder;
		IkController = ik;
		base.Type = EntityType.Armature;
		Name = "Pose";
		base.Pose = this;
	}

	public override void Update()
	{
		if (IsValid)
		{
			UpdatePose();
		}
	}

	public unsafe void Refresh()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Partials.Clear();
		Skeleton* skeleton = GetSkeleton();
		if (skeleton != null)
		{
			for (int i = 0; i < ((Skeleton)skeleton).PartialSkeletonCount; i++)
			{
				uint partialId = GetPartialId(((PartialSkeleton*)((Skeleton)skeleton).PartialSkeletons)[i]);
				Clean(i, partialId);
			}
		}
	}

	private unsafe void UpdatePose()
	{
		Skeleton* skeleton = GetSkeleton();
		if (skeleton != null)
		{
			for (int i = 0; i < ((Skeleton)skeleton).PartialSkeletonCount; i++)
			{
				UpdatePartial(skeleton, i);
			}
		}
	}

	private unsafe void UpdatePartial(Skeleton* skeleton, int index)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Invalid comparison between Unknown and I4
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		PartialSkeleton partial = ((PartialSkeleton*)((Skeleton)skeleton).PartialSkeletons)[index];
		uint partialId = GetPartialId(partial);
		uint num = 0u;
		if (Partials.TryGetValue(index, out PartialSkeletonInfo value))
		{
			num = value.Id;
		}
		else
		{
			string partialName = GetPartialName(partial);
			value = ((partialName != null) ? new PartialSkeletonInfo(partialId, partialName) : new PartialSkeletonInfo(partialId));
			Partials.Add(index, value);
		}
		if (partialId != num)
		{
			Ktisis.Log.Verbose($"Skeleton of '{Parent?.Name ?? "UNKNOWN"}' detected a change in partial #{index} (was {num:X}, now {partialId:X}), rebuilding.");
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			IBoneTreeBuilder boneTreeBuilder = _builder.BuildBoneTree(index, partialId, partial);
			if ((int)((CharacterBase)((Skeleton)skeleton).Owner).GetModelType() != 4)
			{
				boneTreeBuilder.BuildCategoryMap();
			}
			else
			{
				boneTreeBuilder.BuildBoneList();
			}
			if (num != 0)
			{
				Clean(index, partialId);
			}
			value.CopyPartial(partialId, partial);
			value.Name = GetPartialName(partial);
			if (partialId != 0)
			{
				boneTreeBuilder.BindTo(this);
			}
			FilterTree();
			BuildBoneMap(index, partialId);
			if (Scene.Context.Posing.IsEnabled)
			{
				Scene.Context.Posing.ApplyPartialReferencePose(this, index);
			}
			stopwatch.Stop();
			Ktisis.Log.Debug($"Rebuild took {stopwatch.Elapsed.TotalMilliseconds:00.00}ms");
		}
	}

	private void BuildBoneMap(int index, uint id)
	{
		foreach (var item in BoneMap.Keys.Where(((int p, int i) key) => key.p == index))
		{
			BoneMap.Remove(item);
		}
		if (id == 0)
		{
			return;
		}
		foreach (SceneEntity item2 in Recurse())
		{
			if (item2 is BoneNode boneNode && boneNode.Info.PartialIndex == index)
			{
				BoneMap[(index, boneNode.Info.BoneIndex)] = boneNode;
			}
		}
	}

	private unsafe static uint GetPartialId(PartialSkeleton partial)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		SkeletonResourceHandle* skeletonResourceHandle = partial.SkeletonResourceHandle;
		if (skeletonResourceHandle == null)
		{
			return 0u;
		}
		return ((SkeletonResourceHandle)skeletonResourceHandle).Id;
	}

	private unsafe static string? GetPartialName(PartialSkeleton partial)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		SkeletonResourceHandle* skeletonResourceHandle = partial.SkeletonResourceHandle;
		if (skeletonResourceHandle == null)
		{
			return null;
		}
		return ((object)(*(StdString*)(&((DefaultResourceHandle)(&((SkeletonResourceHandle)skeletonResourceHandle).DefaultResourceHandle)).FileName))/*cast due to constrained. prefix*/).ToString();
	}

	private void FilterTree()
	{
		List<BoneNode> source = (from entity in Recurse()
			where entity is BoneNode
			select entity).Cast<BoneNode>().ToList();
		IEnumerable<BoneNode> enumerable = Enumerable.Empty<BoneNode>();
		if (source.Any((BoneNode bone) => bone.Info.Name == "j_f_ago"))
		{
			IEnumerable<BoneNode> second = source.Where((BoneNode bone) => bone.Info.Name == "j_ago");
			enumerable = enumerable.Concat(second);
		}
		if (!Scene.Context.Config.Categories.ShowAllVieraEars && Parent is ActorEntity actorEntity && actorEntity.TryGetEarIdAsChar(out var earId))
		{
			IEnumerable<BoneNode> second2 = source.Where((BoneNode bone) => bone.IsVieraEarBone() && bone.Info.Name[5] != earId);
			enumerable = enumerable.Concat(second2);
		}
		foreach (BoneNode item in enumerable)
		{
			item.Remove();
		}
	}

	public bool HasDTFace()
	{
		return BoneExists("j_f_face");
	}

	public bool HasBunnyEars()
	{
		return AnyBoneExists(PoseUtil.BunnyEarBones);
	}

	public bool HasTail()
	{
		return BoneExists("n_sippo_a");
	}

	public unsafe Skeleton* GetSkeleton()
	{
		if (!IsValid)
		{
			return null;
		}
		if (!(Parent is CharaEntity charaEntity) || !charaEntity.IsDrawing())
		{
			return null;
		}
		if (Parent is ActorEntity actorEntity && !actorEntity.Actor.IsDrawing())
		{
			return null;
		}
		CharacterBase* character = charaEntity.GetCharacter();
		if (character == null)
		{
			return null;
		}
		return ((CharacterBase)character).Skeleton;
	}

	public unsafe hkaPose* GetPose(int index)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Skeleton* skeleton = GetSkeleton();
		if (skeleton == null)
		{
			return null;
		}
		PartialSkeleton val = ((PartialSkeleton*)((Skeleton)skeleton).PartialSkeletons)[index];
		return ((PartialSkeleton)(ref val)).GetHavokPose(0);
	}

	public BoneNode? GetBoneFromMap(int partialIx, int boneIx)
	{
		return BoneMap.GetValueOrDefault((partialIx, boneIx));
	}

	public BoneNode? FindBoneByName(string name)
	{
		return BoneMap.Values.FirstOrDefault((BoneNode bone) => bone.Info.Name == name);
	}

	public bool BoneExists(string name)
	{
		return BoneMap.Values.Any((BoneNode bone) => bone.Info.Name == name);
	}

	public bool AnyBoneExists(string[] names)
	{
		return BoneMap.Values.Any((BoneNode bone) => names.Contains(bone.Info.Name));
	}

	public BoneNode? TryResolveSibling(BoneNode bone)
	{
		string name = bone.Info.Name;
		if (!name.EndsWith("_l") && !name.EndsWith("_r"))
		{
			return null;
		}
		string text = name;
		string prefix = text.Substring(0, text.Length - 2);
		return BoneMap.Values.FirstOrDefault(delegate(BoneNode potentialBone)
		{
			string name2 = potentialBone.Info.Name;
			return name2.Substring(0, name2.Length - 2) == prefix && potentialBone.Info.Name != name;
		});
	}

	public PartialSkeletonInfo? GetPartialInfo(int index)
	{
		return Partials.GetValueOrDefault(index);
	}

	public IEnumerable<int> GetPartialIndices()
	{
		return Partials.Keys;
	}

	public IEnumerable<PartialBoneInfo> ExpandToDescendants(IEnumerable<PartialBoneInfo> bones)
	{
		HashSet<PartialBoneInfo> hashSet = new HashSet<PartialBoneInfo>(bones);
		Stack<PartialBoneInfo> stack = new Stack<PartialBoneInfo>(hashSet);
		BoneNode[] array = BoneMap.Values.ToArray();
		while (stack.Count > 0)
		{
			PartialBoneInfo partialBoneInfo = stack.Pop();
			BoneNode boneFromMap = GetBoneFromMap(partialBoneInfo.PartialIndex, partialBoneInfo.BoneIndex);
			if (boneFromMap == null)
			{
				continue;
			}
			BoneNode[] array2 = array;
			foreach (BoneNode boneNode in array2)
			{
				if (!hashSet.Contains(boneNode.Info) && boneNode.IsBoneDescendantOf(boneFromMap))
				{
					hashSet.Add(boneNode.Info);
					stack.Push(boneNode.Info);
				}
			}
		}
		return hashSet;
	}

	public bool ShouldDraw()
	{
		return Recurse().OfType<IVisibility>().Any((IVisibility vis) => vis.Visible);
	}

	public override void Remove()
	{
		try
		{
			IkController.Destroy();
		}
		finally
		{
			base.Remove();
		}
	}
}
