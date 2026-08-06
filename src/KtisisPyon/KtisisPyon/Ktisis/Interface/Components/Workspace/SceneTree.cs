using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using GLib.Widgets;
using Ktisis.Common.Extensions;
using Ktisis.Common.Utility;
using Ktisis.Data.Config.Bones;
using Ktisis.Data.Config.Entity;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Posing.Types;
using Ktisis.Editor.Selection;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Scene.Entities.World;

namespace Ktisis.Interface.Components.Workspace;

public class SceneTree
{
	private enum TreeNodeFlag
	{
		Leaf,
		Expand,
		Collapse
	}

	private readonly IEditorContext _ctx;

	private readonly SceneDragDropHandler _dragDrop;

	private List<SceneEntity> _nodes;

	private SceneEntity? _shiftNode;

	private int? _originIndex;

	private float MinY;

	private float MaxY;

	private static float IconSpacing => UiBuilder.DefaultFontSizePx * ImGuiHelpers.GlobalScale;

	public SceneTree(IEditorContext ctx)
	{
		_ctx = ctx;
		_dragDrop = new SceneDragDropHandler(ctx);
		_nodes = new List<SceneEntity>();
		_shiftNode = null;
		_originIndex = null;
	}

	public void Draw(float height)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			uint iD = ImGui.GetID(ImU8String.op_Implicit("SceneTree_Frame"));
			ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3f);
			ChildFrameDisposable val = ImRaii.ChildFrame(iD, new Vector2(ImGui.GetContentRegionAvail().X - 3f, height));
			try
			{
				DrawScene(height);
			}
			finally
			{
				((ChildFrameDisposable)(ref val)).Dispose();
			}
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Error drawing scene tree: {value}");
		}
	}

	private void PreCalc(float height)
	{
		float scrollY = ImGui.GetScrollY();
		MinY = scrollY - ImGui.GetFrameHeight();
		MaxY = height + scrollY;
	}

	private void DrawScene(float height)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		_nodes.Clear();
		_shiftNode = null;
		PreCalc(height);
		ImGuiStylePtr style = ImGui.GetStyle();
		Vector2 itemSpacing = ((ImGuiStylePtr)(ref style)).ItemSpacing;
		Vector2 vector = itemSpacing;
		vector.Y = 5f;
		StyleDisposable val = ImRaii.PushStyle((ImGuiStyleVar)13, vector, true);
		try
		{
			IterateTree(_ctx.Scene.Children);
			if (_shiftNode != null)
			{
				ResolveShiftSelect();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void IterateTree(IEnumerable<SceneEntity> entities)
	{
		try
		{
			ImGui.TreePush((IntPtr)IntPtr.Zero);
			foreach (SceneEntity entity in entities)
			{
				_nodes.Add(entity);
				DrawNode(entity, out var shiftClicked);
				if (shiftClicked)
				{
					_shiftNode = entity;
				}
			}
		}
		finally
		{
			ImGui.TreePop();
		}
	}

	private void DrawNode(SceneEntity node, out bool shiftClicked)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		shiftClicked = false;
		Vector2 cursorPos = ImGui.GetCursorPos();
		bool num = cursorPos.Y > MinY && cursorPos.Y < MaxY;
		string obj = $"##SceneTree_{node.GetHashCode():X}";
		ImGui.Selectable(ImU8String.op_Implicit(obj), node.IsSelected, (ImGuiSelectableFlags)18, default(Vector2));
		bool flag = ImGui.IsWindowHovered();
		Vector2 itemRectSize = ImGui.GetItemRectSize();
		_dragDrop.Handle(node);
		uint iD = ImGui.GetID(ImU8String.op_Implicit(obj));
		ImGuiStoragePtr stateStorage = ImGui.GetStateStorage();
		bool flag2 = ((ImGuiStoragePtr)(ref stateStorage)).GetBool(iD);
		List<SceneEntity> list = node.Children.ToList();
		if (num)
		{
			bool flag3 = flag2;
			TreeNodeFlag treeNodeFlag = ((list.Count != 0) ? ((!(node is EntityPose)) ? (flag3 ? TreeNodeFlag.Expand : TreeNodeFlag.Collapse) : TreeNodeFlag.Leaf) : TreeNodeFlag.Leaf);
			TreeNodeFlag flag4 = treeNodeFlag;
			float rightAdjust = DrawButtons(node, flag);
			if (DrawNodeLabel(node, cursorPos, flag4, rightAdjust))
			{
				((ImGuiStoragePtr)(ref stateStorage)).SetBool(iD, flag2 = !flag2);
			}
			ImGuiIOPtr iO = ImGui.GetIO();
			if (flag && IsNodeHovered(cursorPos, itemRectSize, rightAdjust))
			{
				if (ImGui.IsMouseClicked((ImGuiMouseButton)2))
				{
					_ctx.Interface.OpenEditorFor(node);
				}
				else if (((ImGuiIOPtr)(ref iO)).MouseReleased[0] && ((ImGuiIOPtr)(ref iO)).MouseDownDurationPrev[0] < 0.5f)
				{
					if (ImGui.IsKeyDown((ImGuiKey)642))
					{
						shiftClicked = true;
					}
					else
					{
						SelectMode selectMode = GuiHelpers.GetSelectMode();
						node.Select(selectMode);
					}
				}
				else if (ImGui.IsMouseClicked((ImGuiMouseButton)1))
				{
					_ctx.Interface.OpenSceneEntityMenu(node);
				}
				if (((ImGuiIOPtr)(ref iO)).MouseReleased[0] && ((ImGuiIOPtr)(ref iO)).MouseDownDurationPrev[0] < 0.5f && !shiftClicked)
				{
					_originIndex = _nodes.Count - 1;
				}
			}
		}
		if (flag2 || node is EntityPose)
		{
			IterateTree(list);
		}
	}

	private bool DrawNodeLabel(SceneEntity item, Vector2 pos, TreeNodeFlag flag, float rightAdjust = 0f)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		EntityDisplay entityDisplay = _ctx.Config.GetEntityDisplay(item);
		bool result = false;
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SameLine();
		ImGui.SetCursorPosX(pos.X - ((ImGuiStylePtr)(ref style)).ItemSpacing.X);
		if (!(item is EntityPose))
		{
			result = DrawNodeCaret(entityDisplay.Color, flag);
		}
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)0, entityDisplay.Color, true);
		try
		{
			if ((int)entityDisplay.Icon != 0)
			{
				DrawNodeIcon(entityDisplay.Icon);
			}
			float x = ImGui.GetContentRegionAvail().X;
			ImGui.Text(ImU8String.op_Implicit(item.Name.FitToWidth(x - rightAdjust)));
			return result;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private bool DrawNodeCaret(uint color, TreeNodeFlag flag)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		float cursorPosX = ImGui.GetCursorPosX();
		FontAwesomeIcon val = (FontAwesomeIcon)(flag switch
		{
			TreeNodeFlag.Collapse => 61658, 
			TreeNodeFlag.Expand => 61655, 
			_ => 0, 
		});
		if ((int)val != 0)
		{
			ColorDisposable val2 = ImRaii.PushColor((ImGuiCol)0, color.SetAlpha(207), true);
			try
			{
				Icons.DrawIcon(val);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		ImGui.SameLine();
		ImGuiStylePtr style = ImGui.GetStyle();
		Vector2 itemInnerSpacing = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing;
		cursorPosX += itemInnerSpacing.X + IconSpacing;
		ImGui.SetCursorPosX(cursorPosX);
		return ButtonsEx.IsClicked();
	}

	private void DrawNodeIcon(FontAwesomeIcon icon)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		bool num = (int)icon > 0;
		float num2 = (num ? (Icons.CalcIconSize(icon).X / 2f) : 0f);
		float num3 = (num ? IconSpacing : 0f);
		Icons.DrawIcon(icon);
		ImGui.SameLine(0f, num3 - num2);
	}

	private float DrawButtons(SceneEntity node, bool isHover)
	{
		float cursor;
		float num = (cursor = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X);
		DrawVisibilityButton(node, ref cursor, isHover);
		if (node is IAttachable attach)
		{
			DrawAttachButton(attach, ref cursor, isHover);
		}
		if (node is IHideable entity)
		{
			DrawHideButton(entity, ref cursor, isHover);
		}
		return num - cursor;
	}

	private void DrawVisibilityButton(SceneEntity node, ref float cursor, bool isHover)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		if (!(node is IVisibility visibility))
		{
			return;
		}
		bool visible = _ctx.Config.Overlay.Visible;
		bool visible2 = visibility.Visible;
		uint num = (visible2 ? 4026531839u : 2164260863u);
		if (!visible)
		{
			num = num.SetAlpha((byte)(visible2 ? 96u : 48u));
		}
		FontAwesomeIcon icon = (FontAwesomeIcon)((visibility is WorldEntity) ? 62977 : 61550);
		if (DrawButton(ref cursor, icon, num) && isHover)
		{
			HandleVisibilityToggle(node, visibility);
		}
		if (!isHover || !ImGui.IsItemHovered())
		{
			return;
		}
		TooltipDisposable val = ImRaii.Tooltip();
		try
		{
			string text = ((visibility is WorldEntity) ? (node.Type.ToString() + " Root") : ((visibility is BoneNode) ? "Bone" : ((visibility is EntityPose) ? "Skeleton" : ((!(visibility is SkeletonGroup)) ? "Overlay" : "Bones"))));
			string text2 = text;
			ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("common." + (visibility.Visible ? "hide" : "show")) + " " + text2));
		}
		finally
		{
			((TooltipDisposable)(ref val)).Dispose();
		}
	}

	private void HandleVisibilityToggle(SceneEntity node, IVisibility vis)
	{
		if (!(node is EntityPose entityPose))
		{
			vis.Toggle();
			return;
		}
		entityPose.OverlayVisible = !entityPose.OverlayVisible;
		foreach (SceneEntity item in entityPose.Recurse())
		{
			BoneNode boneNode = item as BoneNode;
			if (boneNode == null || !(item is IVisibility visibility))
			{
				continue;
			}
			visibility.Visible = entityPose.OverlayVisible;
			if (!visibility.Visible || !(boneNode.Parent is BoneNodeGroup boneNodeGroup))
			{
				continue;
			}
			BoneCategory? category = boneNodeGroup.Category;
			if (category != null && category.HideOnPoseEntity)
			{
				visibility.Visible = false;
				continue;
			}
			CategoryBone? obj = boneNodeGroup.Category?.Bones.FirstOrDefault((CategoryBone x) => x.Name == boneNode.Info.Name);
			if (obj != null && obj.HideOnPoseEntity)
			{
				visibility.Visible = false;
			}
		}
	}

	private void DrawAttachButton(IAttachable attach, ref float cursor, bool isHover)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		if (!attach.IsAttached())
		{
			return;
		}
		if (DrawButton(ref cursor, (FontAwesomeIcon)61633, uint.MaxValue) && isHover)
		{
			_ctx.Posing.Attachments.Detach(attach);
		}
		if (!isHover || !ImGui.IsItemHovered())
		{
			return;
		}
		PartialBoneInfo parentBone = attach.GetParentBone();
		string text = ((parentBone != null) ? _ctx.Locale.GetBoneName(parentBone) : Ktisis.Locale.Translate("common.unknown"));
		TooltipDisposable val = ImRaii.Tooltip();
		try
		{
			ImU8String val2 = new ImU8String(1, 2);
			((ImU8String)(ref val2)).AppendFormatted<string>(Ktisis.Locale.Translate("workspace.scene_tree.attached_to"));
			((ImU8String)(ref val2)).AppendLiteral(" ");
			((ImU8String)(ref val2)).AppendFormatted<string>(text);
			ImGui.Text(val2);
			ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("workspace.scene_tree.reset_tooltip")));
		}
		finally
		{
			((TooltipDisposable)(ref val)).Dispose();
		}
	}

	private void DrawHideButton(IHideable entity, ref float cursor, bool isHover)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		uint value = (entity.IsHidden ? 2164260863u : 4026531839u);
		if (DrawButton(ref cursor, (FontAwesomeIcon)63226, value) && isHover)
		{
			entity.ToggleHidden();
		}
		if (!isHover || !ImGui.IsItemHovered())
		{
			return;
		}
		TooltipDisposable val = ImRaii.Tooltip();
		try
		{
			string text = ((entity is SceneEntity { Type: var type }) ? type.ToString() : Ktisis.Locale.Translate("common.entity"));
			ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("common." + (entity.IsHidden ? "hide" : "show")) + " " + text));
		}
		finally
		{
			((TooltipDisposable)(ref val)).Dispose();
		}
	}

	private bool DrawButton(ref float cursor, FontAwesomeIcon icon, uint? color = null)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		float num = cursor;
		float num2 = Icons.CalcIconSize(icon).X / ImGuiHelpers.GlobalScale;
		ImGuiStylePtr style = ImGui.GetStyle();
		cursor = num - (num2 + ((ImGuiStylePtr)(ref style)).ItemSpacing.X);
		ImGui.SameLine();
		ImGui.SetCursorPosX(cursor);
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)0, color.GetValueOrDefault(), color.HasValue);
		try
		{
			Icons.DrawIcon(icon);
			return ButtonsEx.IsClicked();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private bool IsNodeHovered(Vector2 pos, Vector2 size, float rightAdjust)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemSpacing.X;
		Vector2 vector = ImGui.GetWindowPos() + pos.AddX(x).SubY(ImGui.GetScrollY() + 2f);
		Vector2 vector2 = vector.Add(size.X - pos.X - x - rightAdjust, size.Y);
		return ImGui.IsMouseHoveringRect(vector, vector2);
	}

	private void ResolveShiftSelect()
	{
		int num = _nodes.IndexOf(_shiftNode);
		if (num < 0 || !_originIndex.HasValue || _originIndex == num)
		{
			return;
		}
		if (_originIndex >= _nodes.Count)
		{
			_originIndex = _nodes.Count - 1;
		}
		if (_originIndex > num)
		{
			for (int i = num; i < _originIndex; i++)
			{
				SceneEntity sceneEntity = _nodes[i];
				if (sceneEntity != null && !sceneEntity.IsSelected)
				{
					sceneEntity.Select(SelectMode.Multiple);
				}
			}
		}
		else
		{
			for (int num2 = num; num2 > _originIndex; num2--)
			{
				SceneEntity sceneEntity2 = _nodes[num2];
				if (sceneEntity2 != null && !sceneEntity2.IsSelected)
				{
					sceneEntity2.Select(SelectMode.Multiple);
				}
			}
		}
		_originIndex = num;
	}
}
