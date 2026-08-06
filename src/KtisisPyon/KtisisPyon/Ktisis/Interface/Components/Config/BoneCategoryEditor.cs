using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Core.Attributes;
using Ktisis.Data.Config;
using Ktisis.Data.Config.Bones;
using Ktisis.Data.Config.Sections;
using Ktisis.Localization;

namespace Ktisis.Interface.Components.Config;

[Transient]
public class BoneCategoryEditor
{
	private readonly ConfigManager _cfg;

	private readonly LocaleManager _locale;

	private readonly Dictionary<string, List<BoneCategory>> CategoryMap = new Dictionary<string, List<BoneCategory>>();

	private BoneCategory? Selected;

	private bool ColorSub;

	private CategoryConfig Config => _cfg.File.Categories;

	public BoneCategoryEditor(ConfigManager cfg, LocaleManager locale)
	{
		_cfg = cfg;
		_locale = locale;
	}

	public void Setup()
	{
		Selected = null;
		BuildCategoryMap();
	}

	private void BuildCategoryMap()
	{
		CategoryMap.Clear();
		for (int i = -1; i < Config.CategoryList.Count; i++)
		{
			string parent = ((i >= 0) ? Config.CategoryList[i].Name : null);
			List<BoneCategory> list = Config.CategoryList.Where((BoneCategory cat) => cat.ParentCategory == parent).ToList();
			if (list.Count > 0)
			{
				CategoryMap.Add(parent ?? string.Empty, list);
			}
		}
	}

	public void Draw()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		ImGui.SetCursorPosX(600f);
		ImGui.Dummy(Vector2.Zero);
		StyleDisposable val = ImRaii.PushStyle((ImGuiStyleVar)1, Vector2.Zero, true);
		try
		{
			ChildDisposable val2 = ImRaii.Child(ImU8String.op_Implicit("##BoneCategoriesFrame"), ImGui.GetContentRegionAvail() - (_cfg.File.Editor.UseToolbar ? new Vector2(0f, 2f) : Vector2.Zero), true);
			try
			{
				if (!val2.Success)
				{
					return;
				}
				StyleDisposable val3 = ImRaii.PushStyle((ImGuiStyleVar)16, new Vector2(10f, 10f), true);
				try
				{
					TableDisposable val4 = ImRaii.Table(ImU8String.op_Implicit("##BoneCategoriesTable"), 2, (ImGuiTableFlags)1);
					try
					{
						if (val4.Success)
						{
							ImGui.TableSetupColumn(ImU8String.op_Implicit("CategoryList"), (ImGuiTableColumnFlags)0, 0f, 0u);
							ImGui.TableSetupColumn(ImU8String.op_Implicit("CategoryInfo"), (ImGuiTableColumnFlags)0, 0f, 0u);
							ImGui.TableNextRow();
							DrawCategoryList();
							DrawCategoryInfo();
							ImGuiStylePtr style = ImGui.GetStyle();
							Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
							contentRegionAvail.X = 0f;
							Vector2 vector = contentRegionAvail;
							vector.Y -= ((ImGuiStylePtr)(ref style)).ItemSpacing.Y + ((ImGuiStylePtr)(ref style)).CellPadding.Y;
							ImGui.Dummy(vector);
						}
					}
					finally
					{
						((TableDisposable)(ref val4)).Dispose();
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			finally
			{
				((ChildDisposable)(ref val2)).Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DrawCategoryList()
	{
		ImGui.TableNextColumn();
		StyleDisposable val = ImRaii.PushStyle((ImGuiStyleVar)15, UiBuilder.DefaultFontSizePx * ImGuiHelpers.GlobalScale, true);
		try
		{
			DrawCategoryList(string.Empty);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DrawCategoryList(string key)
	{
		if (CategoryMap.TryGetValue(key, out List<BoneCategory> value))
		{
			value.ForEach(DrawListCategory);
		}
	}

	private void DrawListCategory(BoneCategory category)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (category.IsNsfw && !Config.ShowNsfwBones)
		{
			return;
		}
		TreeNodeDisposable val = DrawCategoryNode(category);
		try
		{
			if (ImGui.IsItemClicked() && ImGui.GetItemRectMin().X + ImGui.GetTreeNodeToLabelSpacing() < ImGui.GetMousePos().X)
			{
				Selected = ((Selected != category) ? category : null);
			}
			if (val.Success)
			{
				DrawCategoryList(category.Name);
			}
		}
		finally
		{
			((TreeNodeDisposable)(ref val)).Dispose();
		}
	}

	private TreeNodeDisposable DrawCategoryNode(BoneCategory category)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)0, category.GroupColor, true);
		try
		{
			ImGuiTreeNodeFlags val2 = (ImGuiTreeNodeFlags)2176;
			if (Selected == category)
			{
				val2 = (ImGuiTreeNodeFlags)(val2 | 1);
			}
			if (!CategoryMap.ContainsKey(category.Name))
			{
				val2 = (ImGuiTreeNodeFlags)(val2 | 0x100);
			}
			return ImRaii.TreeNode(ImU8String.op_Implicit(_locale.GetCategoryName(category)), val2);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DrawCategoryInfo()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		ImGui.TableNextColumn();
		if (Selected != null)
		{
			ImGui.Spacing();
			ImGui.Text(ImU8String.op_Implicit(_locale.Translate("config.categories.editor.color_header")));
			ImGui.Spacing();
			DrawCategoryColors(Selected);
			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();
			DrawCategoryOverlayOptions(Selected);
		}
	}

	private void DrawCategoryOverlayOptions(BoneCategory category)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit("Overlay:"));
		ImGui.Spacing();
		ImGui.Checkbox(ImU8String.op_Implicit("Hide group when 'Pose' overlay toggled"), ref category.HideOnPoseEntity);
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(ImU8String.op_Implicit("Bones in this category will not be visible when toggling the 'Pose' overlay visibility of an actor.\nThey will only be visible when you specifically set the category to be visible.\n\nThis option can also instead be assigned per group/bone from the right-click menu of groups/bones listed under an actor in the Workspace."));
		}
	}

	private void DrawCategoryColors(BoneCategory category)
	{
		DrawSwitches(category);
		ImGui.Spacing();
		bool flag = false;
		if (category.LinkedColors)
		{
			flag = DrawColorEdit(_locale.Translate("config.categories.editor.group_color"), ref category.GroupColor);
		}
		else
		{
			flag |= DrawColorEdit(_locale.Translate("config.categories.editor.group_color"), ref category.GroupColor);
			flag |= DrawColorEdit(_locale.Translate("config.categories.editor.bone_color"), ref category.BoneColor);
		}
		if (flag && ColorSub)
		{
			SetColors(category, category);
		}
	}

	private void SetColors(BoneCategory parent, BoneCategory child)
	{
		if (parent != child)
		{
			child.GroupColor = parent.GroupColor;
		}
		child.LinkedColors |= parent.LinkedColors;
		if (!child.LinkedColors)
		{
			child.BoneColor = parent.BoneColor;
		}
		if (CategoryMap.TryGetValue(child.Name, out List<BoneCategory> value))
		{
			value.ForEach(delegate(BoneCategory cat)
			{
				SetColors(parent, cat);
			});
		}
	}

	private void DrawSwitches(BoneCategory category)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (ImGui.Checkbox(ImU8String.op_Implicit(_locale.Translate("config.categories.editor.subcategories")), ref ColorSub) && ColorSub)
		{
			SetColors(category, category);
		}
		ImGui.Checkbox(ImU8String.op_Implicit(_locale.Translate("config.categories.editor.link_colors")), ref category.LinkedColors);
	}

	private static bool DrawColorEdit(string label, ref uint color)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Vector4 vector = ImGui.ColorConvertU32ToFloat4(color);
		bool num = ImGui.ColorEdit4(ImU8String.op_Implicit(label), ref vector, (ImGuiColorEditFlags)0);
		if (num)
		{
			color = ImGui.ColorConvertFloat4ToU32(vector);
		}
		return num;
	}
}
