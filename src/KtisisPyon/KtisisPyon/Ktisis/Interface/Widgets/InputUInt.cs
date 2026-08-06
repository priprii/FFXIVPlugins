using Dalamud.Bindings.ImGui;

namespace Ktisis.Interface.Widgets;

public static class InputUInt
{
	public static bool Draw(string label, ref uint value)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)value;
		bool num2 = ImGui.InputInt(ImU8String.op_Implicit(label), ref num, 1, 0, default(ImU8String), (ImGuiInputTextFlags)0);
		if (num2)
		{
			value = (uint)num;
		}
		return num2;
	}
}
