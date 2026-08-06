using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;
using Ktisis.Data.Config;
using Ktisis.Data.Config.Bones;
using Ktisis.Data.Config.Sections;
using Ktisis.Editor.Posing.Ik;
using Ktisis.Editor.Posing.Ik.Ccd;
using Ktisis.Editor.Posing.Ik.TwoJoints;
using Ktisis.Editor.Posing.Types;
using Ktisis.Localization;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Scene.Entities.Skeleton.Constraints;
using Ktisis.Scene.Factory.Types;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Factory.Builders;

public sealed class PoseBuilder : EntityBuilder<EntityPose, IPoseBuilder>, IPoseBuilder, IEntityBuilder<EntityPose, IPoseBuilder>, IEntityBuilderBase<EntityPose, IPoseBuilder>
{
	private class BoneTreeBuilder : BoneEnumerator, IBoneTreeBuilder
	{
		private readonly ISceneManager _scene;

		private readonly uint PartialId;

		private readonly Dictionary<BoneCategory, List<PartialBoneInfo>> CategoryMap = new Dictionary<BoneCategory, List<PartialBoneInfo>>();

		private readonly List<PartialBoneInfo> BoneList = new List<PartialBoneInfo>();

		private Configuration Config => _scene.Context.Config;

		private LocaleManager Locale => _scene.Context.Locale;

		public BoneTreeBuilder(ISceneManager scene, int index, uint partialId, PartialSkeleton partial)
			: base(index, partial)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			_scene = scene;
			PartialId = partialId;
		}

		public unsafe IBoneTreeBuilder BuildBoneList()
		{
			if (GetSkeleton() != null)
			{
				IEnumerable<PartialBoneInfo> collection = EnumerateBones();
				BoneList.Clear();
				BoneList.AddRange(collection);
			}
			return this;
		}

		public unsafe IBoneTreeBuilder BuildCategoryMap()
		{
			CategoryMap.Clear();
			CategoryConfig categories = Config.Categories;
			hkaSkeleton* skeleton = GetSkeleton();
			if (skeleton == null)
			{
				return this;
			}
			foreach (PartialBoneInfo item in EnumerateBones())
			{
				BoneCategory boneCategory = categories.ResolveBestCategory(skeleton, item.BoneIndex);
				if (boneCategory == null)
				{
					Ktisis.Log.Warning("Failed to find category for " + item.Name + "! Skipping...");
				}
				else if (!boneCategory.IsNsfw || Config.Categories.ShowNsfwBones)
				{
					if (CategoryMap.TryGetValue(boneCategory, out List<PartialBoneInfo> value))
					{
						value.Add(item);
						continue;
					}
					Dictionary<BoneCategory, List<PartialBoneInfo>> categoryMap = CategoryMap;
					int num = 1;
					List<PartialBoneInfo> list = new List<PartialBoneInfo>(num);
					CollectionsMarshal.SetCount(list, num);
					CollectionsMarshal.AsSpan(list)[0] = item;
					categoryMap.Add(boneCategory, list);
				}
			}
			BuildOrphanedCategories(categories);
			return this;
		}

		private void BuildOrphanedCategories(CategoryConfig categories)
		{
			List<BoneCategory> keys = CategoryMap.Keys.ToList();
			foreach (BoneCategory category in keys.Where((BoneCategory boneCategory2) => boneCategory2.ParentCategory != null && keys.All((BoneCategory x) => x.Name != boneCategory2.ParentCategory)).ToList())
			{
				BoneCategory boneCategory = categories.CategoryList.Find((BoneCategory x) => x.Name == category.ParentCategory);
				if (boneCategory != null && !CategoryMap.ContainsKey(boneCategory))
				{
					CategoryMap.Add(boneCategory, new List<PartialBoneInfo>());
				}
			}
		}

		public void BindTo(EntityPose pose)
		{
			if (CategoryMap.Count > 0)
			{
				BindGroups(pose, null);
			}
			if (BoneList.Count > 0)
			{
				BindBones(pose, BoneList);
			}
		}

		private void BindGroups(SkeletonNode node, BoneCategory? parent)
		{
			KeyValuePair<BoneCategory, List<PartialBoneInfo>>[] categories = CategoryMap.Where<KeyValuePair<BoneCategory, List<PartialBoneInfo>>>((KeyValuePair<BoneCategory, List<PartialBoneInfo>> x) => x.Key.ParentCategory == parent?.Name).ToArray();
			List<BoneNodeGroup> list = null;
			List<SceneEntity> list2 = node.Children.ToList();
			if (list2.Count > 0)
			{
				list = list2.Where((SceneEntity x) => x is BoneNodeGroup).Cast<BoneNodeGroup>().ToList();
			}
			if (list != null)
			{
				foreach (BoneNodeGroup item in list.Where((BoneNodeGroup group) => categories.All((KeyValuePair<BoneCategory, List<PartialBoneInfo>> cat) => cat.Key.Name != group.Name)))
				{
					BindGroups(item, item.Category);
				}
			}
			KeyValuePair<BoneCategory, List<PartialBoneInfo>>[] array = categories;
			foreach (KeyValuePair<BoneCategory, List<PartialBoneInfo>> keyValuePair in array)
			{
				keyValuePair.Deconstruct(out var key, out var value);
				BoneCategory category = key;
				List<PartialBoneInfo> bones = value;
				BoneNodeGroup boneNodeGroup = list?.Find((BoneNodeGroup group) => group.Category == category);
				bool num2 = boneNodeGroup == null;
				if (boneNodeGroup == null)
				{
					boneNodeGroup = CreateGroupNode(node.Pose, category);
				}
				boneNodeGroup.Name = Locale.GetCategoryName(category);
				boneNodeGroup.Category = category;
				boneNodeGroup.SortPriority = category.SortPriority ?? (-1);
				BindGroups(boneNodeGroup, category);
				BindBones(boneNodeGroup, bones);
				if (num2 && boneNodeGroup.Children.Any())
				{
					node.Add(boneNodeGroup);
				}
			}
			node.OrderByPriority();
		}

		private void BindBones(SkeletonNode node, List<PartialBoneInfo> bones)
		{
			List<BoneNode> list = null;
			List<SceneEntity> list2 = node.Children.ToList();
			if (list2.Count > 0)
			{
				list = list2.Where((SceneEntity x) => x is BoneNode boneNode3 && boneNode3.Info.PartialIndex == Index).Cast<BoneNode>().ToList();
			}
			int num = Partial.ConnectedBoneIndex + 1;
			foreach (PartialBoneInfo boneInfo in bones)
			{
				BoneNode boneNode = list?.Find((BoneNode bone) => bone.Info.Name == boneInfo.Name);
				if (boneNode != null)
				{
					if (Index != boneNode.Info.PartialIndex)
					{
						node.Remove(boneNode);
						continue;
					}
					boneNode.Info = boneInfo;
					boneNode.PartialId = PartialId;
				}
				else
				{
					BoneNode boneNode2 = CreateBoneNode(node, boneInfo);
					boneNode2.Name = Locale.GetBoneName(boneInfo);
					boneNode2.SortPriority = num + boneInfo.BoneIndex;
					node.Add(boneNode2);
				}
			}
			node.OrderByPriority();
		}

		private BoneNodeGroup CreateGroupNode(EntityPose pose, BoneCategory category)
		{
			string name = category.Name;
			if (category != null)
			{
				TwoJointsGroupParams twoJointsGroup = category.TwoJointsGroup;
				if (twoJointsGroup != null)
				{
					TwoJointsGroupParams param = twoJointsGroup;
					if (pose.IkController.TrySetupGroup(name, param, out TwoJointsGroup group))
					{
						return new IkNodeGroup<TwoJointsGroup>(_scene, pose, group);
					}
				}
				CcdGroupParams ccdGroup = category.CcdGroup;
				if (ccdGroup != null)
				{
					CcdGroupParams param2 = ccdGroup;
					if (pose.IkController.TrySetupGroup(name, param2, out CcdGroup group2))
					{
						return new IkNodeGroup<CcdGroup>(_scene, pose, group2);
					}
				}
			}
			return new BoneNodeGroup(_scene, pose);
		}

		private BoneNode CreateBoneNode(SkeletonNode parent, PartialBoneInfo boneInfo)
		{
			if (!(parent is IkNodeGroup<TwoJointsGroup> ikNodeGroup))
			{
				if (parent is IkNodeGroup<CcdGroup> ikNodeGroup2 && ikNodeGroup2.Group.EndBoneIndex == boneInfo.BoneIndex)
				{
					return new CcdEndNode(_scene, parent.Pose, boneInfo, PartialId, ikNodeGroup2.Group);
				}
			}
			else if (ikNodeGroup.Group.EndBoneIndex == boneInfo.BoneIndex)
			{
				return new TwoJointEndNode(_scene, parent.Pose, boneInfo, PartialId, ikNodeGroup.Group);
			}
			return new BoneNode(_scene, parent.Pose, boneInfo, PartialId);
		}
	}

	protected override IPoseBuilder Builder => this;

	public PoseBuilder(ISceneManager scene)
		: base(scene)
	{
		base.Name = "Pose";
	}

	protected override EntityPose Build()
	{
		IIkController ikController = Scene.Context.Posing.CreateIkController();
		EntityPose entityPose = new EntityPose(Scene, this, ikController);
		ikController.Setup(entityPose);
		return entityPose;
	}

	public IBoneTreeBuilder BuildBoneTree(int index, uint partialId, PartialSkeleton partial)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return new BoneTreeBuilder(Scene, index, partialId, partial);
	}
}
