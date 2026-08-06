using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using GLib.Widgets;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Editor.Properties.Types;
using Ktisis.Interface.Windows.Import;
using Ktisis.Localization;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;

namespace Ktisis.Interface.Editor.Properties;

public class PosePropertyList : ObjectPropertyList
{
	private readonly IEditorContext _ctx;

	private readonly GuiManager _gui;

	private readonly LocaleManager _locale;

	private int _partialIndex;

	private string LabelForPartial(EntityPose pose, int partialIndex)
	{
		if (partialIndex == -1)
		{
			return Ktisis.Locale.Translate("object_edit.pose.reference_labels.all");
		}
		string value = pose.GetPartialInfo(partialIndex)?.Name ?? Ktisis.Locale.Translate("object_edit.pose.reference_labels.null");
		return partialIndex switch
		{
			0 => Ktisis.Locale.Translate("object_edit.pose.reference_labels.body"), 
			1 => Ktisis.Locale.Translate("object_edit.pose.reference_labels.face"), 
			2 => Ktisis.Locale.Translate("object_edit.pose.reference_labels.hair"), 
			_ => $"{Ktisis.Locale.Translate("object_edit.pose.reference_labels.custom")} #{partialIndex} ({value})", 
		};
	}

	public PosePropertyList(IEditorContext ctx, GuiManager gui, LocaleManager locale)
	{
		_gui = gui;
		_ctx = ctx;
		_locale = locale;
		_partialIndex = -1;
	}

	public override void Invoke(IPropertyListBuilder builder, SceneEntity entity)
	{
		if (TryGetEntityPose(entity, out EntityPose pose))
		{
			builder.AddHeader(Ktisis.Locale.Translate("object_edit.pose.headers.pose"), delegate
			{
				DrawPoseTab(pose);
			}, 1);
		}
	}

	private async void DrawPoseTab(EntityPose pose)
	{
		ImGuiStylePtr style = ImGui.GetStyle();
		float spacing = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		ImGui.Checkbox(ImU8String.op_Implicit(_locale.Translate("transform_edit.transforms.parenting")), ref _ctx.Config.Gizmo.ParentBones);
		SceneEntity actor = pose.Parent;
		if (!(actor is ActorEntity))
		{
			return;
		}
		ImGui.Spacing();
		if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.pose.export")), default(Vector2)))
		{
			await _ctx.Interface.OpenPoseExport(pose);
		}
		ImGui.SameLine(0f, spacing);
		if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.pose.flip")), default(Vector2)))
		{
			await _ctx.Posing.ApplyFlipPose(pose);
		}
		ImGui.Spacing();
		if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.pose.stash")), default(Vector2)))
		{
			await _ctx.Posing.StashPose(pose);
		}
		ImGui.SameLine(0f, spacing);
		string _hint = "";
		DisabledDisposable _disabled = ImRaii.Disabled(_ctx.Posing.StashedPose == null);
		try
		{
			_hint = ((_disabled.Count > 0) ? "" : $"{Ktisis.Locale.Translate("object_edit.pose.stash.time")} {_ctx.Posing.StashedAt} {Ktisis.Locale.Translate("object_edit.pose.stash.from")} {_ctx.Posing.StashedFrom}");
			if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.pose.stash.apply")), default(Vector2)))
			{
				await _ctx.Posing.ApplyStashedPose(pose);
			}
		}
		finally
		{
			((IDisposable)_disabled)?.Dispose();
		}
		if (ImGui.IsItemHovered())
		{
			TooltipDisposable val = ImRaii.Tooltip();
			try
			{
				ImGui.Text(ImU8String.op_Implicit(_hint));
			}
			finally
			{
				((TooltipDisposable)(ref val)).Dispose();
			}
		}
		ImGui.Spacing();
		Separators.SeparatorText(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.pose.headers.reference")), 0u, 0.3f, 5f, Separators.LineHeight.Bottom, ImGui.GetColorU32((ImGuiCol)24));
		ImGui.Spacing();
		if (ImGui.BeginCombo(ImU8String.op_Implicit("##PartialSelectList"), ImU8String.op_Implicit(LabelForPartial(pose, _partialIndex)), (ImGuiComboFlags)0))
		{
			if (ImGui.Selectable(ImU8String.op_Implicit(LabelForPartial(pose, -1)), _partialIndex == -1, (ImGuiSelectableFlags)0, default(Vector2)))
			{
				_partialIndex = -1;
			}
			foreach (int partialIndex in pose.GetPartialIndices())
			{
				string text = LabelForPartial(pose, partialIndex);
				text = ((text.Length <= 60) ? text : (text.Substring(0, 60) + "..."));
				if (ImGui.Selectable(ImU8String.op_Implicit(text), _partialIndex == partialIndex, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					_partialIndex = partialIndex;
				}
			}
			ImGui.EndCombo();
		}
		ImGui.SameLine(0f, spacing);
		if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.pose.reference")), default(Vector2)))
		{
			if (_partialIndex != -1)
			{
				await _ctx.Posing.ApplyPartialReferencePose(pose, _partialIndex);
			}
			else
			{
				await _ctx.Posing.ApplyReferencePose(pose);
			}
		}
		ImGui.Spacing();
		Separators.SeparatorText(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.pose.headers.import")), 0u, 0.3f, 5f, Separators.LineHeight.Bottom, ImGui.GetColorU32((ImGuiCol)24));
		ImGui.Spacing();
		PoseImportDialog orCreate = _gui.GetOrCreate<PoseImportDialog>(new object[1] { _ctx });
		orCreate.SetTarget((ActorEntity)actor);
		orCreate.DrawEmbed();
	}

	private static bool TryGetEntityPose(SceneEntity entity, [NotNullWhen(true)] out EntityPose? result)
	{
		EntityPose entityPose = ((entity is ActorEntity actorEntity) ? actorEntity.Pose : ((entity is BoneNodeGroup boneNodeGroup) ? boneNodeGroup.Pose : ((entity is BoneNode boneNode) ? boneNode.Pose : ((!(entity is EntityPose entityPose2)) ? null : entityPose2))));
		result = entityPose;
		return result != null;
	}
}
