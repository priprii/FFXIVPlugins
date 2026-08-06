using System.IO;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using GLib.Widgets;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Editor.Properties.Types;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Utility;

namespace Ktisis.Interface.Editor.Properties;

public class ImagePropertyList : ObjectPropertyList
{
	private readonly IEditorContext _ctx;

	public ImagePropertyList(IEditorContext ctx)
	{
		_ctx = ctx;
	}

	public override void Invoke(IPropertyListBuilder builder, SceneEntity entity)
	{
		ReferenceImage img = entity as ReferenceImage;
		if (img != null)
		{
			builder.AddHeader(Ktisis.Locale.Translate("object_edit.image.header"), delegate
			{
				DrawImageTab(img);
			});
		}
	}

	private void DrawImageTab(ReferenceImage img)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Checkbox(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.image.visible")), ref img.Data.Visible);
		string fileName = Path.GetFileName(img.Data.FilePath);
		ImGui.InputText(ImU8String.op_Implicit("##RefImgPath"), ref fileName, 512, (ImGuiInputTextFlags)16400, (ImGuiInputTextCallbackDelegate)null);
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)62831, Ktisis.Locale.Translate("object_edit.image.load"), new Vector2(0f, ImGui.GetFrameHeight())))
		{
			_ctx.Interface.OpenReferenceImages(img.SetFilePath);
		}
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(15, 1);
		((ImU8String)(ref val)).AppendFormatted<string>(Ktisis.Locale.Translate("object_edit.image.opacity"));
		((ImU8String)(ref val)).AppendLiteral("##RefImgOpacity");
		ImGui.SliderFloat(val, ref img.Data.Opacity, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0);
	}
}
