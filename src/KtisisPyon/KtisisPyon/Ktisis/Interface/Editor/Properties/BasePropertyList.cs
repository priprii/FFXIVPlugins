using Dalamud.Bindings.ImGui;
using Ktisis.Interface.Editor.Properties.Types;
using Ktisis.Scene.Entities;

namespace Ktisis.Interface.Editor.Properties;

public class BasePropertyList : ObjectPropertyList
{
	public override void Invoke(IPropertyListBuilder builder, SceneEntity entity)
	{
		builder.AddHeader("General", delegate
		{
			DrawTab(entity);
		});
	}

	private void DrawTab(SceneEntity entity)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		string name = entity.Name;
		if (ImGui.InputText(ImU8String.op_Implicit("Name"), ref name, 100, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null))
		{
			entity.Name = name;
		}
	}
}
