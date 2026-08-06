using System;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using GLib.Widgets;
using Ktisis.Data.Config.Sections;
using Ktisis.Data.Files;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Posing.Data;
using Ktisis.Interface.Components.Files;
using Ktisis.Interface.Types;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;

namespace Ktisis.Interface.Windows.Import;

public class PoseImportDialog : EntityEditWindow<ActorEntity>
{
	private readonly FileSelect<PoseFile> _select;

	public PoseImportDialog(IEditorContext ctx, FileSelect<PoseFile> select)
		: base("pose_import.title", ctx, (ImGuiWindowFlags)64, "###PoseImportDialog")
	{
		_select = select;
		select.OnOpenDialog = OnFileDialogOpen;
	}

	private void OnFileDialogOpen(FileSelect<PoseFile> sender)
	{
		Context.Scene.Overlay.ToggleCharaViewTexture(Context, base.Target);
		Context.Interface.OpenPoseFile(sender.SetFile);
	}

	public override void Draw()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		UpdateTarget();
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(1, 2);
		((ImU8String)(ref val)).AppendFormatted<string>(Ktisis.Locale.Translate("pose_import.header"));
		((ImU8String)(ref val)).AppendLiteral(" ");
		((ImU8String)(ref val)).AppendFormatted<string>(base.Target.Name);
		ImGui.Text(val);
		ImGui.Spacing();
		DrawEmbed();
	}

	public void DrawEmbed()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		((Window)this).PreDraw();
		if (!Context.IsValid)
		{
			return;
		}
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(10, 1);
		((ImU8String)(ref val)).AppendLiteral("PoseEmbed_");
		((ImU8String)(ref val)).AppendFormatted<int>(((object)this).GetHashCode(), "X");
		IdDisposable val2 = ImRaii.PushId(val, true);
		try
		{
			_select.Draw();
			ImGui.Spacing();
			DrawPoseApplication();
			ImGui.Spacing();
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private void DrawPoseApplication()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		bool isSelectBones = (from child in base.Target.Recurse()
			where child is SkeletonNode
			select child).Any((SceneEntity child) => child.IsSelected);
		DrawTransformSelect();
		ImGui.Spacing();
		DrawApplyModes(isSelectBones);
		ImGui.Spacing();
		ImGui.Spacing();
		if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("pose_import.apply")), default(Vector2)))
		{
			ApplyPoseFile(isSelectBones);
		}
	}

	private void DrawTransformSelect()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("pose_import.transforms.header")));
		FileConfig file = Context.Config.File;
		PoseTransforms importPoseTransforms = file.ImportPoseTransforms;
		bool flag = importPoseTransforms.HasFlag(PoseTransforms.Rotation);
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(15, 1);
		((ImU8String)(ref val)).AppendFormatted<string>(Ktisis.Locale.Translate("common.rotation"));
		((ImU8String)(ref val)).AppendLiteral("##PoseImportRot");
		if (ImGui.Checkbox(val, ref flag))
		{
			file.ImportPoseTransforms ^= PoseTransforms.Rotation;
		}
		ImGui.SameLine();
		bool flag2 = _select.Selected != null && _select.Selected.Path.EndsWith(".cmp");
		bool flag3 = importPoseTransforms.HasFlag(PoseTransforms.Position);
		bool flag4 = importPoseTransforms.HasFlag(PoseTransforms.Scale);
		DisabledDisposable val2 = ImRaii.Disabled(flag2);
		try
		{
			if (flag2)
			{
				flag3 = false;
				file.ImportPoseTransforms &= ~PoseTransforms.Position;
				flag4 = false;
				file.ImportPoseTransforms &= ~PoseTransforms.Scale;
			}
			ImU8String val3 = new ImU8String(15, 1);
			((ImU8String)(ref val3)).AppendFormatted<string>(Ktisis.Locale.Translate("common.position"));
			((ImU8String)(ref val3)).AppendLiteral("##PoseImportPos");
			if (ImGui.Checkbox(val3, ref flag3))
			{
				file.ImportPoseTransforms ^= PoseTransforms.Position;
			}
			ImGui.SameLine();
			ImU8String val4 = new ImU8String(17, 1);
			((ImU8String)(ref val4)).AppendFormatted<string>(Ktisis.Locale.Translate("common.scale"));
			((ImU8String)(ref val4)).AppendLiteral("##PoseImportScale");
			if (ImGui.Checkbox(val4, ref flag4))
			{
				file.ImportPoseTransforms ^= PoseTransforms.Scale;
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private void DrawApplyModes(bool isSelectBones)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("pose_import.modes.header")));
		FileConfig file = Context.Config.File;
		PoseMode importPoseModes = file.ImportPoseModes;
		bool flag = file.ImportPoseSelectedBones && isSelectBones;
		DisabledDisposable val = ImRaii.Disabled(!isSelectBones);
		try
		{
			if (ImGui.Checkbox(ImU8String.op_Implicit(Ktisis.Locale.Translate("pose_import.modes.selective_import")), ref flag))
			{
				file.ImportPoseSelectedBones = !file.ImportPoseSelectedBones;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		if (flag)
		{
			IndentDisposable val2 = ImRaii.PushIndent(1, true);
			try
			{
				ImGui.Checkbox(ImU8String.op_Implicit(Ktisis.Locale.Translate("pose_import.modes.descendants")), ref file.SelectedBonesIncludeDescendants);
				val = ImRaii.Disabled(!file.ImportPoseTransforms.HasFlag(PoseTransforms.Position));
				try
				{
					ImGui.Checkbox(ImU8String.op_Implicit(Ktisis.Locale.Translate("pose_import.modes.anchor")), ref file.AnchorPoseSelectedBones);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		if (!flag || file.SelectedBonesIncludeDescendants)
		{
			bool flag2 = importPoseModes.HasFlag(PoseMode.Body);
			ImU8String val3 = default(ImU8String);
			((ImU8String)(ref val3))._002Ector(16, 1);
			((ImU8String)(ref val3)).AppendFormatted<string>(Ktisis.Locale.Translate("common.chara_parts.body"));
			((ImU8String)(ref val3)).AppendLiteral("##PoseImportBody");
			if (ImGui.Checkbox(val3, ref flag2))
			{
				file.ImportPoseModes ^= PoseMode.Body;
			}
			ImGui.SameLine();
			bool flag3 = importPoseModes.HasFlag(PoseMode.Face);
			ImU8String val4 = default(ImU8String);
			((ImU8String)(ref val4))._002Ector(16, 1);
			((ImU8String)(ref val4)).AppendFormatted<string>(Ktisis.Locale.Translate("common.chara_parts.face"));
			((ImU8String)(ref val4)).AppendLiteral("##PoseImportFace");
			if (ImGui.Checkbox(val4, ref flag3))
			{
				file.ImportPoseModes ^= PoseMode.Face;
			}
			if (flag3 && _select.IsFileOpened && base.Target.Pose?.HasDTFace() != _select.Selected?.File.HasDTFace())
			{
				ImGui.SameLine();
				Icons.DrawIcon((FontAwesomeIcon)61553, ColorHelpers.RgbaVector4ToUint(ImGuiColors.DalamudYellow));
				if (ImGui.IsItemHovered())
				{
					ImGui.SetTooltip(ImU8String.op_Implicit(Ktisis.Locale.Translate("pose_import.modes.warn_face_compat")));
				}
			}
		}
		ImGui.Checkbox(ImU8String.op_Implicit(Ktisis.Locale.Translate("pose_import.modes.exclude_ears")), ref file.ExcludePoseEarBones);
		if (_select.IsFileOpened && Context.Posing.IsIkEnabled)
		{
			ImGui.Spacing();
			Icons.DrawIcon((FontAwesomeIcon)61553, ColorHelpers.RgbaVector4ToUint(ImGuiColors.DalamudYellow));
			ImGuiStylePtr style = ImGui.GetStyle();
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			ImGui.TextWrapped(ImU8String.op_Implicit(Ktisis.Locale.Translate("pose_import.modes.warn_ik_on")));
		}
	}

	private void ApplyPoseFile(bool isSelectBones)
	{
		PoseFile poseFile = _select.Selected?.File;
		if (poseFile != null)
		{
			EntityPose pose = base.Target.Pose;
			if (pose != null)
			{
				FileConfig file = Context.Config.File;
				bool selectedBones = isSelectBones && file.ImportPoseSelectedBones;
				bool selectedBonesIncludeDescendants = file.SelectedBonesIncludeDescendants;
				bool anchorPoseSelectedBones = file.AnchorPoseSelectedBones;
				bool excludePoseEarBones = file.ExcludePoseEarBones;
				Context.Posing.ApplyPoseFile(pose, poseFile, file.ImportPoseModes, file.ImportPoseTransforms, selectedBones, selectedBonesIncludeDescendants, anchorPoseSelectedBones, excludePoseEarBones);
			}
		}
	}
}
