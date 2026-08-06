using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImGuizmo;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using Ktisis.Common.Extensions;
using Ktisis.Common.Utility;
using Ktisis.Core.Attributes;
using Ktisis.Data.Config.Entity;
using Ktisis.Data.Config.Sections;
using Ktisis.Editor.Camera.Types;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Selection;
using Ktisis.Interface.Editor.Popup;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Scene.Entities.Utility;
using Ktisis.Scene.Entities.World;
using Ktisis.Scene.Modules.Actors;
using Ktisis.Scene.Modules.Lights;
using Ktisis.Services.Game;
using Ktisis.Structs.Lights;
using Ktisis.Structs.Objects;

namespace Ktisis.Interface.Overlay;

[Transient]
public class SceneDraw
{
	private readonly SelectableGui _select;

	private readonly RefOverlay _refs;

	private WorldObject? _hovered;

	private IGameObject? _hoveredActor;

	private bool _isHoveringWorld;

	private bool _isHoveringActor;

	private bool _isHoveringLight;

	private IEditorContext _ctx;

	private readonly GuiManager _gui;

	private WorldObjectPopup? _popup;

	private ActorService _actors;

	private readonly GPoseService _gpose;

	private OverlayConfig Config => _ctx.Config.Overlay;

	public SceneDraw(SelectableGui select, RefOverlay refs, GuiManager gui, ActorService actors, GPoseService gpose)
	{
		_select = select;
		_refs = refs;
		_gui = gui;
		_actors = actors;
		_gpose = gpose;
	}

	public void SetContext(IEditorContext ctx)
	{
		_ctx = ctx;
	}

	public void DrawScene(bool gizmo = false, bool gizmoIsEnded = false)
	{
		ISelectableFrame frame = _select.BeginFrame();
		DrawEntities(frame, _ctx.Scene.Children);
		DrawSelect(frame, gizmo, gizmoIsEnded);
		if (_ctx.ShowWorldObjects)
		{
			_isHoveringWorld = false;
			_isHoveringActor = false;
			_isHoveringLight = false;
			DrawWorldObjects();
			DrawWorldActors();
			DrawWorldLights();
			if (!_isHoveringWorld)
			{
				SetHovered(null);
			}
			if (!_isHoveringActor)
			{
				SetHoveredActor(null);
			}
		}
	}

	public void DrawRefOverlay()
	{
		foreach (ReferenceImage item in _ctx.Scene.Children.OfType<ReferenceImage>())
		{
			_refs.DrawInstance(item);
		}
	}

	private void DrawEntities(ISelectableFrame frame, IEnumerable<SceneEntity> entities, float opacity = 1f)
	{
		foreach (SceneEntity entity in entities)
		{
			if (!(entity is EntityPose pose))
			{
				if (entity is IVisibility { Visible: not false } visibility && visibility is ITransform transform)
				{
					Vector3? vector = transform.GetTransform()?.Position;
					if (vector.HasValue)
					{
						frame.AddItem(entity, vector.Value, _ctx, opacity);
					}
				}
				if (entity is ActorEntity actor)
				{
					DrawEntities(frame, entity.Children, GetOpacityMultiplier(actor));
				}
				else
				{
					DrawEntities(frame, entity.Children);
				}
			}
			else
			{
				DrawSkeleton(frame, pose);
			}
		}
	}

	private unsafe void DrawSkeleton(ISelectableFrame frame, EntityPose pose)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		if (!pose.ShouldDraw() && !Config.BulkVisOverride)
		{
			return;
		}
		Skeleton* skeleton = pose.GetSkeleton();
		if (skeleton == null || ((Skeleton)skeleton).PartialSkeletons == null)
		{
			return;
		}
		Camera* sceneCamera = CameraService.GetSceneCamera();
		if (sceneCamera == null)
		{
			return;
		}
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		List<BoneNode> list = ((Config.DrawLines && (Config.ColorSelectedBoneParentLine || Config.ColorSelectedBoneDescendantLine) && (!Config.DrawLinesGizmo || !ImGuizmo.IsUsing())) ? (from x in pose.GetAllBones()
			where x.IsSelected
			select x).ToList() : null);
		HashSet<BoneNode> hashSet = null;
		if (list != null && Config.ColorSelectedBoneDescendantLine)
		{
			hashSet = new HashSet<BoneNode>();
			foreach (BoneNode item in list)
			{
				foreach (BoneNode allBone in pose.GetAllBones())
				{
					if (allBone.IsBoneDescendantOf(item))
					{
						hashSet.Add(allBone);
					}
				}
			}
		}
		float? opacityMultiplier = null;
		if (pose.Parent is ActorEntity actor)
		{
			opacityMultiplier = GetOpacityMultiplier(actor);
		}
		ushort partialSkeletonCount = ((Skeleton)skeleton).PartialSkeletonCount;
		for (int num = 0; num < partialSkeletonCount; num++)
		{
			PartialSkeleton val = ((PartialSkeleton*)((Skeleton)skeleton).PartialSkeletons)[num];
			hkaPose* havokPose = ((PartialSkeleton)(ref val)).GetHavokPose(0);
			if (havokPose == null || ((hkaPose)havokPose).Skeleton == null)
			{
				continue;
			}
			hkaSkeleton* skeleton2 = ((hkaPose)havokPose).Skeleton;
			int length = ((hkaSkeleton)skeleton2).Bones.Length;
			for (int num2 = 0; num2 < length; num2++)
			{
				BoneNode boneFromMap = pose.GetBoneFromMap(num, num2);
				if ((boneFromMap == null || !boneFromMap.Visible) && !Config.BulkVisOverride)
				{
					continue;
				}
				Transform transform = boneFromMap?.CalcTransformOverlay();
				if (transform == null || boneFromMap == null)
				{
					continue;
				}
				if (opacityMultiplier.HasValue)
				{
					frame.AddItem(boneFromMap, transform.Position, _ctx, opacityMultiplier.Value);
				}
				else
				{
					frame.AddItem(boneFromMap, transform.Position, _ctx);
				}
				if (!Config.DrawLines || (!Config.DrawLinesGizmo && ImGuizmo.IsUsing()))
				{
					continue;
				}
				for (int num3 = num2; num3 < length; num3++)
				{
					if (((hkaSkeleton)skeleton2).ParentIndices[num3] != num2)
					{
						continue;
					}
					BoneNode boneFromMap2 = pose.GetBoneFromMap(num, num3);
					if ((boneFromMap2 != null && boneFromMap2.Visible) || Config.BulkVisOverride)
					{
						Transform transform2 = boneFromMap2?.CalcTransformOverlay();
						if (transform2 != null)
						{
							EntityDisplay entityDisplay = _ctx.Config.GetEntityDisplay(boneFromMap);
							DrawLine(sceneCamera, windowDrawList, transform.Position, transform2.Position, GetBoneLineColor(boneFromMap2, list, hashSet, entityDisplay), opacityMultiplier);
						}
					}
				}
			}
		}
	}

	private uint GetBoneLineColor(BoneNode? bone, List<BoneNode>? selectedBones, HashSet<BoneNode>? descendantSet, EntityDisplay display)
	{
		if (selectedBones == null)
		{
			if (display.Color == uint.MaxValue)
			{
				return Config.DefaultLineColor;
			}
			return display.Color;
		}
		if (bone != null && bone.IsSelected)
		{
			if (!Config.ColorSelectedBoneParentLine)
			{
				return display.Color;
			}
			return Config.SelectedBoneParentLineColor;
		}
		if (bone != null && descendantSet != null && descendantSet.Contains(bone))
		{
			return Config.SelectedBoneDescendantLineColor;
		}
		if (display.Color == uint.MaxValue)
		{
			return Config.DefaultLineColor;
		}
		return display.Color;
	}

	private unsafe void DrawLine(Camera* camera, ImDrawListPtr drawList, Vector3 fromPos, Vector3 toPos, uint color, float? opacityMultiplier)
	{
		if (CameraService.WorldToScreen(camera, fromPos, out var screenPos) && CameraService.WorldToScreen(camera, toPos, out var screenPos2))
		{
			float num = (ImGuizmo.IsUsing() ? Config.LineOpacityUsing : Config.LineOpacity);
			if (opacityMultiplier.HasValue)
			{
				num *= opacityMultiplier.Value;
			}
			((ImDrawListPtr)(ref drawList)).AddLine(screenPos, screenPos2, color.SetAlpha(num), Config.LineThickness);
		}
	}

	private void DrawSelect(ISelectableFrame frame, bool gizmo, bool gizmoIsEnded)
	{
		if (_select.Draw(frame, out SceneEntity clicked, gizmo) && clicked != null && !(gizmo && gizmoIsEnded))
		{
			SelectMode selectMode = GuiHelpers.GetSelectMode();
			_ctx.Selection.Select(clicked, selectMode);
		}
	}

	private unsafe void DrawWorldObjects()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		ImDrawListPtr backgroundDrawList = ImGui.GetBackgroundDrawList();
		Camera* sceneCamera = CameraService.GetSceneCamera();
		List<ImRect> clipRects = SelectableGui.WindowOverlaps();
		if (sceneCamera == null)
		{
			return;
		}
		foreach (WorldObject obj in _ctx.Scene.World.Objects)
		{
			if (_ctx.Scene.Children.OfType<ObjectEntity>().Any((ObjectEntity ent) => ent.Object.Equals(obj)) || !CameraService.WorldToScreen(sceneCamera, obj.InitialTransform.Position, out var screenPos))
			{
				continue;
			}
			float num = ObjectDistance(new Vector2(obj.InitialTransform.Position.X, obj.InitialTransform.Position.Z));
			if (num > Config.WorldCameraRange)
			{
				continue;
			}
			float num2 = float.Lerp(1f, Config.WorldNodeScaleFactor, num / Config.WorldCameraRange);
			((ImDrawListPtr)(ref backgroundDrawList)).AddNgonFilled(screenPos, (Config.WorldNodeRadius + Config.WorldNodeOutlineWidth - 1f) * num2, Config.WorldNodeColor, 4);
			if (Config.WorldNodeOutlineWidth > 0f)
			{
				((ImDrawListPtr)(ref backgroundDrawList)).AddNgon(screenPos, (Config.WorldNodeRadius + Config.WorldNodeOutlineWidth / 2f) * num2, 4278190080u, 4, Config.WorldNodeOutlineWidth);
			}
			float num3 = (6f + Config.WorldNodeRadius + Config.WorldNodeOutlineWidth / 2f) * num2;
			Vector2 vector = new Vector2(num3, num3);
			if (_isHoveringWorld || SelectableGui.CheckPosClip(screenPos, clipRects) || !ImGui.IsMouseHoveringRect(screenPos - vector, screenPos + vector))
			{
				continue;
			}
			WorldObjectPopup popup = _popup;
			if (popup != null && popup.IsOpen && _popup.WorldObj.Equals(obj))
			{
				continue;
			}
			_isHoveringWorld = true;
			SetHovered(obj);
			TooltipDisposable val = ImRaii.Tooltip();
			try
			{
				ColorDisposable val2 = ImRaii.PushColor((ImGuiCol)0, Config.WorldNodeColor, true);
				try
				{
					ImU8String val3 = new ImU8String(17, 0);
					((ImU8String)(ref val3)).AppendLiteral("Object Details...");
					ImGui.Text(val3);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			finally
			{
				((TooltipDisposable)(ref val)).Dispose();
			}
			ImGui.SetNextFrameWantCaptureMouse(true);
			if (ImGui.IsMouseClicked((ImGuiMouseButton)0))
			{
				_popup = _gui.CreatePopup<WorldObjectPopup>(new object[3] { obj, num, _ctx });
				_popup.Open();
			}
		}
	}

	private unsafe void DrawWorldActors()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		ImDrawListPtr backgroundDrawList = ImGui.GetBackgroundDrawList();
		Camera* sceneCamera = CameraService.GetSceneCamera();
		List<ImRect> clipRects = SelectableGui.WindowOverlaps();
		if (sceneCamera == null)
		{
			return;
		}
		foreach (IGameObject overworldActor in _actors.GetOverworldActors())
		{
			if (_ctx.Scene.Children.OfType<ActorEntity>().Any((ActorEntity ent) => ent.Actor.ObjectIndex == overworldActor.ObjectIndex) || !CameraService.WorldToScreen(sceneCamera, overworldActor.Position, out var screenPos))
			{
				continue;
			}
			float num = ObjectDistance(new Vector2(overworldActor.Position.X, overworldActor.Position.Z));
			if (num > Config.WorldCameraRange)
			{
				continue;
			}
			float num2 = float.Lerp(1f, Config.WorldNodeScaleFactor, num / Config.WorldCameraRange);
			((ImDrawListPtr)(ref backgroundDrawList)).AddNgonFilled(screenPos, (Config.WorldNodeRadius + Config.WorldNodeOutlineWidth - 1f) * num2, Config.ActorNodeColor, 5);
			if (Config.WorldNodeOutlineWidth > 0f)
			{
				((ImDrawListPtr)(ref backgroundDrawList)).AddNgon(screenPos, (Config.WorldNodeRadius + Config.WorldNodeOutlineWidth / 2f) * num2, 4278190080u, 5, Config.WorldNodeOutlineWidth);
			}
			float num3 = (6f + Config.WorldNodeRadius + Config.WorldNodeOutlineWidth / 2f) * num2;
			Vector2 vector = new Vector2(num3, num3);
			if (_isHoveringWorld || _isHoveringActor || SelectableGui.CheckPosClip(screenPos, clipRects) || !ImGui.IsMouseHoveringRect(screenPos - vector, screenPos + vector))
			{
				continue;
			}
			WorldObjectPopup popup = _popup;
			if (popup != null && popup.IsOpen)
			{
				continue;
			}
			_isHoveringActor = true;
			SetHoveredActor(overworldActor);
			TooltipDisposable val = ImRaii.Tooltip();
			try
			{
				ColorDisposable val2 = ImRaii.PushColor((ImGuiCol)0, Config.WorldNodeColor, true);
				try
				{
					string text = (StringExtensions.IsNullOrEmpty(overworldActor.Name.TextValue) ? $"{overworldActor.ObjectIndex}" : $"{overworldActor.Name} ({overworldActor.ObjectIndex})");
					ImU8String val3 = new ImU8String(10, 1);
					((ImU8String)(ref val3)).AppendLiteral("Add Actor ");
					((ImU8String)(ref val3)).AppendFormatted<string>(text);
					ImGui.Text(val3);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			finally
			{
				((TooltipDisposable)(ref val)).Dispose();
			}
			ImGui.SetNextFrameWantCaptureMouse(true);
			if (ImGui.IsMouseClicked((ImGuiMouseButton)0))
			{
				_ctx.Scene.GetModule<ActorModule>().AddFromOverworld(overworldActor);
			}
		}
	}

	private unsafe void DrawWorldLights()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		ImDrawListPtr backgroundDrawList = ImGui.GetBackgroundDrawList();
		Camera* sceneCamera = CameraService.GetSceneCamera();
		List<ImRect> clipRects = SelectableGui.WindowOverlaps();
		if (sceneCamera == null)
		{
			return;
		}
		foreach (WorldObject light in _ctx.Scene.World.Lights)
		{
			if (_ctx.Scene.Children.OfType<LightEntity>().Any((LightEntity ent) => ent.WorldLight.Equals(light)) || !CameraService.WorldToScreen(sceneCamera, light.InitialTransform.Position, out var screenPos))
			{
				continue;
			}
			float num = ObjectDistance(new Vector2(light.InitialTransform.Position.X, light.InitialTransform.Position.Z));
			if (num > Config.WorldCameraRange)
			{
				continue;
			}
			SceneLight* address = (SceneLight*)light.Address;
			if (address == null || !((DrawObject)(&address->DrawObject)).IsVisible)
			{
				continue;
			}
			float num2 = float.Lerp(1f, Config.WorldNodeScaleFactor, num / Config.WorldCameraRange);
			((ImDrawListPtr)(ref backgroundDrawList)).AddNgonFilled(screenPos, (Config.WorldNodeRadius + Config.WorldNodeOutlineWidth - 1f) * num2, Config.LightNodeColor, 3);
			if (Config.WorldNodeOutlineWidth > 0f)
			{
				((ImDrawListPtr)(ref backgroundDrawList)).AddNgon(screenPos, (Config.WorldNodeRadius + Config.WorldNodeOutlineWidth / 2f) * num2, 4278190080u, 3, Config.WorldNodeOutlineWidth);
			}
			float num3 = (6f + Config.WorldNodeRadius + Config.WorldNodeOutlineWidth / 2f) * num2;
			Vector2 vector = new Vector2(num3, num3);
			if (_isHoveringWorld || _isHoveringActor || _isHoveringLight || SelectableGui.CheckPosClip(screenPos, clipRects) || !ImGui.IsMouseHoveringRect(screenPos - vector, screenPos + vector))
			{
				continue;
			}
			WorldObjectPopup popup = _popup;
			if (popup != null && popup.IsOpen)
			{
				continue;
			}
			_isHoveringLight = true;
			TooltipDisposable val = ImRaii.Tooltip();
			try
			{
				ColorDisposable val2 = ImRaii.PushColor((ImGuiCol)0, Config.WorldNodeColor, true);
				try
				{
					ImU8String val3 = new ImU8String(15, 0);
					((ImU8String)(ref val3)).AppendLiteral("Add World Light");
					ImGui.Text(val3);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			finally
			{
				((TooltipDisposable)(ref val)).Dispose();
			}
			ImGui.SetNextFrameWantCaptureMouse(true);
			if (ImGui.IsMouseClicked((ImGuiMouseButton)0))
			{
				_ctx.Scene.GetModule<LightModule>().AddFromOverworld(light);
			}
		}
	}

	private unsafe float ObjectDistance(Vector2 xzPosition)
	{
		Vector2 value = default(Vector2);
		EditorCamera current = _ctx.Cameras.Current;
		if (current is WorkCamera workCamera)
		{
			value.X = workCamera.Position.X;
			value.Y = workCamera.Position.Z;
		}
		else if (current != null)
		{
			value.X = current.Camera->Position.X;
			value.Y = current.Camera->Position.Z;
		}
		return Vector2.Distance(value, xzPosition);
	}

	private void SetHovered(WorldObject? obj)
	{
		if (!obj.Equals(_hovered))
		{
			_hovered?.SetOutline(OutlineChoice.None);
			_hovered = obj;
			_hovered?.SetOutline(Config.WorldOutlineColor);
		}
	}

	private void SetHoveredActor(IGameObject? actor)
	{
		if (actor == null || !((IEquatable<IGameObject>)actor).Equals(_hoveredActor))
		{
			SetActorHighlight(highlightOn: false);
			_hoveredActor = actor;
			SetActorHighlight(highlightOn: true);
		}
	}

	private static ObjectHighlightColor GetHighlightColor(OutlineChoice choice)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		return (ObjectHighlightColor)(choice switch
		{
			OutlineChoice.None => 0, 
			OutlineChoice.Red => 1, 
			OutlineChoice.Green => 2, 
			OutlineChoice.Blue => 3, 
			OutlineChoice.Yellow => 4, 
			OutlineChoice.Orange => 5, 
			OutlineChoice.Pink => 6, 
			_ => 0, 
		});
	}

	private unsafe void SetActorHighlight(bool highlightOn)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (_hoveredActor == null)
		{
			return;
		}
		GameObject* address = (GameObject*)_hoveredActor.Address;
		if (address != null && ((GameObject)address).DrawObject != null)
		{
			if (highlightOn)
			{
				((GameObject)address).Highlight(GetHighlightColor(Config.WorldOutlineColor), true);
			}
			else
			{
				((GameObject)address).Highlight((ObjectHighlightColor)0, true);
			}
		}
	}

	private float GetOpacityMultiplier(ActorEntity actor)
	{
		if (!Config.DimOverlayForInactiveActors)
		{
			return 1f;
		}
		ActiveState activeStateType = Config.ActiveStateType;
		if ((activeStateType == ActiveState.Target || activeStateType == ActiveState.Both) ? true : false)
		{
			IGameObject? gPoseTarget = _gpose.GPoseTarget;
			if (((gPoseTarget != null) ? new ushort?(gPoseTarget.ObjectIndex) : ((ushort?)null)) == actor.Actor.ObjectIndex)
			{
				return 1f;
			}
		}
		else
		{
			activeStateType = Config.ActiveStateType;
			bool flag = (uint)(activeStateType - 1) <= 1u;
			if (flag && (actor.IsSelected || actor.Recurse().Any((SceneEntity x) => x.IsSelected)))
			{
				return 1f;
			}
		}
		return Config.InactiveOpacity;
	}
}
