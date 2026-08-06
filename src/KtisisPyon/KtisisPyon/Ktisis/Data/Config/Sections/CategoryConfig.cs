using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Container.String;
using Ktisis.Data.Config.Bones;

namespace Ktisis.Data.Config.Sections;

public class CategoryConfig
{
	public readonly List<BoneCategory> CategoryList = new List<BoneCategory>();

	public bool ShowNsfwBones = true;

	public bool ShowAllVieraEars;

	public bool ShowFriendlyBoneNames = true;

	public BoneCategory? Default { get; set; }

	public void AddCategory(BoneCategory category)
	{
		Ktisis.Log.Debug("Registering category: " + category.Name);
		if (category.IsDefault)
		{
			Default = category;
		}
		int valueOrDefault = category.SortPriority.GetValueOrDefault();
		if (!category.SortPriority.HasValue)
		{
			valueOrDefault = CategoryList.Count;
			category.SortPriority = valueOrDefault;
		}
		CategoryList.Add(category);
	}

	public BoneCategory? GetByName(string name)
	{
		return CategoryList.Find((BoneCategory category) => category.Name == name);
	}

	public BoneCategory? GetByNameOrDefault(string name)
	{
		return GetByName(name) ?? Default;
	}

	public BoneCategory? GetForBoneName(string name)
	{
		return CategoryList.Find((BoneCategory category) => category.Bones.Any((CategoryBone bone) => bone.Name == name));
	}

	public BoneCategory? GetForBoneNameOrDefault(string name)
	{
		return GetForBoneName(name) ?? Default;
	}

	public unsafe BoneCategory? ResolveBestCategory(hkaSkeleton* skeleton, int index)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (skeleton == null)
		{
			return null;
		}
		while (index > -1)
		{
			hkaBone val = ((hkaSkeleton)skeleton).Bones[index];
			string text = ((hkStringPtr)(ref val.Name)).String;
			if (text == null)
			{
				break;
			}
			BoneCategory forBoneName = GetForBoneName(text);
			if (forBoneName != null)
			{
				return forBoneName;
			}
			if (text.StartsWith("j_ex_h"))
			{
				return GetByNameOrDefault("Hair");
			}
			index = ((hkaSkeleton)skeleton).ParentIndices[index];
		}
		return Default;
	}
}
