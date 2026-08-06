using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Common.Math;
using GLib.Widgets;
using Ktisis.Common.Extensions;
using Ktisis.Common.Utility;
using Ktisis.Data.Config;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Posing.Ik.TwoJoints;
using Ktisis.Editor.Posing.Ik.Types;
using Ktisis.Interface.Components.Transforms;
using Ktisis.Interface.Editor.Popup;
using Ktisis.Interface.Editor.Properties.Types;
using Ktisis.Interface.Overlay;
using Ktisis.Interface.Windows.Import;
using Ktisis.Localization;
using Ktisis.Scene.Decor.Ik;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Scene.Entities.Skeleton.Constraints;
using Ktisis.Structs.Actors;
using Ktisis.Structs.Camera;

namespace Ktisis.Interface.Editor.Properties;

public class ActorPropertyList : ObjectPropertyList
{
	private readonly IEditorContext _ctx;

	private readonly GuiManager _gui;

	private readonly ConfigManager _cfg;

	private readonly LocaleManager _locale;

	private static Dictionary<GazeControl, TransformTable>? GazeTables;

	private const string IkCfgPopup = "##IkCfgPopup";

	private const string ImportOptsPopupId = "##KtisisCharaImportOptions";

	private bool IsLinked
	{
		get
		{
			return _ctx.Config.Editor.LinkedGaze;
		}
		set
		{
			_ctx.Config.Editor.LinkedGaze = value;
		}
	}

	public ActorPropertyList(IEditorContext ctx, GuiManager gui, ConfigManager cfg, LocaleManager locale)
	{
		_ctx = ctx;
		_gui = gui;
		_cfg = cfg;
		_locale = locale;
	}

	public override void Invoke(IPropertyListBuilder builder, SceneEntity entity)
	{
		SceneEntity sceneEntity = ((entity is BoneNode boneNode) ? boneNode.Pose.Parent : ((entity is BoneNodeGroup boneNodeGroup) ? boneNodeGroup.Pose.Parent : ((!(entity is EntityPose entityPose)) ? entity : entityPose.Parent)));
		SceneEntity sceneEntity2 = sceneEntity;
		ActorEntity actor = sceneEntity2 as ActorEntity;
		if (actor != null)
		{
			builder.AddHeader(Ktisis.Locale.Translate("object_edit.actor.headers.actor"), delegate
			{
				DrawActorTab(actor);
			}, 0);
			builder.AddHeader(Ktisis.Locale.Translate("object_edit.actor.headers.adv"), delegate
			{
				DrawAdvancedTab(actor);
			}, 2);
		}
	}

	private void DrawActorTab(ActorEntity actor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		ImGui.Spacing();
		if (Buttons.IconButton((FontAwesomeIcon)61508))
		{
			_ctx.Interface.OpenActorEditor(actor);
		}
		ImGui.SameLine(0f, x);
		ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.actor.chara_edit")));
		ImGui.Spacing();
		if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.actor.export")), default(Vector2)))
		{
			_ctx.Interface.OpenCharaExport(actor);
		}
		ImGui.Spacing();
		Separators.SeparatorText(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.actor.headers.import")), 0u, 0.3f, 5f, Separators.LineHeight.Bottom, ImGui.GetColorU32((ImGuiCol)24));
		ImGui.Spacing();
		CharaImportDialog orCreate = _gui.GetOrCreate<CharaImportDialog>(new object[1] { _ctx });
		((Window)orCreate).OnOpen();
		orCreate.SetTarget(actor);
		orCreate.DrawEmbed();
	}

	private void DrawAdvancedTab(ActorEntity actor)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		Separators.SeparatorText(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.actor.headers.gaze")), 0u, 0.3f, 5f, Separators.LineHeight.Bottom, ImGui.GetColorU32((ImGuiCol)24));
		DrawGazeTab(actor);
		if (TryGetEntityPose(actor, out EntityPose result) && result.IkController.GroupCount != 0)
		{
			ImGui.Spacing();
			Separators.SeparatorText(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.actor.headers.ik")), 0u, 0.3f, 5f, Separators.LineHeight.Bottom, ImGui.GetColorU32((ImGuiCol)24));
			ImGui.Spacing();
			DrawConstraintsTab(result);
		}
	}

	private unsafe void DrawGazeTab(ActorEntity actor)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		if (GazeTables == null)
		{
			GazeTables = new Dictionary<GazeControl, TransformTable>();
		}
		ActorGaze value = (actor.Gaze.HasValue ? actor.Gaze.Value : default(ActorGaze));
		bool flag = actor.GetHuman() != null;
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		bool flag2 = false;
		DisabledDisposable val = ImRaii.Disabled(_ctx.Posing.IsEnabled);
		try
		{
			DrawActorTargeting(actor);
			if (flag)
			{
				if (Buttons.IconButton((FontAwesomeIcon)(IsLinked ? 61633 : 61735)))
				{
					if (IsLinked)
					{
						GazeContainer other = value.Other;
						if (other.Gaze.Mode != GazeMode.Disabled)
						{
							other.Gaze.Mode = GazeMode.Target;
							flag2 = true;
							value.Head = other;
							value.Eyes = other;
							value.Torso = other;
							value.Other.Gaze.Mode = GazeMode.Disabled;
						}
					}
					IsLinked = !IsLinked;
				}
				ImGui.SameLine(0f, x);
				ImGui.Text(ImU8String.op_Implicit(IsLinked ? Ktisis.Locale.Translate("object_edit.actor.gaze.linked") : Ktisis.Locale.Translate("object_edit.actor.gaze.unlinked")));
				ImGui.Spacing();
			}
			bool flag3 = value.Other.Gaze.Mode == GazeMode._KtisisFollowGizmo_ || value.Eyes.Gaze.Mode == GazeMode._KtisisFollowGizmo_ || value.Head.Gaze.Mode == GazeMode._KtisisFollowGizmo_ || value.Torso.Gaze.Mode == GazeMode._KtisisFollowGizmo_;
			if (IsLinked || !flag)
			{
				flag2 |= DrawGaze(actor, ref value.Other.Gaze, GazeControl.All, flag3);
			}
			else
			{
				flag2 |= DrawGaze(actor, ref value.Eyes.Gaze, GazeControl.Eyes, flag3);
				ImGui.Spacing();
				flag2 |= DrawGaze(actor, ref value.Head.Gaze, GazeControl.Head, flag3);
				ImGui.Spacing();
				flag2 |= DrawGaze(actor, ref value.Torso.Gaze, GazeControl.Torso, flag3);
			}
			if (!flag3 || _ctx.Posing.IsEnabled)
			{
				_gui.Get<OverlayWindow>().GazeTarget = null;
			}
			if (flag2)
			{
				actor.Gaze = value;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private unsafe bool DrawGaze(ActorEntity actor, ref Gaze gaze, GazeControl type, bool anyGizmo)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		if (!GazeTables.ContainsKey(type))
		{
			GazeTables.Add(type, new TransformTable(_cfg, _locale));
		}
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(5, 1);
		((ImU8String)(ref val)).AppendLiteral("Gaze_");
		((ImU8String)(ref val)).AppendFormatted<GazeControl>(type);
		IdDisposable val2 = ImRaii.PushId(val, true);
		try
		{
			ImGuiStylePtr style = ImGui.GetStyle();
			float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
			bool flag = false;
			bool flag2 = gaze.Mode != GazeMode.Disabled;
			CharacterEx* character = (CharacterEx*)actor.Character;
			bool flag3 = gaze.Mode == GazeMode._KtisisFollowCam_;
			bool flag4 = gaze.Mode == GazeMode._KtisisFollowGizmo_;
			if (type != GazeControl.All || !flag2)
			{
				gaze.Pos = character->Gaze[type].Pos;
			}
			else
			{
				gaze.Pos = character->Gaze[GazeControl.Torso].Pos;
			}
			ImU8String val3 = new ImU8String(0, 1);
			((ImU8String)(ref val3)).AppendFormatted<GazeControl>(type);
			if (ImGui.Checkbox(val3, ref flag2))
			{
				flag = true;
				if (flag2)
				{
					gaze.Pos = GetCameraLerpFor(actor);
				}
				gaze.Mode = (flag2 ? GazeMode.Target : GazeMode.Disabled);
			}
			float num = Icons.CalcIconSize((FontAwesomeIcon)61550).X + Icons.CalcIconSize((FontAwesomeIcon)61732).X + x * 5f;
			ImGui.SameLine(0f, x);
			ImGui.SameLine(0f, 0f);
			float num2 = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - num;
			style = ImGui.GetStyle();
			ImGui.SetCursorPosX(num2 - ((ImGuiStylePtr)(ref style)).WindowPadding.X);
			ColorDisposable val4 = ImRaii.PushColor((ImGuiCol)21, ImGui.GetColorU32((ImGuiCol)23), flag3);
			try
			{
				if (Buttons.IconButtonTooltip((FontAwesomeIcon)61550, Ktisis.Locale.Translate("object_edit.actor.gaze.camera"), Vector2.Zero))
				{
					flag = true;
					flag2 = true;
					gaze.Mode = (flag3 ? GazeMode.Target : GazeMode._KtisisFollowCam_);
				}
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
			ImGui.SameLine(0f, x);
			DisabledDisposable val5 = ImRaii.Disabled(anyGizmo && !flag4);
			try
			{
				val4 = ImRaii.PushColor((ImGuiCol)21, ImGui.GetColorU32((ImGuiCol)23), flag4);
				try
				{
					if (Buttons.IconButtonTooltip((FontAwesomeIcon)61732, Ktisis.Locale.Translate("object_edit.actor.gaze.gizmo"), Vector2.Zero))
					{
						if (!flag2)
						{
							gaze.Pos = GetCameraLerpFor(actor);
						}
						flag = true;
						flag2 = true;
						gaze.Mode = (flag4 ? GazeMode.Target : GazeMode._KtisisFollowGizmo_);
					}
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val5)?.Dispose();
			}
			OverlayWindow overlayWindow = _gui.Get<OverlayWindow>();
			if (flag2 && flag4 && !_ctx.Posing.IsEnabled)
			{
				if (!overlayWindow.GazeTarget.HasValue)
				{
					overlayWindow.GazeTarget = gaze.Pos;
				}
				else if (overlayWindow.GazeManipulated)
				{
					gaze.Pos = overlayWindow.GazeTarget.Value;
					flag = true;
				}
			}
			val5 = ImRaii.Disabled(!flag2 || flag3 || flag4);
			try
			{
				flag |= GazeTables[type].DrawPosition(ref gaze.Pos, TransformTableFlags.UseAvailable);
			}
			finally
			{
				((IDisposable)val5)?.Dispose();
			}
			return flag;
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private void DrawActorTargeting(ActorEntity actor)
	{
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		uint actorGazeTarget = actor.GetActorGazeTarget();
		if (!actor.Actor.IsPcCharacter() && actorGazeTarget == 0)
		{
			return;
		}
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)61632, Ktisis.Locale.Translate("object_edit.actor.gaze.target")))
		{
			_gui.CreatePopup<ActorGazeTargetPopup>(new object[2] { _ctx, actor }).Open();
		}
		ActorEntity actorEntity = null;
		foreach (ActorEntity item in _ctx.Scene.Children.OfType<ActorEntity>().ToList())
		{
			if (item.Actor.ObjectIndex == actorGazeTarget)
			{
				actorEntity = item;
			}
		}
		ImGui.AlignTextToFramePadding();
		bool num = actorGazeTarget != 0;
		string text = (num ? (Ktisis.Locale.Translate("object_edit.actor.gaze.targeting") + ": " + ((actorEntity != null) ? actorEntity.Name : $"{Ktisis.Locale.Translate("object_edit.actor.gaze.unk")} ({actorGazeTarget})")) : Ktisis.Locale.Translate("object_edit.actor.gaze.null"));
		DisabledDisposable val = ImRaii.Disabled(!num);
		try
		{
			ImGuiStylePtr style = ImGui.GetStyle();
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			ImGui.Text(ImU8String.op_Implicit(text));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private unsafe Vector3 GetCameraLerpFor(ActorEntity actor)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		GameCameraEx* active = GameCameraEx.GetActive();
		if (active == null)
		{
			return Vector3.op_Implicit(((GameObject)actor.CsGameObject).Position);
		}
		return Vector3.Lerp(Vector3.op_Implicit(((GameObject)actor.CsGameObject).Position), active->Position, 0.5f);
	}

	private void DrawConstraintsTab(EntityPose pose)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		bool flag = pose.IkController.GetGroups().Any<(string, IIkGroup)>(((string name, IIkGroup group) p) => p.group.IsEnabled);
		DisabledDisposable val = ImRaii.Disabled(pose.IkController.GetGroups().All<(string, IIkGroup)>(((string name, IIkGroup group) p) => p.group.IsEnabled));
		try
		{
			if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("transform_edit.ik.enable_all")), default(Vector2)))
			{
				foreach (var item3 in from tuple2 in pose.IkController.GetGroups()
					where !tuple2.@group.IsEnabled
					select tuple2)
				{
					IIkGroup item = item3.Item2;
					if (TryGetGroupEndNode(pose, item, out IkEndNode node))
					{
						node.Toggle();
					}
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		ImGui.SameLine();
		val = ImRaii.Disabled(!flag);
		try
		{
			if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("transform_edit.ik.disable_all")), default(Vector2)))
			{
				foreach (var item4 in from tuple2 in pose.IkController.GetGroups()
					where tuple2.@group.IsEnabled
					select tuple2)
				{
					IIkGroup item2 = item4.Item2;
					if (TryGetGroupEndNode(pose, item2, out IkEndNode node2))
					{
						node2.Toggle();
					}
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		foreach (var (text, ikGroup) in pose.IkController.GetGroups())
		{
			if (!TryGetGroupEndNode(pose, ikGroup, out IkEndNode node3))
			{
				continue;
			}
			ImU8String val2 = new ImU8String(7, 1);
			((ImU8String)(ref val2)).AppendLiteral("IkProp_");
			((ImU8String)(ref val2)).AppendFormatted<string>(text);
			IdDisposable val3 = ImRaii.PushId(val2, true);
			try
			{
				bool isEnabled = ikGroup.IsEnabled;
				if (ImGui.Checkbox(ImU8String.op_Implicit(" " + _locale.Translate("boneCategory." + text)), ref isEnabled))
				{
					node3.Toggle();
				}
				float num = Icons.CalcIconSize((FontAwesomeIcon)62042).X + Icons.CalcIconSize((FontAwesomeIcon)61761).X + x * 5f;
				ImGui.SameLine(0f, 0f);
				float num2 = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - num;
				ImGuiStylePtr style2 = ImGui.GetStyle();
				ImGui.SetCursorPosX(num2 - ((ImGuiStylePtr)(ref style2)).WindowPadding.X);
				ColorDisposable val4 = ImRaii.PushColor((ImGuiCol)21, ImGui.GetColorU32((ImGuiCol)23), node3.IsSelected);
				try
				{
					bool flag2 = !node3.IsSelected || _ctx.Selection.Count > 1;
					if (Buttons.IconButtonTooltip((FontAwesomeIcon)62042, Ktisis.Locale.Translate("transform_edit.ik.target"), Vector2.Zero) && flag2)
					{
						node3.Select(GuiHelpers.GetSelectMode());
					}
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
				ImGui.SameLine(0f, x);
				if (Buttons.IconButtonTooltip((FontAwesomeIcon)61761, Ktisis.Locale.Translate("transform_edit.ik.edit"), Vector2.Zero))
				{
					ImGui.OpenPopup(ImU8String.op_Implicit("##IkCfgPopup"), (ImGuiPopupFlags)0);
				}
				if (!ImGui.IsPopupOpen(ImU8String.op_Implicit("##IkCfgPopup"), (ImGuiPopupFlags)0))
				{
					continue;
				}
				PopupDisposable val5 = ImRaii.Popup(ImU8String.op_Implicit("##IkCfgPopup"));
				try
				{
					if (val5.Success)
					{
						DrawIkConfig(node3);
					}
				}
				finally
				{
					((PopupDisposable)(ref val5)).Dispose();
				}
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
	}

	private void DrawIkConfig(IIkNode ik)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		bool isEnabled = ik.IsEnabled;
		if (ImGui.Checkbox(ImU8String.op_Implicit(Ktisis.Locale.Translate("transform_edit.ik.active")), ref isEnabled))
		{
			if (isEnabled)
			{
				ik.Enable();
			}
			else
			{
				ik.Disable();
			}
		}
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		if (!(ik is ICcdNode node))
		{
			if (ik is ITwoJointsNode node2)
			{
				DrawTwoJoints(node2);
			}
		}
		else
		{
			DrawCcd(node);
		}
	}

	private void DrawCcd(ICcdNode node)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		ImGui.SliderFloat(ImU8String.op_Implicit(_locale.Translate("transform_edit.ik.ccd.gain")), ref node.Group.Gain, 0f, 1f, ImU8String.op_Implicit("%.2f"), (ImGuiSliderFlags)0);
		ImGui.SliderInt(ImU8String.op_Implicit(_locale.Translate("transform_edit.ik.ccd.iterations")), ref node.Group.Iterations, 0, 60, default(ImU8String), (ImGuiSliderFlags)0);
	}

	private void DrawTwoJoints(ITwoJointsNode node)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Checkbox(ImU8String.op_Implicit(_locale.Translate("transform_edit.ik.two_joints.enforce")), ref node.Group.EnforceRotation);
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit(_locale.Translate("transform_edit.ik.two_joints.mode")));
		DrawIkMode(_locale.Translate("transform_edit.ik.two_joints.fixed"), TwoJointsMode.Fixed, node.Group);
		DrawIkMode(_locale.Translate("transform_edit.ik.two_joints.relative"), TwoJointsMode.Relative, node.Group);
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit(_locale.Translate("transform_edit.ik.two_joints.gain")));
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(13, 1);
		((ImU8String)(ref val)).AppendFormatted<string>(_locale.Translate("transform_edit.ik.two_joints.gain.shoulder"));
		((ImU8String)(ref val)).AppendLiteral("##FirstWeight");
		ImGui.SliderFloat(val, ref node.Group.FirstBoneGain, 0f, 1f, ImU8String.op_Implicit("%.2f"), (ImGuiSliderFlags)0);
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(14, 1);
		((ImU8String)(ref val2)).AppendFormatted<string>(_locale.Translate("transform_edit.ik.two_joints.gain.elbow"));
		((ImU8String)(ref val2)).AppendLiteral("##SecondWeight");
		ImGui.SliderFloat(val2, ref node.Group.SecondBoneGain, 0f, 1f, ImU8String.op_Implicit("%.2f"), (ImGuiSliderFlags)0);
		ImU8String val3 = default(ImU8String);
		((ImU8String)(ref val3))._002Ector(12, 1);
		((ImU8String)(ref val3)).AppendFormatted<string>(_locale.Translate("transform_edit.ik.two_joints.gain.hand"));
		((ImU8String)(ref val3)).AppendLiteral("##HandWeight");
		ImGui.SliderFloat(val3, ref node.Group.EndBoneGain, 0f, 1f, ImU8String.op_Implicit("%.2f"), (ImGuiSliderFlags)0);
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit(_locale.Translate("transform_edit.ik.two_joints.hinges")));
		ImGui.Spacing();
		ImGui.SliderFloat(ImU8String.op_Implicit(_locale.Translate("transform_edit.ik.two_joints.hinges.min")), ref node.Group.MinHingeAngle, -1f, 1f, ImU8String.op_Implicit("%.2f"), (ImGuiSliderFlags)0);
		ImGui.SliderFloat(ImU8String.op_Implicit(_locale.Translate("transform_edit.ik.two_joints.hinges.max")), ref node.Group.MaxHingeAngle, -1f, 1f, ImU8String.op_Implicit("%.2f"), (ImGuiSliderFlags)0);
		ImGui.SliderFloat3(ImU8String.op_Implicit(_locale.Translate("transform_edit.ik.two_joints.hinges.axis")), ref node.Group.HingeAxis, -1f, 1f, ImU8String.op_Implicit("%.2f"), (ImGuiSliderFlags)0);
		ImGui.Spacing();
	}

	private static void DrawIkMode(string label, TwoJointsMode mode, TwoJointsGroup group)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		bool flag = group.Mode == mode;
		if (ImGui.RadioButton(ImU8String.op_Implicit(label), flag))
		{
			group.Mode = mode;
		}
	}

	private static bool TryGetEntityPose(SceneEntity entity, [NotNullWhen(true)] out EntityPose? result)
	{
		EntityPose entityPose = ((entity is ActorEntity actorEntity) ? actorEntity.Pose : ((entity is BoneNodeGroup boneNodeGroup) ? boneNodeGroup.Pose : ((entity is BoneNode boneNode) ? boneNode.Pose : ((!(entity is EntityPose entityPose2)) ? null : entityPose2))));
		result = entityPose;
		return result != null;
	}

	private static bool TryGetGroupEndNode(EntityPose pose, IIkGroup group, [NotNullWhen(true)] out IkEndNode? node)
	{
		node = pose.Recurse().FirstOrDefault((SceneEntity sceneEntity) => sceneEntity is IkEndNode && sceneEntity.Parent is IkNodeGroupBase ikNodeGroupBase && ikNodeGroupBase.Group == group) as IkEndNode;
		return node != null;
	}
}
