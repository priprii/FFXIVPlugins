using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImGuizmo;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;
using GLib.Widgets;
using Ktisis.Common.Extensions;
using Ktisis.Common.Utility;
using Ktisis.Core.Attributes;
using Ktisis.Data.Config;
using Ktisis.Data.Config.Entity;
using Ktisis.Editor.Context.Types;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.World;
using Ktisis.Services.Game;
using Ktisis.Structs.Lights;

namespace Ktisis.Interface.Overlay;

[Transient]
public class SelectableGui
{
	private class SelectableFrame : ISelectableFrame
	{
		private readonly List<ItemSelect> Items = new List<ItemSelect>();

		public IEnumerable<IItemSelect> GetItems()
		{
			return Items.AsReadOnly();
		}

		public unsafe void AddItem(SceneEntity entity, Vector3 worldPos, IEditorContext ctx, float opacityMultiplier = 1f)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
			Camera* sceneCamera = CameraService.GetSceneCamera();
			if (sceneCamera == null || !CameraService.WorldToScreen(sceneCamera, worldPos, out var screenPos))
			{
				return;
			}
			float dist = Vector3.Distance(Vector3.op_Implicit(((Object)(&((Camera)sceneCamera).Object)).Position), worldPos);
			ItemSelect item = new ItemSelect(entity, screenPos, dist, opacityMultiplier);
			Items.Add(item);
			if (!(entity is LightEntity lightEntity))
			{
				return;
			}
			SceneLight* ptr = lightEntity.GetObject();
			if (ptr == null || ptr->RenderLight == null || ptr->RenderLight->LightType == LightType.PointLight)
			{
				return;
			}
			float z = Math.Min(ptr->RenderLight->Range, 1f);
			Quaternion? quaternion = lightEntity.GetTransform()?.Rotation;
			if (quaternion.HasValue)
			{
				if (ptr->RenderLight->LightType == LightType.AreaLight)
				{
					quaternion *= (new Vector3(ptr->RenderLight->AreaAngle.X, ptr->RenderLight->AreaAngle.Y, 0f) * MathHelpers.Rad2Deg).EulerAnglesToQuaternion();
				}
				Vector3 vector = Vector3.Transform(new Vector3(0f, 0f, z), quaternion.Value);
				if (CameraService.WorldToScreen(sceneCamera, worldPos + vector, out var screenPos2))
				{
					float alpha = (ImGuizmo.IsUsing() ? ctx.Config.Overlay.LineOpacityUsing : ctx.Config.Overlay.LineOpacity);
					ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
					EntityDisplay entityDisplay = ctx.Config.GetEntityDisplay(lightEntity);
					((ImDrawListPtr)(ref windowDrawList)).AddLine(screenPos, screenPos2, entityDisplay.Color.SetAlpha(alpha), ctx.Config.Overlay.LineThickness);
				}
			}
		}
	}

	private class ItemSelect : IItemSelect
	{
		public readonly int SortPriority;

		public string Name => Entity.Name;

		public SceneEntity Entity { get; }

		public Vector2 ScreenPos { get; }

		public float Distance { get; }

		public float OpacityMultiplier { get; }

		public bool IsHovered { get; set; }

		public ItemSelect(SceneEntity entity, Vector2 screenPos, float dist, float opacityMultiplier)
		{
			Entity = entity;
			ScreenPos = screenPos;
			Distance = dist;
			OpacityMultiplier = opacityMultiplier;
		}
	}

	private readonly ConfigManager _cfg;

	private int ScrollIndex;

	private const int HoverPadding = 6;

	private Configuration Config => _cfg.File;

	public SelectableGui(ConfigManager cfg)
	{
		_cfg = cfg;
	}

	public ISelectableFrame BeginFrame()
	{
		return new SelectableFrame();
	}

	public bool Draw(ISelectableFrame frame, out SceneEntity? clicked, bool gizmo)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		clicked = null;
		if (!Config.Overlay.DrawDotsGizmo && ImGuizmo.IsUsing())
		{
			return false;
		}
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		List<IItemSelect> list = frame.GetItems().ToList();
		List<ImRect> clipRects = WindowOverlaps();
		bool flag = false;
		foreach (IItemSelect item in list)
		{
			EntityDisplay entityDisplay = Config.GetEntityDisplay(item.Entity);
			bool isSelected = item.Entity.IsSelected;
			IItemSelect itemSelect = item;
			itemSelect.IsHovered = entityDisplay.Mode switch
			{
				DisplayMode.Dot => DrawPrimDot(windowDrawList, item.ScreenPos, entityDisplay, item.OpacityMultiplier, isSelected), 
				DisplayMode.Icon => DrawIconDot(windowDrawList, item.ScreenPos, entityDisplay, isSelected), 
				_ => false, 
			};
			if (!CheckPosClip(item.ScreenPos, clipRects))
			{
				flag |= item.IsHovered;
			}
		}
		if (!flag)
		{
			return false;
		}
		list.RemoveAll((IItemSelect item) => !item.IsHovered);
		return DrawSelectWindow(list, out clicked, gizmo);
	}

	private bool DrawSelectWindow(IReadOnlyList<IItemSelect> items, out SceneEntity? clicked, bool gizmo)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		clicked = null;
		if (items.Count == 0 || (gizmo && (ImGuizmo.IsUsing() || ImGuizmo.IsOver())))
		{
			return false;
		}
		bool flag = false;
		try
		{
			ImGui.SetNextWindowPos(ImGui.GetMousePos().AddX(20f));
			ImGui.SetNextWindowSize(-Vector2.One, (ImGuiCond)1);
			flag = ImGui.Begin(ImU8String.op_Implicit("##Hover"), (ImGuiWindowFlags)4139);
			if (flag)
			{
				return DrawSelectList(items, out clicked);
			}
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Error drawing select list:\n{value}");
		}
		finally
		{
			if (flag)
			{
				ImGui.End();
			}
		}
		return false;
	}

	private bool DrawSelectList(IReadOnlyList<IItemSelect> list, out SceneEntity? clicked)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		clicked = null;
		int scrollIndex = ScrollIndex;
		ImGuiIOPtr iO = ImGui.GetIO();
		ScrollIndex = scrollIndex - (int)((ImGuiIOPtr)(ref iO)).MouseWheel;
		if (ScrollIndex >= list.Count)
		{
			ScrollIndex = 0;
		}
		else if (ScrollIndex < 0)
		{
			ScrollIndex = list.Count - 1;
		}
		ImGui.SetNextFrameWantCaptureMouse(true);
		bool flag = ImGui.IsMouseClicked((ImGuiMouseButton)0);
		for (int i = 0; i < list.Count; i++)
		{
			IItemSelect itemSelect = list[i];
			bool flag2 = i == ScrollIndex;
			ImGui.Selectable(ImU8String.op_Implicit(itemSelect.Name), flag2, (ImGuiSelectableFlags)0, default(Vector2));
			if (flag2 && flag)
			{
				clicked = itemSelect.Entity;
			}
		}
		return clicked != null;
	}

	private bool DrawPrimDot(ImDrawListPtr drawList, Vector2 pos2d, EntityDisplay display, float opacityMultiplier, bool isSelect = false)
	{
		float num = Config.Overlay.DotRadius;
		if (isSelect)
		{
			num += 1f;
		}
		byte b = (byte)((display.Color & 0xFF000000u) >> 24);
		float alpha = ((b == 0) ? opacityMultiplier : ((float)(int)b / 255f * opacityMultiplier));
		uint num2 = display.Color.SetAlpha(alpha);
		((ImDrawListPtr)(ref drawList)).AddCircleFilled(pos2d, num, num2, 16);
		((ImDrawListPtr)(ref drawList)).AddCircle(pos2d, num, 4278190080u.SetAlpha(alpha), 16, isSelect ? 2.5f : 1f);
		return IsHovering(pos2d, num);
	}

	private bool DrawIconDot(ImDrawListPtr drawList, Vector2 pos2d, EntityDisplay display, bool isSelect = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Vector2 vector = Icons.CalcIconSize(display.Icon);
		float num = UiBuilder.DefaultFontSizePx * ImGuiHelpers.GlobalScale;
		bool flag = IsHovering(pos2d, num);
		((ImDrawListPtr)(ref drawList)).AddCircleFilled(pos2d, num, isSelect ? 2936012800u : (flag ? 3388997632u : 1879048192u), 16);
		if (isSelect)
		{
			((ImDrawListPtr)(ref drawList)).AddCircle(pos2d, num, 4293914607u, 16, 1.5f);
		}
		ImGui.SetCursorPos(pos2d - vector / 2f);
		Icons.DrawIcon(display.Icon, display.Color);
		return flag;
	}

	private static bool IsHovering(Vector2 pos2d, float radius)
	{
		return ImGui.IsMouseHoveringRect(pos2d.Add(0f - radius - 6f), pos2d.Add(radius + 6f));
	}

	public static List<ImRect> WindowOverlaps()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		List<ImRect> list = new List<ImRect>();
		ImGuiContextPtr currentContext = ImGui.GetCurrentContext();
		foreach (ImGuiWindowPtr item in ((IEnumerable<ImGuiWindowPtr>)(object)((ImGuiContextPtr)(ref currentContext)).Windows).Where((ImGuiWindowPtr w) => ((ImGuiWindowPtr)(ref w)).WasActive))
		{
			ImGuiWindowPtr current = item;
			if (((ImGuiWindowPtr)(ref current)).Pos != Vector2.Zero)
			{
				list.Add(new ImRect(((ImGuiWindowPtr)(ref current)).Pos, ((ImGuiWindowPtr)(ref current)).Size + ((ImGuiWindowPtr)(ref current)).Pos));
			}
		}
		return list;
	}

	public static bool CheckPosClip(Vector2 position, List<ImRect> clipRects)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		foreach (ImRect item in clipRects.Where((ImRect w) => w.Min != Vector2.Zero))
		{
			ImRect current = item;
			if (ImGuiP.Contains(ref current, position))
			{
				return true;
			}
		}
		return false;
	}
}
