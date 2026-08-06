using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using GLib.Widgets;
using Ktisis.Data.Config.Entity;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Posing.Attachment;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Entities;

namespace Ktisis.Interface.Components.Workspace;

public class SceneDragDropHandler
{
	private readonly IEditorContext _ctx;

	private const string PayloadId = "KTISIS_SCENE_NODE";

	private SceneEntity? Source;

	private IAttachManager Manager => _ctx.Posing.Attachments;

	public SceneDragDropHandler(IEditorContext ctx)
	{
		_ctx = ctx;
	}

	public void Handle(SceneEntity entity)
	{
		HandleSource(entity);
		if (Source != null)
		{
			HandleTarget(entity);
		}
	}

	private void HandleSource(SceneEntity entity)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (!(entity is IAttachable))
		{
			return;
		}
		DragDropSourceDisposable val = ImRaii.DragDropSource((ImGuiDragDropFlags)2);
		try
		{
			if (!val.Success)
			{
				return;
			}
			ImGui.SetDragDropPayload(ImU8String.op_Implicit("KTISIS_SCENE_NODE"), ReadOnlySpan<byte>.Empty, (ImGuiCond)0);
			Source = entity;
			EntityDisplay entityDisplay = _ctx.Config.GetEntityDisplay(entity);
			ColorDisposable val2 = ImRaii.PushColor((ImGuiCol)0, entityDisplay.Color, true);
			try
			{
				FontAwesomeIcon icon = entityDisplay.Icon;
				if ((int)icon != 0)
				{
					Icons.DrawIcon(icon);
					ImGuiStylePtr style = ImGui.GetStyle();
					ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
				}
				ImGui.Text(ImU8String.op_Implicit(entity.Name));
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((DragDropSourceDisposable)(ref val)).Dispose();
		}
	}

	private unsafe void HandleTarget(SceneEntity entity)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		DragDropTargetDisposable val = ImRaii.DragDropTarget();
		try
		{
			if (val.Success && ImGui.AcceptDragDropPayload(ImU8String.op_Implicit("KTISIS_SCENE_NODE"), (ImGuiDragDropFlags)0).Handle != null)
			{
				SceneEntity source = Source;
				if (source != null)
				{
					HandlePayload(entity, source);
				}
			}
		}
		finally
		{
			((DragDropTargetDisposable)(ref val)).Dispose();
		}
	}

	private void HandlePayload(SceneEntity target, SceneEntity source)
	{
		Ktisis.Log.Info(target.Name + " accepting payload from " + source.Name);
		if (target is IAttachTarget target2 && source is IAttachable child)
		{
			Manager.Attach(child, target2);
		}
	}
}
