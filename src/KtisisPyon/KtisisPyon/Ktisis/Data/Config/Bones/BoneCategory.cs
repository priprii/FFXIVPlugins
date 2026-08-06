using System.Collections.Generic;

namespace Ktisis.Data.Config.Bones;

public class BoneCategory(string name)
{
	public readonly string Name = name;

	public uint GroupColor = 4294942568u;

	public uint BoneColor = uint.MaxValue;

	public bool LinkedColors;

	public bool HideOnPoseEntity;

	public bool IsNsfw;

	public bool IsDefault;

	public string? ParentCategory;

	public int? SortPriority;

	public readonly List<CategoryBone> Bones = new List<CategoryBone>();

	public TwoJointsGroupParams? TwoJointsGroup;

	public CcdGroupParams? CcdGroup;

	public List<string> Presets = new List<string>();
}
