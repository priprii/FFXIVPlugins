using System;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Types;
using Ktisis.Scene.Entities.World;
using Ktisis.Structs.Objects;

namespace Ktisis.Interface.Editor.Popup;

public class WorldObjectPopup(WorldObject obj, float distance, IEditorContext ctx) : KtisisPopup("##WorldObjectPopup", (ImGuiWindowFlags)0)
{
	public WorldObject WorldObj;

	protected override void OnDraw()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		WorldObj = obj;
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(14, 0);
		((ImU8String)(ref val)).AppendLiteral("Object Details");
		ImGui.Text(val);
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		DisabledDisposable val2 = ImRaii.Disabled();
		try
		{
			ImU8String val3 = new ImU8String(10, 1);
			((ImU8String)(ref val3)).AppendLiteral("\tAddress: ");
			((ImU8String)(ref val3)).AppendFormatted<nint>(obj.Address, "X");
			ImGui.Text(val3);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		ImGui.Separator();
		ImU8String val4 = default(ImU8String);
		((ImU8String)(ref val4))._002Ector(12, 1);
		((ImU8String)(ref val4)).AppendLiteral("Model path: ");
		((ImU8String)(ref val4)).AppendFormatted<string>(obj.Path);
		ImGui.Text(val4);
		ImU8String val5 = default(ImU8String);
		((ImU8String)(ref val5))._002Ector(11, 1);
		((ImU8String)(ref val5)).AppendLiteral("Distance: ");
		((ImU8String)(ref val5)).AppendFormatted<float>(distance, "0.00");
		((ImU8String)(ref val5)).AppendLiteral("y");
		ImGui.Text(val5);
		ImGui.Spacing();
		if (ImGui.Button(ImU8String.op_Implicit("Add"), default(Vector2)))
		{
			Confirm();
		}
		style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		if (ImGui.Button(ImU8String.op_Implicit("Hide"), default(Vector2)))
		{
			ConfirmAndHide();
		}
		style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		if (ImGui.Button(ImU8String.op_Implicit("Cancel"), default(Vector2)))
		{
			Close();
		}
	}

	private void Confirm()
	{
		ctx.Scene.Factory.BuildObject().SetName($"Object {ctx.Scene.Children.OfType<ObjectEntity>().Count() + 1}").SetAddress(obj.Address)
			.Add();
		Close();
	}

	private void ConfirmAndHide()
	{
		if (ctx.Scene.Factory.BuildObject().SetName($"Object {ctx.Scene.Children.OfType<ObjectEntity>().Count() + 1}").SetAddress(obj.Address)
			.Add() is ObjectEntity objectEntity)
		{
			objectEntity.ToggleHidden();
			objectEntity.Visible = false;
		}
		Close();
	}
}
